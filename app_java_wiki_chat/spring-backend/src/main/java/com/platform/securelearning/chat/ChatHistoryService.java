package com.platform.securelearning.chat;

import com.platform.securelearning.chat.dto.ChatHistoryEntry;
import com.platform.securelearning.crypto.EncryptedPayload;
import com.platform.securelearning.crypto.EncryptionService;
import java.nio.charset.StandardCharsets;
import java.time.Instant;
import java.util.List;
import org.springframework.stereotype.Service;

/** Encrypts each turn before it touches the database, decrypts on the way back out (SR-4). */
@Service
public class ChatHistoryService {

    private final ChatMessageRepository repository;
    private final EncryptionService encryptionService;

    public ChatHistoryService(ChatMessageRepository repository, EncryptionService encryptionService) {
        this.repository = repository;
        this.encryptionService = encryptionService;
    }

    public void save(String roomCode, String senderSessionId, String plaintext) {
        EncryptedPayload payload = encryptionService.encrypt(plaintext.getBytes(StandardCharsets.UTF_8));
        repository.save(new ChatMessage(roomCode, senderSessionId, payload.ciphertext(), payload.nonce(), Instant.now()));
    }

    public List<ChatHistoryEntry> history(String roomCode) {
        return repository.findByRoomCodeOrderByCreatedAtAsc(roomCode).stream()
                .map(message -> new ChatHistoryEntry(
                        message.getSenderSessionId(),
                        new String(encryptionService.decrypt(
                                new EncryptedPayload(message.getCiphertext(), message.getNonce())),
                                StandardCharsets.UTF_8),
                        message.getCreatedAt()))
                .toList();
    }
}
