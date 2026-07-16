"""
Pluggable AI translation used only to fill gaps the Markdown author left unfilled
(FR-2 requires all 8 languages be cross-mapped, but a teacher will rarely type out
every pair by hand). Translation is strictly best-effort and never raises: any
failure (no API key configured, network error, malformed response) just means that
gap stays unfilled until a human or a later ingestion run supplies it, so the
knowledge-graph pipeline always completes deterministically and needs no network
access or API key to run in tests (see AI_WIKI['LLM_PROVIDER_KEY'] in settings.py).
"""
import json
import logging
import urllib.request
from abc import ABC, abstractmethod

from django.conf import settings

log = logging.getLogger(__name__)

_LANGUAGE_NAMES = {
    "en": "English", "es": "Spanish", "de": "German", "fr": "French",
    "pl": "Polish", "ru": "Russian", "ja": "Japanese", "zh": "Chinese",
}


class TranslationProvider(ABC):
    @abstractmethod
    def translate(self, term: str, source_lang: str, target_lang: str) -> str | None:
        """Returns the translated term, or None if unavailable."""


class NullTranslationProvider(TranslationProvider):
    """Used whenever no LLM provider key is configured — every gap stays a gap."""

    def translate(self, term: str, source_lang: str, target_lang: str) -> str | None:
        return None


class OpenAiTranslationProvider(TranslationProvider):
    """
    Minimal OpenAI Chat Completions client (no SDK dependency): a single, tightly
    constrained prompt asking for exactly one word/phrase back, nothing else. Real
    network calls only happen when `settings.AI_WIKI['LLM_PROVIDER_KEY']` is set —
    tests substitute `_call_api` to avoid ever touching the network.
    """
    ENDPOINT = "https://api.openai.com/v1/chat/completions"
    MODEL = "gpt-4o-mini"

    def __init__(self, api_key: str):
        self._api_key = api_key

    def translate(self, term: str, source_lang: str, target_lang: str) -> str | None:
        source_name = _LANGUAGE_NAMES.get(source_lang, source_lang)
        target_name = _LANGUAGE_NAMES.get(target_lang, target_lang)
        prompt = (
            f'Translate the {source_name} word or phrase "{term}" into {target_name}. '
            "Reply with ONLY the translated word or phrase, nothing else.")
        try:
            return self._call_api(prompt)
        except Exception:
            log.warning("AI translation of %r (%s -> %s) failed",
                       term, source_lang, target_lang, exc_info=True)
            return None

    def _call_api(self, prompt: str) -> str | None:
        body = json.dumps({
            "model": self.MODEL,
            "messages": [{"role": "user", "content": prompt}],
            "temperature": 0,
        }).encode("utf-8")
        request = urllib.request.Request(
            self.ENDPOINT, data=body, method="POST",
            headers={"Content-Type": "application/json",
                     "Authorization": f"Bearer {self._api_key}"})
        with urllib.request.urlopen(request, timeout=10) as response:
            payload = json.loads(response.read().decode("utf-8"))
        content = payload["choices"][0]["message"]["content"].strip()
        return content or None


def get_translation_provider() -> TranslationProvider:
    api_key = settings.AI_WIKI.get("LLM_PROVIDER_KEY")
    if not api_key:
        return NullTranslationProvider()
    return OpenAiTranslationProvider(api_key)
