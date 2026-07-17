package com.platform.securelearning.crypto;

/** Ciphertext and the nonce it was sealed with, both ready to persist as-is. */
public record EncryptedPayload(byte[] ciphertext, byte[] nonce) {
}
