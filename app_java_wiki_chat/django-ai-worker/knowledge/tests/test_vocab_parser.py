from django.test import SimpleTestCase

from knowledge.vocab_parser import parse_vocab_entries


class VocabParserTest(SimpleTestCase):

    def test_parses_term_reading_language_and_translations(self):
        text = (
            "# Kanji\n\n"
            "- 猫 (neko) [ja] => cat [en]; kot [pl]\n"
            "- hello [en] => cześć [pl]; hola [es]\n")

        entries = parse_vocab_entries(text)
        self.assertEqual(len(entries), 2)

        cat_entry = entries[0]
        self.assertEqual(cat_entry.term, "猫")
        self.assertEqual(cat_entry.reading, "neko")
        self.assertEqual(cat_entry.language, "ja")
        self.assertEqual([(t.text, t.language) for t in cat_entry.translations],
                         [("cat", "en"), ("kot", "pl")])

        hello_entry = entries[1]
        self.assertEqual(hello_entry.reading, "")
        self.assertEqual([(t.text, t.language) for t in hello_entry.translations],
                         [("cześć", "pl"), ("hola", "es")])

    def test_ignores_lines_inside_fenced_code_blocks(self):
        text = (
            "```symbols\n"
            "m>=Scorpio\n"
            "```\n"
            "- dog [en] => pies [pl]\n"
            "```text\n"
            "- fake [en] => not-real [pl]\n"
            "```\n")

        entries = parse_vocab_entries(text)
        self.assertEqual([e.term for e in entries], ["dog"])

    def test_non_matching_lines_are_skipped(self):
        text = "Just a regular sentence.\n- incomplete without arrow [en]\n"
        self.assertEqual(parse_vocab_entries(text), [])
