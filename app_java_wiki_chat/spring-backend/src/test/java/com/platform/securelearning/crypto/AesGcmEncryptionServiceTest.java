package com.platform.securelearning.crypto;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;

import java.nio.charset.StandardCharsets;
import java.util.Base64;
import javax.crypto.KeyGenerator;
import org.junit.jupiter.api.Test;

class AesGcmEncryptionServiceTest {

    private static String randomKeyBase64() throws Exception {
        KeyGenerator keyGenerator = KeyGenerator.getInstance("AES");
        keyGenerator.init(256);
        return Base64.getEncoder().encodeToString(keyGenerator.generateKey().getEncoded());
    }

    @Test
    void decryptRecoversTheOriginalPlaintext() throws Exception {
        AesGcmEncryptionService service = new AesGcmEncryptionService(randomKeyBase64());
        byte[] plaintext = "hello, world".getBytes(StandardCharsets.UTF_8);

        EncryptedPayload payload = service.encrypt(plaintext);

        assertThat(service.decrypt(payload)).isEqualTo(plaintext);
    }

    @Test
    void ciphertextNeverEqualsThePlaintext() throws Exception {
        AesGcmEncryptionService service = new AesGcmEncryptionService(randomKeyBase64());
        byte[] plaintext = "sensitive chat message".getBytes(StandardCharsets.UTF_8);

        EncryptedPayload payload = service.encrypt(plaintext);

        assertThat(payload.ciphertext()).isNotEqualTo(plaintext);
    }

    @Test
    void encryptingTheSamePlaintextTwiceProducesDifferentCiphertextAndNonce() throws Exception {
        AesGcmEncryptionService service = new AesGcmEncryptionService(randomKeyBase64());
        byte[] plaintext = "repeat me".getBytes(StandardCharsets.UTF_8);

        EncryptedPayload first = service.encrypt(plaintext);
        EncryptedPayload second = service.encrypt(plaintext);

        assertThat(first.nonce()).isNotEqualTo(second.nonce());
        assertThat(first.ciphertext()).isNotEqualTo(second.ciphertext());
    }

    @Test
    void decryptingWithTheWrongKeyFails() throws Exception {
        AesGcmEncryptionService encryptor = new AesGcmEncryptionService(randomKeyBase64());
        AesGcmEncryptionService otherKey = new AesGcmEncryptionService(randomKeyBase64());
        EncryptedPayload payload = encryptor.encrypt("secret".getBytes(StandardCharsets.UTF_8));

        assertThatThrownBy(() -> otherKey.decrypt(payload)).isInstanceOf(IllegalStateException.class);
    }

    @Test
    void generatesAnEphemeralKeyWhenNoneIsConfigured() {
        AesGcmEncryptionService service = new AesGcmEncryptionService("");
        byte[] plaintext = "no key configured".getBytes(StandardCharsets.UTF_8);

        assertThat(service.decrypt(service.encrypt(plaintext))).isEqualTo(plaintext);
    }

    @Test
    void ephemeralKeysDifferBetweenInstances() {
        AesGcmEncryptionService first = new AesGcmEncryptionService("");
        AesGcmEncryptionService second = new AesGcmEncryptionService("");
        EncryptedPayload payload = first.encrypt("x".getBytes(StandardCharsets.UTF_8));

        assertThatThrownBy(() -> second.decrypt(payload)).isInstanceOf(IllegalStateException.class);
    }
}
