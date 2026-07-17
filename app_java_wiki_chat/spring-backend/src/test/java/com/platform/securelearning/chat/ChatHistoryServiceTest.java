package com.platform.securelearning.chat;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.groups.Tuple.tuple;

import com.platform.securelearning.chat.dto.ChatHistoryEntry;
import com.platform.securelearning.crypto.AesGcmEncryptionService;
import java.util.Base64;
import java.util.List;
import javax.crypto.KeyGenerator;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.orm.jpa.DataJpaTest;

/** Uses the real AES-GCM path (no mocked crypto) to verify the encrypt-on-save/decrypt-on-read round trip. */
@DataJpaTest
class ChatHistoryServiceTest {

    @Autowired
    private ChatMessageRepository repository;

    private ChatHistoryService chatHistoryService;

    @BeforeEach
    void setUp() throws Exception {
        KeyGenerator keyGenerator = KeyGenerator.getInstance("AES");
        keyGenerator.init(256);
        String testKey = Base64.getEncoder().encodeToString(keyGenerator.generateKey().getEncoded());
        chatHistoryService = new ChatHistoryService(repository, new AesGcmEncryptionService(testKey));
    }

    @Test
    void savedMessagesComeBackDecryptedInOrder() {
        chatHistoryService.save("ROOM1", "session-a", "hello");
        chatHistoryService.save("ROOM1", "session-b", "cześć");

        List<ChatHistoryEntry> history = chatHistoryService.history("ROOM1");

        assertThat(history).extracting(ChatHistoryEntry::senderSessionId, ChatHistoryEntry::text)
                .containsExactly(tuple("session-a", "hello"), tuple("session-b", "cześć"));
    }

    @Test
    void ciphertextStoredInThePersistedRowIsNeverThePlaintext() {
        chatHistoryService.save("ROOM2", "session-a", "top secret vocabulary list");

        ChatMessage stored = repository.findByRoomCodeOrderByCreatedAtAsc("ROOM2").get(0);

        assertThat(new String(stored.getCiphertext())).doesNotContain("top secret vocabulary list");
    }

    @Test
    void historyIsScopedToItsOwnRoom() {
        chatHistoryService.save("ROOM-A", "session-a", "in room A");
        chatHistoryService.save("ROOM-B", "session-a", "in room B");

        assertThat(chatHistoryService.history("ROOM-A")).extracting(ChatHistoryEntry::text)
                .containsExactly("in room A");
    }
}
