package com.platform.securelearning.crypto;

/**
 * Symmetric encrypt/decrypt seam for data at rest (SR-4). {@link AesGcmEncryptionService}
 * is an interim implementation; once cpp-crypto-core's ML-KEM key exchange is wired in
 * via JNI, an implementation backed by that derived session key can replace it without
 * any change to callers ({@code chat.ChatHistoryService}).
 */
public interface EncryptionService {

    EncryptedPayload encrypt(byte[] plaintext);

    byte[] decrypt(EncryptedPayload payload);
}
