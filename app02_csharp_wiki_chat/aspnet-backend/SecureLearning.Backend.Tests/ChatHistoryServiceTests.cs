using System.Security.Cryptography;
using System.Text;
using SecureLearning.Backend.Chat;
using SecureLearning.Backend.Crypto;

namespace SecureLearning.Backend.Tests;

/// <summary>Uses the real AES-GCM path (no mocked crypto) to verify the encrypt-on-save/decrypt-on-read round trip.</summary>
public class ChatHistoryServiceTests
{
    private readonly InMemoryChatMessageRepository repository = new();
    private readonly ChatHistoryService service;

    public ChatHistoryServiceTests()
    {
        string testKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        service = new ChatHistoryService(repository, new AesGcmEncryptionService(testKey));
    }

    [Fact]
    public void SavedMessagesComeBackDecryptedInOrder()
    {
        service.Save("ROOM1", "session-a", "hello");
        service.Save("ROOM1", "session-b", "cześć");

        var history = service.History("ROOM1");

        Assert.Equal(["session-a", "session-b"], history.Select(e => e.SenderSessionId));
        Assert.Equal(["hello", "cześć"], history.Select(e => e.Text));
    }

    [Fact]
    public void StoredCiphertextIsNeverThePlaintext()
    {
        service.Save("ROOM2", "session-a", "top secret vocabulary list");

        var stored = repository.FindByRoomOrdered("ROOM2").Single();

        Assert.DoesNotContain("top secret", Encoding.Latin1.GetString(stored.Ciphertext));
    }

    [Fact]
    public void HistoryIsScopedToItsOwnRoom()
    {
        service.Save("ROOM-A", "session-a", "in room A");
        service.Save("ROOM-B", "session-a", "in room B");

        Assert.Equal(["in room A"], service.History("ROOM-A").Select(e => e.Text));
    }
}
