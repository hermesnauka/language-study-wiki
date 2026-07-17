package com.platform.securelearning.session;

/** Thrown when a third participant tries to join a room already holding 2 sessions (US-3.1). */
public class RoomFullException extends RuntimeException {

    public RoomFullException(String roomCode) {
        super("Room " + roomCode + " already has 2 participants");
    }
}
