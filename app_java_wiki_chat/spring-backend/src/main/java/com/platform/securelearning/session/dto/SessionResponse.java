package com.platform.securelearning.session.dto;

import com.platform.securelearning.session.Session;

public record SessionResponse(String roomCode, String sessionId) {

    public static SessionResponse from(Session session) {
        return new SessionResponse(session.roomCode(), session.sessionId());
    }
}
