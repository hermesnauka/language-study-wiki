"""
The LangGraph ingestion pipeline: turns one raw Markdown wiki file into knowledge-graph
rows. Modeled as an explicit state machine (rather than one big function) so each step
— sanitize, extract symbols, parse vocabulary, opportunistically AI-translate gaps,
persist — is independently testable and the "Karpathy-style" ingestion story (Markdown
in, graph out) is visible directly in the graph structure.

sanitize -> extract_symbols -> parse_vocab -> auto_translate -> persist
"""
from itertools import combinations
from typing import TypedDict

from django.db import transaction
from langgraph.graph import END, START, StateGraph

from . import markdown_sandbox
from .models import Language, SymbolMapping, Term, TranslationEdge, TranslationSource, WikiDocument
from .symbols import SymbolCodec, extract_symbol_mappings
from .translation import get_translation_provider
from .vocab_parser import VocabTranslation, parse_vocab_entries


class IngestState(TypedDict, total=False):
    path: str
    raw_text: str
    sanitized_text: str
    symbol_map: dict
    expanded_text: str
    vocab_entries: list
    document: WikiDocument
    already_ingested: bool
    summary: dict


def _sanitize(state: IngestState) -> dict:
    return {"sanitized_text": markdown_sandbox.sanitize(state["raw_text"])}


def _extract_symbols(state: IngestState) -> dict:
    symbol_map = extract_symbol_mappings(state["sanitized_text"])
    codec = SymbolCodec(symbol_map)
    return {
        "symbol_map": symbol_map,
        "expanded_text": codec.expand(state["sanitized_text"]),
    }


def _parse_vocab(state: IngestState) -> dict:
    return {"vocab_entries": parse_vocab_entries(state["expanded_text"])}


def _auto_translate(state: IngestState) -> dict:
    """
    Fills in missing pairs among the 8 supported languages for each vocab entry that
    already has at least one human-declared translation, using the term's own language
    as an additional pivot. Never invents a translation for a term with zero
    human-declared anchors — that would let an unverified AI guess become the *only*
    source of truth for a brand-new word, defeating the "teacher declares vocabulary"
    model — and every AI-sourced edge is tagged TranslationSource.AI so it stays
    distinguishable from what the teacher wrote.
    """
    provider = get_translation_provider()
    enriched = []
    for entry in state["vocab_entries"]:
        known = {entry.language: entry.term}
        for translation in entry.translations:
            known[translation.language] = translation.text
        if len(known) < 2:
            enriched.append(entry)
            continue

        translations = list(entry.translations)
        covered = {entry.language} | {t.language for t in translations}
        for target_lang, _ in Language.choices:
            if target_lang in covered:
                continue
            source_lang, source_term = next(iter(known.items()))
            translated = provider.translate(source_term, source_lang, target_lang)
            if translated:
                translations.append(VocabTranslation(text=translated, language=target_lang,
                                                      source="AI"))
        enriched.append(entry.__class__(term=entry.term, language=entry.language,
                                        reading=entry.reading, translations=translations))
    return {"vocab_entries": enriched}


def _persist(state: IngestState) -> dict:
    with transaction.atomic():
        document = WikiDocument.objects.create(
            path=state["path"],
            content_hash=WikiDocument.hash_of(state["raw_text"]),
            raw_length=len(state["raw_text"]),
        )
        for symbol, phrase in state["symbol_map"].items():
            SymbolMapping.objects.create(document=document, symbol=symbol, phrase=phrase)

        terms_created = edges_created = 0
        for entry in state["vocab_entries"]:
            source_term, created = Term.objects.get_or_create(
                text=entry.term, language=entry.language,
                defaults={"reading": entry.reading, "first_seen_in": document})
            terms_created += int(created)

            # origin tracks, per term in THIS entry, whether it came from the
            # teacher's Markdown or from an AI-filled gap — independent of whether it
            # happens to be adjacent to the source term, so a later fix here can't
            # silently mislabel an AI translation as MARKDOWN just because it's
            # directly linked to the declared term.
            term_objects = {source_term.id: source_term}
            origin = {source_term.id: TranslationSource.MARKDOWN}
            for translation in entry.translations:
                target_term, created = Term.objects.get_or_create(
                    text=translation.text, language=translation.language,
                    defaults={"first_seen_in": document})
                terms_created += int(created)
                term_objects[target_term.id] = target_term
                origin[target_term.id] = (TranslationSource.AI if translation.source == "AI"
                                          else TranslationSource.MARKDOWN)

            # A fully connected cross-language matrix: every declared/AI-filled term
            # in this entry becomes mutually reachable, not just reachable from the
            # source term, so Unity can look up a translation starting from any node.
            # An edge is MARKDOWN only when both endpoints are teacher-declared;
            # touching even one AI-filled term marks the whole edge AI-sourced.
            for left, right in combinations(term_objects.values(), 2):
                source = (TranslationSource.MARKDOWN
                         if origin[left.id] == TranslationSource.MARKDOWN
                         and origin[right.id] == TranslationSource.MARKDOWN
                         else TranslationSource.AI)
                _, created = TranslationEdge.objects.get_or_create(
                    from_term=left, to_term=right, defaults={"source": source})
                edges_created += int(created)
                if created:
                    TranslationEdge.objects.get_or_create(
                        from_term=right, to_term=left, defaults={"source": source})
                    edges_created += 1

    return {
        "document": document,
        "summary": {
            "documentId": document.id,
            "path": document.path,
            "symbolsFound": len(state["symbol_map"]),
            "termsCreated": terms_created,
            "edgesCreated": edges_created,
        },
    }


def build_pipeline():
    graph = StateGraph(IngestState)
    graph.add_node("sanitize", _sanitize)
    graph.add_node("extract_symbols", _extract_symbols)
    graph.add_node("parse_vocab", _parse_vocab)
    graph.add_node("auto_translate", _auto_translate)
    graph.add_node("persist", _persist)

    graph.add_edge(START, "sanitize")
    graph.add_edge("sanitize", "extract_symbols")
    graph.add_edge("extract_symbols", "parse_vocab")
    graph.add_edge("parse_vocab", "auto_translate")
    graph.add_edge("auto_translate", "persist")
    graph.add_edge("persist", END)
    return graph.compile()


def ingest_markdown(path: str, raw_text: str) -> dict:
    """
    Runs the full pipeline over one document's raw text; returns the summary dict.
    Idempotent by content, not by path: re-ingesting byte-identical content (whether
    under the same path or a different one) is a no-op that reports the original
    document rather than raising a uniqueness error or creating duplicate terms/edges.
    """
    existing = WikiDocument.objects.filter(content_hash=WikiDocument.hash_of(raw_text)).first()
    if existing:
        return {
            "documentId": existing.id, "path": existing.path, "alreadyIngested": True,
            "symbolsFound": existing.symbol_mappings.count(),
            "termsCreated": 0, "edgesCreated": 0,
        }
    result = build_pipeline().invoke({"path": path, "raw_text": raw_text})
    return result["summary"]
