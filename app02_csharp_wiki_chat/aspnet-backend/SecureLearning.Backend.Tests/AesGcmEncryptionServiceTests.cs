using System.Security.Cryptography;
using System.Text;
using SecureLearning.Backend.Crypto;

namespace SecureLearning.Backend.Tests;

public class AesGcmEncryptionServiceTests
{
    private static string RandomKeyBase64() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    [Fact]
    public void DecryptRecoversTheOriginalPlaintext()
    {
        var service = new AesGcmEncryptionService(RandomKeyBase64());
        byte[] plaintext = Encoding.UTF8.GetBytes("hello, world");

        var payload = service.Encrypt(plaintext);

        Assert.Equal(plaintext, service.Decrypt(payload));
    }

    [Fact]
    public void CiphertextNeverContainsThePlaintext()
    {
        var service = new AesGcmEncryptionService(RandomKeyBase64());
        byte[] plaintext = Encoding.UTF8.GetBytes("sensitive chat message");

        var payload = service.Encrypt(plaintext);

        Assert.DoesNotContain("sensitive", Encoding.Latin1.GetString(payload.Ciphertext));
    }

    [Fact]
    public void SamePlaintextEncryptsDifferentlyEachTime()
    {
        var service = new AesGcmEncryptionService(RandomKeyBase64());
        byte[] plaintext = Encoding.UTF8.GetBytes("repeat me");

        var first = service.Encrypt(plaintext);
        var second = service.Encrypt(plaintext);

        Assert.NotEqual(first.Nonce, second.Nonce);
        Assert.NotEqual(first.Ciphertext, second.Ciphertext);
    }

    [Fact]
    public void DecryptingWithTheWrongKeyFails()
    {
        var encryptor = new AesGcmEncryptionService(RandomKeyBase64());
        var otherKey = new AesGcmEncryptionService(RandomKeyBase64());
        var payload = encryptor.Encrypt(Encoding.UTF8.GetBytes("secret"));

        Assert.ThrowsAny<CryptographicException>(() => otherKey.Decrypt(payload));
    }

    [Fact]
    public void TamperedCiphertextFailsAuthentication()
    {
        var service = new AesGcmEncryptionService(RandomKeyBase64());
        var payload = service.Encrypt(Encoding.UTF8.GetBytes("integrity matters"));
        payload.Ciphertext[0] ^= 0xFF;

        Assert.ThrowsAny<CryptographicException>(() => service.Decrypt(payload));
    }

    [Fact]
    public void GeneratesAnEphemeralKeyWhenNoneConfigured()
    {
        var service = new AesGcmEncryptionService(null);
        byte[] plaintext = Encoding.UTF8.GetBytes("no key configured");

        Assert.Equal(plaintext, service.Decrypt(service.Encrypt(plaintext)));
    }
}
