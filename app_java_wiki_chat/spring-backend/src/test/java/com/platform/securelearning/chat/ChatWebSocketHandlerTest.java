package com.platform.securelearning.chat;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import com.platform.securelearning.session.RoomService;
import com.platform.securelearning.session.Session;
import com.platform.securelearning.translation.TranslationGateway;
import java.net.URI;
import java.time.Instant;
import java.util.List;
import java.util.Optional;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.web.socket.CloseStatus;
import org.springframework.web.socket.TextMessage;
import org.springframework.web.socket.WebSocketSession;

class ChatWebSocketHandlerTest {

    private static final String ROOM = "ROOM1";

    private RoomService roomService;
    private TranslationGateway translationGateway;
    private ChatHistoryService chatHistoryService;
    private ChatWebSocketHandler handler;

    @BeforeEach
    void setUp() {
        roomService = mock(RoomService.class);
        translationGateway = mock(TranslationGateway.class);
        chatHistoryService = mock(ChatHistoryService.class);
        handler = new ChatWebSocketHandler(roomService, translationGateway, chatHistoryService);
    }

    private WebSocketSession socketFor(String sessionId) {
        WebSocketSession session = mock(WebSocketSession.class);
        when(session.getUri()).thenReturn(URI.create("ws://localhost/ws/chat/" + ROOM + "?sessionId=" + sessionId));
        when(session.isOpen()).thenReturn(true);
        return session;
    }

    @Test
    void connectionIsRejectedWhenTheSessionDoesNotBelongToTheRoom() throws Exception {
        WebSocketSession socket = socketFor("stranger");
        when(roomService.belongsToRoom(ROOM, "stranger")).thenReturn(false);

        handler.afterConnectionEstablished(socket);

        verify(socket).close(any(CloseStatus.class));
    }

    @Test
    void incomingMessageIsTranslatedPersistedAndRelayedToThePeerOnly() throws Exception {
        WebSocketSession senderSocket = socketFor("session-a");
        WebSocketSession peerSocket = socketFor("session-b");
        when(roomService.belongsToRoom(ROOM, "session-a")).thenReturn(true);
        when(roomService.belongsToRoom(ROOM, "session-b")).thenReturn(true);
        when(roomService.peersOf(ROOM, "session-a"))
                .thenReturn(List.of(new Session("session-b", ROOM, Instant.now())));
        when(translationGateway.translate("cat", "en", "pl")).thenReturn(Optional.of("kot"));

        handler.afterConnectionEstablished(senderSocket);
        handler.afterConnectionEstablished(peerSocket);
        handler.handleTextMessage(senderSocket,
                new TextMessage("{\"text\":\"cat\",\"sourceLang\":\"en\",\"targetLang\":\"pl\"}"));

        verify(chatHistoryService).save(ROOM, "session-a", "cat");
        verify(senderSocket, never()).sendMessage(any());
        verify(peerSocket).sendMessage(argThatContains("kot"));
    }

    @Test
    void untranslatableMessageFallsBackToTheOriginalText() throws Exception {
        WebSocketSession senderSocket = socketFor("session-a");
        when(roomService.belongsToRoom(ROOM, "session-a")).thenReturn(true);
        when(roomService.peersOf(ROOM, "session-a"))
                .thenReturn(List.of(new Session("session-b", ROOM, Instant.now())));
        when(translationGateway.translate(any(), any(), any())).thenReturn(Optional.empty());

        WebSocketSession peerSocket = socketFor("session-b");
        when(roomService.belongsToRoom(ROOM, "session-b")).thenReturn(true);
        handler.afterConnectionEstablished(senderSocket);
        handler.afterConnectionEstablished(peerSocket);

        handler.handleTextMessage(senderSocket,
                new TextMessage("{\"text\":\"hola\",\"sourceLang\":\"es\",\"targetLang\":\"pl\"}"));

        verify(peerSocket).sendMessage(argThatContains("hola"));
    }

    private static TextMessage argThatContains(String fragment) {
        return org.mockito.ArgumentMatchers.argThat(message -> message.getPayload().contains(fragment));
    }
}
