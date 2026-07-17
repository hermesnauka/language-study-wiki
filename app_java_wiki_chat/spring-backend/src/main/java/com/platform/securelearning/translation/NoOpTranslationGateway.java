package com.platform.securelearning.translation;

import java.util.Optional;

/** Used when no AI worker is configured — every message is relayed untranslated. */
public class NoOpTranslationGateway implements TranslationGateway {

    @Override
    public Optional<String> translate(String text, String sourceLang, String targetLang) {
        return Optional.empty();
    }
}
