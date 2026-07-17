package com.platform.securelearning.chat.dto;

import java.time.Instant;

public record ChatHistoryEntry(String senderSessionId, String text, Instant createdAt) {
}
