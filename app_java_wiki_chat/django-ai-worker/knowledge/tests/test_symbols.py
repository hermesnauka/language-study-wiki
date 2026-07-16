from django.test import SimpleTestCase

from knowledge.markdown_sandbox import sanitize
from knowledge.symbols import SymbolCodec, extract_symbol_mappings


class SymbolsTest(SimpleTestCase):

    def test_extracts_mappings_from_fenced_symbols_block(self):
        text = sanitize(
            "# Zodiac\n\n"
            "```symbols\n"
            "m>=Scorpio\n"
            "m&=Virgo\n"
            "```\n"
            "Some prose that mentions m>= should not be parsed as a mapping.\n")

        mappings = extract_symbol_mappings(text)
        self.assertEqual(mappings, {"m>": "Scorpio", "m&": "Virgo"})

    def test_codec_expands_and_contracts_symbols_interchangeably(self):
        codec = SymbolCodec({"m>": "Scorpio", "m&": "Virgo"})

        expanded = codec.expand("Born under m> rising, with m& moon.")
        self.assertEqual(expanded, "Born under Scorpio rising, with Virgo moon.")

        contracted = codec.contract(expanded)
        self.assertEqual(contracted, "Born under m> rising, with m& moon.")

    def test_expand_prefers_longer_symbols_first_to_avoid_prefix_shadowing(self):
        codec = SymbolCodec({"m": "meter", "m>": "Scorpio"})
        self.assertEqual(codec.expand("m>"), "Scorpio")
