package com.platform.securelearning.config;

import com.platform.securelearning.chat.ChatHistoryService;
import com.platform.securelearning.chat.ChatWebSocketHandler;
import com.platform.securelearning.session.RoomService;
import com.platform.securelearning.translation.TranslationGateway;
import org.springframework.context.annotation.Configuration;
import org.springframework.web.socket.config.annotation.EnableWebSocket;
import org.springframework.web.socket.config.annotation.WebSocketConfigurer;
import org.springframework.web.socket.config.annotation.WebSocketHandlerRegistry;

@Configuration
@EnableWebSocket
public class WebSocketConfig implements WebSocketConfigurer {

    private final RoomService roomService;
    private final TranslationGateway translationGateway;
    private final ChatHistoryService chatHistoryService;

    public WebSocketConfig(RoomService roomService, TranslationGateway translationGateway,
                            ChatHistoryService chatHistoryService) {
        this.roomService = roomService;
        this.translationGateway = translationGateway;
        this.chatHistoryService = chatHistoryService;
    }

    @Override
    public void registerWebSocketHandlers(WebSocketHandlerRegistry registry) {
        registry.addHandler(new ChatWebSocketHandler(roomService, translationGateway, chatHistoryService),
                        "/ws/chat/*")
                .setAllowedOrigins("*");
    }
}
