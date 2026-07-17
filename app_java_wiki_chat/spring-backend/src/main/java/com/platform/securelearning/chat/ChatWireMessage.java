package com.platform.securelearning.chat;

/** JSON wire format for {@code /ws/chat/{roomCode}} in both directions. */
public record ChatWireMessage(String text, String sourceLang, String targetLang) {
}
