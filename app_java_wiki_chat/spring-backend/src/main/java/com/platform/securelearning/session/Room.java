package com.platform.securelearning.session;

import java.util.ArrayList;
import java.util.List;

/**
 * Exactly two anonymous {@link Session}s (US-3.1: "2 remote users"). Rooms live only
 * in memory — never persisted — since a room is nothing but an ephemeral pairing of
 * two opaque session ids; there is no state here worth surviving a restart, and
 * keeping it out of the database means there is nothing to leak even if the chat
 * history store were compromised.
 */
class Room {

    static final int CAPACITY = 2;

    private final String roomCode;
    private final List<Session> sessions = new ArrayList<>(CAPACITY);

    Room(String roomCode) {
        this.roomCode = roomCode;
    }

    synchronized Session addParticipant() {
        if (sessions.size() >= CAPACITY) {
            throw new RoomFullException(roomCode);
        }
        Session session = new Session(SessionIdGenerator.newId(), roomCode, java.time.Instant.now());
        sessions.add(session);
        return session;
    }

    synchronized boolean isFull() {
        return sessions.size() >= CAPACITY;
    }

    synchronized boolean contains(String sessionId) {
        return sessions.stream().anyMatch(s -> s.sessionId().equals(sessionId));
    }

    synchronized List<Session> peersOf(String sessionId) {
        return sessions.stream().filter(s -> !s.sessionId().equals(sessionId)).toList();
    }

    String roomCode() {
        return roomCode;
    }
}
