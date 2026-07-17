using System.Collections.Concurrent;

namespace SecureLearning.Backend.Chat;

/// <summary>
/// Storage seam for encrypted chat turns. The in-memory implementation satisfies the
/// repo-wide "mock storage, no live network in tests" convention; a database-backed
/// implementation (EF Core) can replace it without touching <see cref="ChatHistoryService"/>.
/// </summary>
public interface IChatMessageRepository
{
    void Save(ChatMessage message);

    IReadOnlyList<ChatMessage> FindByRoomOrdered(string roomCode);
}

public sealed class InMemoryChatMessageRepository : IChatMessageRepository
{
    private readonly ConcurrentDictionary<string, List<ChatMessage>> byRoom = new();

    public void Save(ChatMessage message)
    {
        var list = byRoom.GetOrAdd(message.RoomCode, _ => new List<ChatMessage>());
        lock (list)
        {
            list.Add(message);
        }
    }

    public IReadOnlyList<ChatMessage> FindByRoomOrdered(string roomCode)
    {
        if (!byRoom.TryGetValue(roomCode, out var list))
        {
            return Array.Empty<ChatMessage>();
        }
        lock (list)
        {
            return list.OrderBy(m => m.CreatedAt).ToList();
        }
    }
}
