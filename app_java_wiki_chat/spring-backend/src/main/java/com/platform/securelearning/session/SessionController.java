package com.platform.securelearning.session;

import com.platform.securelearning.session.dto.SessionResponse;
import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.ResponseStatus;
import org.springframework.web.bind.annotation.RestController;

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
