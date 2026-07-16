"""
SR-5: user-uploaded Markdown wiki files must be parsed without any risk of RCE or XSS.

Two properties make this safe:
  1. No `eval`/`exec`/subprocess/template-rendering ever touches file content — every
     downstream step (symbols.py, vocab_parser.py) only does string splitting and
     regex matching over plain `str` objects, so there is no code path where uploaded
     content could be interpreted as instructions.
  2. Before that string processing happens, `sanitize` strips every HTML/XML tag from
     the raw text with `bleach` (an allow-list sanitizer, not a deny-list one — unknown
     or malformed tags are removed by default rather than passed through), so a
     Markdown file that embeds `<script>`, event handler attributes, or raw HTML can
     never have that markup reach the knowledge graph or, later, any Unity/webview
     surface that might render it.

A hard size cap defends against a single oversized file (e.g. a crafted "zip-bomb"
style Markdown or a pathological regex-backtracking input) consuming unbounded memory
or CPU during ingestion.
"""
import html

import bleach

from django.conf import settings


class MarkdownTooLargeError(ValueError):
    """Raised when a wiki file exceeds AI_WIKI['MAX_DOCUMENT_BYTES']."""


def sanitize(raw_text: str) -> str:
    """
    Strips all HTML/XML markup from `raw_text`, leaving only plain text and Markdown's
    own lightweight syntax (headings, lists, fenced code blocks) — none of which is
    ever executed, only pattern-matched.

    `bleach.clean` removes actual tag structure first (via an HTML parser, not string
    substitution) and only then HTML-entity-encodes any literal `<`/`>`/`&` left over
    in text nodes, so it is safe to `html.unescape` its output back to plain text
    afterwards: there is no remaining tag structure for those entities to reconstitute.
    """
    max_bytes = settings.AI_WIKI["MAX_DOCUMENT_BYTES"]
    encoded_length = len(raw_text.encode("utf-8"))
    if encoded_length > max_bytes:
        raise MarkdownTooLargeError(
            f"Wiki document is {encoded_length} bytes, exceeding the "
            f"{max_bytes}-byte sandbox limit")
    cleaned = bleach.clean(raw_text, tags=[], attributes={}, strip=True)
    return html.unescape(cleaned)


def extract_fenced_blocks(sanitized_text: str, info_string: str) -> list[str]:
    """
    Returns the contents of every fenced code block tagged with `info_string`
    (e.g. ` ```symbols ` ... ` ``` `). Fenced blocks are the only place structured,
    machine-readable data (symbol mappings) is expected, so free-form prose elsewhere
    in the document is never misread as data.
    """
    blocks = []
    lines = sanitized_text.splitlines()
    fence = f"```{info_string}"
    i = 0
    while i < len(lines):
        if lines[i].strip() == fence:
            body = []
            i += 1
            while i < len(lines) and lines[i].strip() != "```":
                body.append(lines[i])
                i += 1
            blocks.append("\n".join(body))
        i += 1
    return blocks
