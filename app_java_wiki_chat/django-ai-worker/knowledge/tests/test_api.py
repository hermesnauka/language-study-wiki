import json
from unittest.mock import patch

from django.test import TestCase

SAMPLE_MARKDOWN = (
    "```symbols\n"
    "m>=Scorpio\n"
    "```\n"
    "- cat [en] => kot [pl]\n"
)


class ApiTest(TestCase):

    def _post(self, path, body):
        return self.client.post(path, data=json.dumps(body), content_type="application/json")

    def test_ingest_then_query_the_graph_and_apply_symbols(self):
        ingest_response = self._post("/api/wiki/ingest",
                                     {"path": "cats.md", "content": SAMPLE_MARKDOWN})
        self.assertEqual(ingest_response.status_code, 200)
        ingest_body = ingest_response.json()
        self.assertEqual(ingest_body["symbolsFound"], 1)
        self.assertEqual(ingest_body["termsCreated"], 2)
        document_id = ingest_body["documentId"]

        # Re-ingesting identical content is reported, not duplicated.
        repeat = self._post("/api/wiki/ingest",
                            {"path": "cats-again.md", "content": SAMPLE_MARKDOWN}).json()
        self.assertTrue(repeat["alreadyIngested"])
        self.assertEqual(repeat["documentId"], document_id)

        terms = self.client.get("/api/graph/terms").json()
        self.assertEqual({(t["text"], t["language"]) for t in terms},
                         {("cat", "en"), ("kot", "pl")})

        cat_terms = self.client.get("/api/graph/terms?language=en").json()
        self.assertEqual(len(cat_terms), 1)
        cat_id = cat_terms[0]["id"]

        translations = self.client.get(f"/api/graph/terms/{cat_id}/translations").json()
        self.assertEqual(translations[0]["toTerm"]["text"], "kot")
        self.assertEqual(translations[0]["source"], "MARKDOWN")

        export = self.client.get("/api/graph/export").json()
        self.assertEqual(len(export["terms"]), 2)
        self.assertEqual(len(export["edges"]), 2)  # bidirectional

        expand = self._post("/api/symbols/apply",
                            {"documentId": document_id, "text": "m> season",
                             "direction": "expand"}).json()
        self.assertEqual(expand["result"], "Scorpio season")

        contract = self._post("/api/symbols/apply",
                              {"documentId": document_id, "text": "Scorpio season",
                               "direction": "contract"}).json()
        self.assertEqual(contract["result"], "m> season")

    def test_ingest_missing_content_is_a_400(self):
        response = self._post("/api/wiki/ingest", {"path": "empty.md"})
        self.assertEqual(response.status_code, 400)
        self.assertIn("error", response.json())

    def test_ingest_oversized_document_is_a_400(self):
        huge = "- cat [en] => kot [pl]\n" * 100_000
        response = self._post("/api/wiki/ingest", {"content": huge})
        self.assertEqual(response.status_code, 400)

    def test_symbols_apply_unknown_document_is_a_404(self):
        response = self._post("/api/symbols/apply",
                              {"documentId": 999999, "text": "x", "direction": "expand"})
        self.assertEqual(response.status_code, 404)

    def test_translations_of_unknown_term_is_a_404(self):
        response = self.client.get("/api/graph/terms/999999/translations")
        self.assertEqual(response.status_code, 404)

    def test_chat_translate_returns_the_provider_result(self):
        with patch("knowledge.views.get_translation_provider") as get_provider:
            get_provider.return_value.translate.return_value = "kot"
            response = self._post("/api/chat/translate",
                                  {"text": "cat", "sourceLang": "en", "targetLang": "pl"})
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.json(), {"translation": "kot"})

    def test_chat_translate_returns_null_when_provider_unavailable(self):
        with patch("knowledge.views.get_translation_provider") as get_provider:
            get_provider.return_value.translate.return_value = None
            response = self._post("/api/chat/translate",
                                  {"text": "cat", "sourceLang": "en", "targetLang": "pl"})
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.json(), {"translation": None})

    def test_chat_translate_missing_field_is_a_400(self):
        response = self._post("/api/chat/translate", {"text": "cat", "sourceLang": "en"})
        self.assertEqual(response.status_code, 400)
