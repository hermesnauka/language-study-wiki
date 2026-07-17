package com.platform.securelearning.chat;

import java.util.List;
import org.springframework.data.jpa.repository.JpaRepository;

public interface ChatMessageRepository extends JpaRepository<ChatMessage, Long> {

    List<ChatMessage> findByRoomCodeOrderByCreatedAtAsc(String roomCode);
}
