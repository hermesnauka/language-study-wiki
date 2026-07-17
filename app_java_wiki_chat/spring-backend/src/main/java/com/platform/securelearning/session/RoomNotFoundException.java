package com.platform.securelearning.session;

/** Thrown when a room code does not correspond to any active room. */
public class RoomNotFoundException extends RuntimeException {

    public RoomNotFoundException(String roomCode) {
        super("No such room: " + roomCode);
    }
}
