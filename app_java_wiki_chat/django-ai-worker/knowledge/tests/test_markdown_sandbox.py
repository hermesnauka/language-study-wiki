from django.test import SimpleTestCase, override_settings

from knowledge.markdown_sandbox import MarkdownTooLargeError, extract_fenced_blocks, sanitize


class MarkdownSandboxTest(SimpleTestCase):

    def test_strips_script_tags_and_event_handler_markup(self):
        malicious = '<script>alert(1)</script>\n<img src=x onerror="alert(2)">\nSafe text.'
        cleaned = sanitize(malicious)

        self.assertNotIn("<script>", cleaned)
        self.assertNotIn("onerror", cleaned)
        self.assertIn("Safe text.", cleaned)

    @override_settings(AI_WIKI={"INGEST_DIR": "/tmp", "LLM_PROVIDER_KEY": None,
                                "MAX_DOCUMENT_BYTES": 16})
    def test_rejects_documents_over_the_configured_size_cap(self):
        with self.assertRaises(MarkdownTooLargeError):
            sanitize("this text is definitely longer than sixteen bytes")

    def test_extracts_only_blocks_with_the_matching_info_string(self):
        text = "```symbols\na=b\n```\n```python\nprint('not a symbol block')\n```"
        self.assertEqual(extract_fenced_blocks(text, "symbols"), ["a=b"])
        self.assertEqual(extract_fenced_blocks(text, "python"),
                         ["print('not a symbol block')"])
