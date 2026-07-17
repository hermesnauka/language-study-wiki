package com.platform.securelearning.translation;

import java.util.Optional;

/**
 * Best-effort AI translation (FR-3), mirroring django-ai-worker's
 * {@code TranslationProvider}: never throws, returns {@link Optional#empty()} when
 * translation is unavailable so the chat flow always completes — a translation gap
 * just means the original text is relayed untranslated.
 */
public interface TranslationGateway {

    Optional<String> translate(String text, String sourceLang, String targetLang);
}
