"""
User-defined symbol <-> phrase substitution.

Mappings are declared by the user as a key-value list inside a fenced ` ```symbols `
code block, one mapping per line, split on the FIRST `=` (so a symbol may itself
contain `=`-adjacent characters like `>` or `&`, e.g. `m>=Scorpio`, `m&=Virgo`).
Requiring the dedicated fenced block (rather than scanning arbitrary prose for
`key=value`-shaped lines) keeps extraction unambiguous and avoids false positives in
free-form text.

Symbols and phrases are then usable interchangeably: SymbolCodec.expand() turns
symbols into their phrases (for parsing/search/AI translation, which need real words),
and SymbolCodec.contract() turns phrases back into symbols (for compact display).
"""
import re
from dataclasses import dataclass

from .markdown_sandbox import extract_fenced_blocks

_MAPPING_LINE = re.compile(r"^\s*(\S+?)=(.+?)\s*$")


def extract_symbol_mappings(sanitized_text: str) -> dict[str, str]:
    """Parses every ` ```symbols ` block into a single {symbol: phrase} dict."""
    mappings: dict[str, str] = {}
    for block in extract_fenced_blocks(sanitized_text, "symbols"):
        for line in block.splitlines():
            if not line.strip():
                continue
            match = _MAPPING_LINE.match(line)
            if match:
                mappings[match.group(1)] = match.group(2)
    return mappings


@dataclass(frozen=True)
class SymbolCodec:
    """Bidirectional symbol/phrase substitution built from one document's mappings."""
    mappings: dict

    def expand(self, text: str) -> str:
        """Replaces every symbol occurrence with its phrase (longest symbols first,
        so one symbol can never be shadowed by a shorter symbol that is its prefix)."""
        for symbol in sorted(self.mappings, key=len, reverse=True):
            text = text.replace(symbol, self.mappings[symbol])
        return text

    def contract(self, text: str) -> str:
        """Replaces every phrase occurrence with its symbol (the inverse of expand)."""
        for symbol, phrase in sorted(self.mappings.items(),
                                     key=lambda kv: len(kv[1]), reverse=True):
            text = text.replace(phrase, symbol)
        return text
