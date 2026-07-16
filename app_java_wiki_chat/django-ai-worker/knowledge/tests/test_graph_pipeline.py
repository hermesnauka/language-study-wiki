from unittest.mock import patch

from django.test import TestCase

from knowledge.graph_pipeline import ingest_markdown
from knowledge.models import SymbolMapping, Term, TranslationEdge, TranslationSource, WikiDocument
from knowledge.translation import NullTranslationProvider


SAMPLE_MARKDOWN = (
    "# Zodiac Vocabulary\n\n"
    "```symbols\n"
    "m>=Scorpio\n"
    "m&=Virgo\n"
    "```\n\n"
    "## Terms\n"
    "- 猫 (neko) [ja] => cat [en]; kot [pl]\n"
    "- m> [en] => Skorpion [pl]\n"
)


class GraphPipelineTest(TestCase):

    def test_full_ingestion_persists_document_symbols_terms_and_edges(self):
        summary = ingest_markdown("zodiac.md", SAMPLE_MARKDOWN)

        document = WikiDocument.objects.get(id=summary["documentId"])
        self.assertEqual(document.path, "zodiac.md")
        self.assertEqual(summary["symbolsFound"], 2)

        self.assertEqual(
            set(SymbolMapping.objects.filter(document=document)
                .values_list("symbol", "phrase")),
            {("m>", "Scorpio"), ("m&", "Virgo")})

        # The symbol "m>" in the second vocab line was expanded to "Scorpio" before
        # vocabulary parsing, proving symbols and phrases are used interchangeably in
        # content (not just recognized in isolation).
        self.assertTrue(Term.objects.filter(text="Scorpio", language="en").exists())
        self.assertFalse(Term.objects.filter(text="m>", language="en").exists())

        cat_ja = Term.objects.get(text="猫", language="ja")
        cat_en = Term.objects.get(text="cat", language="en")
        cat_pl = Term.objects.get(text="kot", language="pl")

        # Fully connected: every pair among {猫, cat, kot} is a bidirectional edge.
        for left in (cat_ja, cat_en, cat_pl):
            for right in (cat_ja, cat_en, cat_pl):
                if left != right:
                    self.assertTrue(
                        TranslationEdge.objects.filter(from_term=left, to_term=right).exists(),
                        f"missing edge {left} -> {right}")

        edge = TranslationEdge.objects.get(from_term=cat_ja, to_term=cat_en)
        self.assertEqual(edge.source, TranslationSource.MARKDOWN)

    def test_re_ingesting_identical_content_is_idempotent(self):
        ingest_markdown("zodiac.md", SAMPLE_MARKDOWN)
        terms_before = Term.objects.count()
        edges_before = TranslationEdge.objects.count()

        # A second ingest of the same text under a different nominal path must not
        # duplicate terms/edges — get_or_create keys purely on (text, language).
        ingest_markdown("zodiac-copy.md", SAMPLE_MARKDOWN)

        self.assertEqual(Term.objects.count(), terms_before)
        self.assertEqual(TranslationEdge.objects.count(), edges_before)

    def test_auto_translate_fills_gaps_and_tags_them_as_ai_sourced(self):
        text = "- hello [en] => cześć [pl]\n"

        class FakeProvider(NullTranslationProvider):
            def translate(self, term, source_lang, target_lang):
                return f"{term}-{target_lang}"

        with patch("knowledge.graph_pipeline.get_translation_provider",
                  return_value=FakeProvider()):
            ingest_markdown("gap.md", text)

        hello_en = Term.objects.get(text="hello", language="en")
        czesc_pl = Term.objects.get(text="cześć", language="pl")
        hello_es = Term.objects.get(text="hello-es", language="es")

        # The human-declared pair stays MARKDOWN...
        declared_edge = TranslationEdge.objects.get(from_term=hello_en, to_term=czesc_pl)
        self.assertEqual(declared_edge.source, TranslationSource.MARKDOWN)

        # ...but any edge touching an AI-filled gap is tagged AI, even the one
        # directly off the human-declared source term.
        ai_edge = TranslationEdge.objects.get(from_term=hello_en, to_term=hello_es)
        self.assertEqual(ai_edge.source, TranslationSource.AI)

        # All 8 languages end up covered for this entry (en, pl declared + 6 AI-filled).
        covered_languages = set(
            TranslationEdge.objects.filter(from_term=hello_en)
            .values_list("to_term__language", flat=True)) | {"en"}
        self.assertEqual(len(covered_languages), 8)
