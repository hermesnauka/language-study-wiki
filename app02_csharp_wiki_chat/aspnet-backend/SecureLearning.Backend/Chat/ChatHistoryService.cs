using System.Text;
using SecureLearning.Backend.Crypto;

namespace SecureLearning.Backend.Chat;

public sealed record ChatHistoryEntry(string SenderSessionId, string Text, DateTimeOffset CreatedAt);

/// <summary>Encrypts each turn before it reaches storage, decrypts on the way back out (SR-4).</summary>
public sealed class ChatHistoryService(IChatMessageRepository repository, IEncryptionService encryption)
{
    public void Save(string roomCode, string senderSessionId, string plaintext)
    {
        var payload = encryption.Encrypt(Encoding.UTF8.GetBytes(plaintext));
        repository.Save(new ChatMessage(roomCode, senderSessionId, payload.Ciphertext,
            payload.Nonce, DateTimeOffset.UtcNow));
    }

    public IReadOnlyList<ChatHistoryEntry> History(string roomCode) =>
        repository.FindByRoomOrdered(roomCode)
            .Select(m => new ChatHistoryEntry(
                m.SenderSessionId,
                Encoding.UTF8.GetString(encryption.Decrypt(new EncryptedPayload(m.Ciphertext, m.Nonce))),
                m.CreatedAt))
            .ToList();
}
