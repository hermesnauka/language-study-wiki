package com.platform.securelearning.chat;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Lob;
import java.time.Instant;

/**
 * One chat turn, at rest (SR-4). Only ciphertext is stored — no plaintext, and the
 * only participant reference is the opaque {@code senderSessionId} (SR-2), so a
 * database dump alone reveals neither what was said nor who said it.
 */
@Entity
public class ChatMessage {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(nullable = false)
    private String roomCode;

    @Column(nullable = false)
    private String senderSessionId;

    @Lob
    @Column(nullable = false)
    private byte[] ciphertext;

    @Column(nullable = false)
    private byte[] nonce;

    @Column(nullable = false)
    private Instant createdAt;

    protected ChatMessage() {
    }

    public ChatMessage(String roomCode, String senderSessionId, byte[] ciphertext, byte[] nonce, Instant createdAt) {
        this.roomCode = roomCode;
        this.senderSessionId = senderSessionId;
        this.ciphertext = ciphertext;
        this.nonce = nonce;
        this.createdAt = createdAt;
    }

    public Long getId() {
        return id;
    }

    public String getRoomCode() {
        return roomCode;
    }

    public String getSenderSessionId() {
        return senderSessionId;
    }

    public byte[] getCiphertext() {
        return ciphertext;
    }

    public byte[] getNonce() {
        return nonce;
    }

    public Instant getCreatedAt() {
        return createdAt;
    }
}
