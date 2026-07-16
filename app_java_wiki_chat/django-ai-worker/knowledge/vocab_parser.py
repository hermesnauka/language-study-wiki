"""
Parses vocabulary lines out of a sanitized Markdown wiki document (US-2.1, FR-2).

Expected line format (one entry per line, anywhere outside a fenced code block):

    - <term> (<reading>)? [<lang>] => <translation> [<lang>]; <translation> [<lang>]; ...

Examples::

    - 猫 (neko) [ja] => cat [en]; kot [pl]
    - hello [en] => cześć [pl]; hola [es]

`<lang>` is one of the two-letter codes in knowledge.models.Language. The reading is
optional and typically used for CJK romanization (romaji/pinyin). Fenced code blocks
are skipped entirely so a `m>=Scorpio`-style symbol mapping can never be misread as a
vocabulary line.
"""
import re
from dataclasses import dataclass, field

_ENTRY_LINE = re.compile(
    r"^-\s*(?P<term>.+?)\s*(?:\((?P<reading>[^)]*)\))?\s*\[(?P<lang>[a-zA-Z]{2})\]"
    r"\s*=>\s*(?P<rest>.+)$")
_TRANSLATION = re.compile(r"(?P<term>[^\[;]+?)\s*\[(?P<lang>[a-zA-Z]{2})\]")


@dataclass(frozen=True)
class VocabTranslation:
    text: str
    language: str
    #: "MARKDOWN" for translations the author wrote, "AI" for gaps the worker filled
    #: in later (see graph_pipeline._auto_translate). Kept as a plain string rather
    #: than importing knowledge.models.TranslationSource, so parsing stays free of any
    #: Django/ORM dependency.
    source: str = "MARKDOWN"


@dataclass(frozen=True)
class VocabEntry:
    term: str
    language: str
    reading: str = ""
    translations: list = field(default_factory=list)


def _strip_fenced_blocks(text: str) -> str:
    """Drops the content of every fenced code block, keeping surrounding prose."""
    kept, in_fence = [], False
    for line in text.splitlines():
        if line.strip().startswith("```"):
            in_fence = not in_fence
            continue
        if not in_fence:
            kept.append(line)
    return "\n".join(kept)


def parse_vocab_entries(sanitized_text: str) -> list:
    """Extracts every VocabEntry from the document's prose (not its code blocks)."""
    entries = []
    for line in _strip_fenced_blocks(sanitized_text).splitlines():
        match = _ENTRY_LINE.match(line.strip())
        if not match:
            continue
        translations = [
            VocabTranslation(text=t.group("term").strip(), language=t.group("lang").lower())
            for t in _TRANSLATION.finditer(match.group("rest"))
        ]
        entries.append(VocabEntry(
            term=match.group("term").strip(),
            language=match.group("lang").lower(),
            reading=(match.group("reading") or "").strip(),
            translations=translations,
        ))
    return entries
