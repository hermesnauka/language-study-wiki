package com.platform.securelearning.chat;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.platform.securelearning.session.RoomService;
import com.platform.securelearning.session.Session;
import com.platform.securelearning.translation.TranslationGateway;
import java.io.IOException;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.web.socket.CloseStatus;
import org.springframework.web.socket.TextMessage;
import org.springframework.web.socket.WebSocketSession;
import org.springframework.web.socket.handler.TextWebSocketHandler;

/**
 * The 2-party real-time chat gateway (FR-3, US-3.1). Each inbound message is
 * translated (best-effort), persisted encrypted, then relayed to the other
 * participant's socket only — never echoed back to the sender, never sent across
 * rooms.
 */
public class ChatWebSocketHandler extends TextWebSocketHandler {

    private static final Logger log = LoggerFactory.getLogger(ChatWebSocketHandler.class);

    private final RoomService roomService;
    private final TranslationGateway translationGateway;
    private final ChatHistoryService chatHistoryService;
    private final ObjectMapper objectMapper = new ObjectMapper();

    /** sessionId -> live socket, so a translated reply can be routed to the right peer. */
    private final Map<String, WebSocketSession> liveSockets = new ConcurrentHashMap<>();

    public ChatWebSocketHandler(RoomService roomService, TranslationGateway translationGateway,
                                 ChatHistoryService chatHistoryService) {
        this.roomService = roomService;
        this.translationGateway = translationGateway;
        this.chatHistoryService = chatHistoryService;
    }

    static String roomCodeOf(WebSocketSession session) {
        String path = session.getUri().getPath();
        return path.substring(path.lastIndexOf('/') + 1);
    }

    static String sessionIdOf(WebSocketSession session) {
        String query = session.getUri().getQuery();
        if (query == null) {
            return null;
        }
        for (String param : query.split("&")) {
            String[] parts = param.split("=", 2);
            if (parts.length == 2 && parts[0].equals("sessionId")) {
                return parts[1];
            }
        }
        return null;
    }

    @Override
    public void afterConnectionEstablished(WebSocketSession session) throws Exception {
        String roomCode = roomCodeOf(session);
        String sessionId = sessionIdOf(session);
        if (sessionId == null || !roomService.belongsToRoom(roomCode, sessionId)) {
            session.close(CloseStatus.POLICY_VIOLATION.withReason("Unknown session for this room"));
            return;
        }
        liveSockets.put(sessionId, session);
    }

    @Override
    protected void handleTextMessage(WebSocketSession session, TextMessage message) throws IOException {
        String roomCode = roomCodeOf(session);
        String senderSessionId = sessionIdOf(session);
        ChatWireMessage incoming = objectMapper.readValue(message.getPayload(), ChatWireMessage.class);

        chatHistoryService.save(roomCode, senderSessionId, incoming.text());

        String outgoingText = translationGateway
                .translate(incoming.text(), incoming.sourceLang(), incoming.targetLang())
                .orElse(incoming.text());
        ChatWireMessage outgoing = new ChatWireMessage(outgoingText, incoming.sourceLang(), incoming.targetLang());
        String payload = objectMapper.writeValueAsString(outgoing);

        for (Session peer : roomService.peersOf(roomCode, senderSessionId)) {
            WebSocketSession peerSocket = liveSockets.get(peer.sessionId());
            if (peerSocket != null && peerSocket.isOpen()) {
                peerSocket.sendMessage(new TextMessage(payload));
            }
        }
    }

    @Override
    public void afterConnectionClosed(WebSocketSession session, CloseStatus status) {
        liveSockets.values().remove(session);
    }
}
