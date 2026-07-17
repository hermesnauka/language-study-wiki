package com.platform.securelearning.session;

import java.util.UUID;

/** Opaque, unlinkable participant identifiers (SR-2) — no relation to any real identity. */
final class SessionIdGenerator {

    private SessionIdGenerator() {
    }

    static String newId() {
        return UUID.randomUUID().toString();
    }
}
