package com.platform.securelearning.session;

import java.time.Instant;

/**
 * An anonymous chat participant (SR-2). Deliberately carries nothing but an opaque,
 * randomly generated id and a room assignment — no username, email, or IP address is
 * ever attached to a Session, so even a compromised backend has no PII to leak.
 */
public record Session(String sessionId, String roomCode, Instant joinedAt) {
}
