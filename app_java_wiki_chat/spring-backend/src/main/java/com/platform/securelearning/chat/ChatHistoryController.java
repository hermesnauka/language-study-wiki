package com.platform.securelearning.chat;

import com.platform.securelearning.chat.dto.ChatHistoryEntry;
import com.platform.securelearning.session.RoomService;
import java.util.List;
import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestHeader;
import org.springframework.web.bind.annotation.ResponseStatus;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.server.ResponseStatusException;

/** US-3.2: securely retrieve a room's bilingual history — participants of that room only. */
@RestController
public class ChatHistoryController {

    private final ChatHistoryService chatHistoryService;
    private final RoomService roomService;

    public ChatHistoryController(ChatHistoryService chatHistoryService, RoomService roomService) {
        this.chatHistoryService = chatHistoryService;
        this.roomService = roomService;
    }

    @GetMapping("/api/rooms/{roomCode}/history")
    public List<ChatHistoryEntry> history(@PathVariable String roomCode,
                                           @RequestHeader("X-Session-Id") String sessionId) {
        if (!roomService.belongsToRoom(roomCode, sessionId)) {
            throw new ResponseStatusException(HttpStatus.FORBIDDEN, "Not a participant of this room");
        }
        return chatHistoryService.history(roomCode);
    }
}
