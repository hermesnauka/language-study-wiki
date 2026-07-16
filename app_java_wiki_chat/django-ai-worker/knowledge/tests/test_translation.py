from unittest.mock import patch

from django.test import SimpleTestCase, override_settings

from knowledge.translation import (NullTranslationProvider, OpenAiTranslationProvider,
                                   get_translation_provider)


class TranslationProviderTest(SimpleTestCase):

    @override_settings(AI_WIKI={"INGEST_DIR": "/tmp", "LLM_PROVIDER_KEY": None,
                                "MAX_DOCUMENT_BYTES": 1024})
    def test_falls_back_to_null_provider_with_no_api_key_configured(self):
        provider = get_translation_provider()
        self.assertIsInstance(provider, NullTranslationProvider)
        self.assertIsNone(provider.translate("cat", "en", "pl"))

    @override_settings(AI_WIKI={"INGEST_DIR": "/tmp", "LLM_PROVIDER_KEY": "test-key",
                                "MAX_DOCUMENT_BYTES": 1024})
    def test_uses_openai_provider_when_api_key_configured(self):
        provider = get_translation_provider()
        self.assertIsInstance(provider, OpenAiTranslationProvider)

    def test_openai_provider_never_raises_and_returns_none_on_failure(self):
        provider = OpenAiTranslationProvider(api_key="test-key")
        with patch.object(provider, "_call_api", side_effect=TimeoutError("no network")):
            self.assertIsNone(provider.translate("cat", "en", "pl"))

    def test_openai_provider_returns_the_mocked_translation(self):
        provider = OpenAiTranslationProvider(api_key="test-key")
        with patch.object(provider, "_call_api", return_value="kot"):
            self.assertEqual(provider.translate("cat", "en", "pl"), "kot")
