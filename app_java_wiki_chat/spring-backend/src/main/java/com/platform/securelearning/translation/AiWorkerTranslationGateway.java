package com.platform.securelearning.translation;

import java.util.Optional;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.web.client.RestClient;

/**
 * Calls django-ai-worker's {@code POST /api/chat/translate}. Any failure (worker down,
 * network error, malformed response) is swallowed and logged — see {@link TranslationGateway}.
 */
public class AiWorkerTranslationGateway implements TranslationGateway {

    private static final Logger log = LoggerFactory.getLogger(AiWorkerTranslationGateway.class);

    private final RestClient restClient;

    public AiWorkerTranslationGateway(RestClient restClient) {
        this.restClient = restClient;
    }

    @Override
    public Optional<String> translate(String text, String sourceLang, String targetLang) {
        try {
            TranslateResponse response = restClient.post()
                    .uri("/api/chat/translate")
                    .body(new TranslateRequest(text, sourceLang, targetLang))
                    .retrieve()
                    .body(TranslateResponse.class);
            return response == null ? Optional.empty() : Optional.ofNullable(response.translation());
        } catch (Exception e) {
            log.warn("AI worker translation of {} -> {} failed", sourceLang, targetLang, e);
            return Optional.empty();
        }
    }
}
