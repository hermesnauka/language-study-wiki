using System.Security.Cryptography;

namespace SecureLearning.Backend.Crypto;

/// <summary>
/// AES-256-GCM at-rest encryption — an interim placeholder standing in for the
/// post-quantum symmetric key cpp-crypto-core's ML-KEM exchange will eventually derive.
/// AES-GCM gives confidentiality/integrity for stored data, not quantum resistance.
/// Key comes from configuration (<c>App:Crypto:SymmetricKeyBase64</c>, env
/// <c>APP_CRYPTO_KEY</c>); when unset a random process-lifetime key is generated,
/// which is fine for dev/tests but makes history unreadable after a restart.
/// </summary>
public sealed class AesGcmEncryptionService : IEncryptionService
{
    private const int NonceLengthBytes = 12;
    private const int TagLengthBytes = 16;
    private const int KeyLengthBytes = 32;

    private readonly byte[] key;

    public AesGcmEncryptionService(string? configuredKeyBase64, ILogger<AesGcmEncryptionService>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(configuredKeyBase64))
        {
            logger?.LogWarning(
                "No App:Crypto:SymmetricKeyBase64 configured — generating an ephemeral AES-256 key "
                + "for this process only. Chat history will be unreadable after restart. This is a "
                + "dev-only placeholder pending the cpp-crypto-core PQC bridge.");
            key = RandomNumberGenerator.GetBytes(KeyLengthBytes);
        }
        else
        {
            key = Convert.FromBase64String(configuredKeyBase64);
        }
    }

    public EncryptedPayload Encrypt(byte[] plaintext)
    {
        byte[] nonce = RandomNumberGenerator.GetBytes(NonceLengthBytes);
        byte[] ciphertextWithTag = new byte[plaintext.Length + TagLengthBytes];
        using var aes = new AesGcm(key, TagLengthBytes);
        // Tag is appended to the ciphertext so EncryptedPayload stays a two-field
        // record matching the Java backend's storage shape.
        aes.Encrypt(nonce, plaintext,
            ciphertextWithTag.AsSpan(0, plaintext.Length),
            ciphertextWithTag.AsSpan(plaintext.Length, TagLengthBytes));
        return new EncryptedPayload(ciphertextWithTag, nonce);
    }

    public byte[] Decrypt(EncryptedPayload payload)
    {
        int plaintextLength = payload.Ciphertext.Length - TagLengthBytes;
        if (plaintextLength < 0)
        {
            throw new CryptographicException("Ciphertext shorter than its authentication tag — corrupted data");
        }
        byte[] plaintext = new byte[plaintextLength];
        using var aes = new AesGcm(key, TagLengthBytes);
        aes.Decrypt(payload.Nonce,
            payload.Ciphertext.AsSpan(0, plaintextLength),
            payload.Ciphertext.AsSpan(plaintextLength, TagLengthBytes),
            plaintext);
        return plaintext;
    }
}
