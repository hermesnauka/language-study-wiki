package com.platform.securelearning.crypto;

import java.security.GeneralSecurityException;
import java.security.SecureRandom;
import java.util.Base64;
import javax.crypto.Cipher;
import javax.crypto.KeyGenerator;
import javax.crypto.SecretKey;
import javax.crypto.spec.GCMParameterSpec;
import javax.crypto.spec.SecretKeySpec;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

/**
 * AES-256-GCM at-rest encryption. This is an interim placeholder standing in for the
 * post-quantum symmetric key that cpp-crypto-core's ML-KEM exchange will eventually
 * derive (see {@link EncryptionService}); AES-GCM itself gives no quantum resistance,
 * only confidentiality/integrity for data already stored today.
 */
@Service
public class AesGcmEncryptionService implements EncryptionService {

    private static final Logger log = LoggerFactory.getLogger(AesGcmEncryptionService.class);
    private static final String ALGORITHM = "AES/GCM/NoPadding";
    private static final int GCM_TAG_LENGTH_BITS = 128;
    private static final int NONCE_LENGTH_BYTES = 12;

    private final SecretKey key;
    private final SecureRandom random = new SecureRandom();

    public AesGcmEncryptionService(@Value("${app.crypto.symmetric-key-base64:}") String configuredKeyBase64) {
        this.key = configuredKeyBase64.isBlank() ? generateEphemeralKey() : decodeKey(configuredKeyBase64);
    }

    private SecretKey generateEphemeralKey() {
        log.warn("No app.crypto.symmetric-key-base64 configured — generating an ephemeral "
                + "AES-256 key for this process only. Chat history will be unreadable after "
                + "restart. This is a dev-only placeholder pending the cpp-crypto-core PQC bridge.");
        try {
            KeyGenerator keyGenerator = KeyGenerator.getInstance("AES");
            keyGenerator.init(256);
            return keyGenerator.generateKey();
        } catch (GeneralSecurityException e) {
            throw new IllegalStateException("AES key generation is unavailable", e);
        }
    }

    private SecretKey decodeKey(String base64Key) {
        return new SecretKeySpec(Base64.getDecoder().decode(base64Key), "AES");
    }

    @Override
    public EncryptedPayload encrypt(byte[] plaintext) {
        try {
            byte[] nonce = new byte[NONCE_LENGTH_BYTES];
            random.nextBytes(nonce);
            Cipher cipher = Cipher.getInstance(ALGORITHM);
            cipher.init(Cipher.ENCRYPT_MODE, key, new GCMParameterSpec(GCM_TAG_LENGTH_BITS, nonce));
            byte[] ciphertext = cipher.doFinal(plaintext);
            return new EncryptedPayload(ciphertext, nonce);
        } catch (GeneralSecurityException e) {
            throw new IllegalStateException("Encryption failed", e);
        }
    }

    @Override
    public byte[] decrypt(EncryptedPayload payload) {
        try {
            Cipher cipher = Cipher.getInstance(ALGORITHM);
            cipher.init(Cipher.DECRYPT_MODE, key, new GCMParameterSpec(GCM_TAG_LENGTH_BITS, payload.nonce()));
            return cipher.doFinal(payload.ciphertext());
        } catch (GeneralSecurityException e) {
            throw new IllegalStateException("Decryption failed — wrong key or corrupted data", e);
        }
    }
}
