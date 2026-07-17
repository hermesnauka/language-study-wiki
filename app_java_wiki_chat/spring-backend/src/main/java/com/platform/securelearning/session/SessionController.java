package com.platform.securelearning.session;

import com.platform.securelearning.session.dto.LanguageChangeRequest;
import com.platform.securelearning.session.dto.SessionResponse;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestHeader;
import org.springframework.web.bind.annotation.ResponseStatus;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.server.ResponseStatusException;

@RestController
public class SessionController {

    private final RoomService roomService;

    public SessionController(RoomService roomService) {
        this.roomService = roomService;
    }

    @PostMapping("/api/rooms")
    public SessionResponse createRoom() {
        return SessionResponse.from(roomService.createRoom());
    }

    @PostMapping("/api/rooms/{roomCode}/join")
    public SessionResponse join(@PathVariable String roomCode) {
        return SessionResponse.from(roomService.join(roomCode));
    }

    /** US-1.1: "context switches emit a secure state-change event to the backend" — a
     * participant-only, unpersisted signal (no PII, nothing worth storing). */
    @PostMapping("/api/rooms/{roomCode}/language")
    public ResponseEntity<Void> changeLanguage(@PathVariable String roomCode,
                                                @RequestHeader("X-Session-Id") String sessionId,
                                                @RequestBody LanguageChangeRequest request) {
        if (!roomService.belongsToRoom(roomCode, sessionId)) {
            throw new ResponseStatusException(HttpStatus.FORBIDDEN, "Not a participant of this room");
        }
        return ResponseEntity.noContent().build();
    }

    @ExceptionHandler(RoomNotFoundException.class)
    @ResponseStatus(HttpStatus.NOT_FOUND)
    public String handleNotFound(RoomNotFoundException e) {
        return e.getMessage();
    }

    @ExceptionHandler(RoomFullException.class)
    @ResponseStatus(HttpStatus.CONFLICT)
    public String handleFull(RoomFullException e) {
        return e.getMessage();
    }
}
