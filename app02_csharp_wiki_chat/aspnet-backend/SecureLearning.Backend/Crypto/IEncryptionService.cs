namespace SecureLearning.Backend.Crypto;

/// <summary>Ciphertext and the nonce it was sealed with, both ready to persist as-is.</summary>
public sealed record EncryptedPayload(byte[] Ciphertext, byte[] Nonce);

/// <summary>
/// Symmetric encrypt/decrypt seam for data at rest (SR-4). <see cref="AesGcmEncryptionService"/>
/// is the interim implementation; once cpp-crypto-core's ML-KEM exchange is bridged in
/// (P/Invoke), an implementation keyed by that derived session secret replaces it
/// without any change to callers.
/// </summary>
public interface IEncryptionService
{
    EncryptedPayload Encrypt(byte[] plaintext);

    byte[] Decrypt(EncryptedPayload payload);
}
