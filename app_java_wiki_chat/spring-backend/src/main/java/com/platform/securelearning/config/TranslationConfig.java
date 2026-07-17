package com.platform.securelearning.config;

import com.platform.securelearning.translation.AiWorkerTranslationGateway;
import com.platform.securelearning.translation.NoOpTranslationGateway;
import com.platform.securelearning.translation.TranslationGateway;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.web.client.RestClient;

@Configuration
public class TranslationConfig {

    @Bean
    public TranslationGateway translationGateway(@Value("${app.ai-worker.base-url:}") String baseUrl) {
        if (baseUrl.isBlank()) {
            return new NoOpTranslationGateway();
        }
        return new AiWorkerTranslationGateway(RestClient.builder().baseUrl(baseUrl).build());
    }
}
