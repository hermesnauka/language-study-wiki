using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace SecureLearning.Backend.Sessions;

/// <summary>Pairs anonymous sessions into 2-party rooms (US-3.1); in-memory only, see <see cref="Room"/>.</summary>
public sealed class RoomService
{
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int CodeLength = 8;

    private readonly ConcurrentDictionary<string, Room> rooms = new();

    public Session CreateRoom()
    {
        var room = new Room(NewRoomCode());
        rooms[room.RoomCode] = room;
        return room.AddParticipant();
    }

    public Session Join(string roomCode)
    {
        if (!rooms.TryGetValue(roomCode, out var room))
        {
            throw new RoomNotFoundException(roomCode);
        }
        return room.AddParticipant();
    }

    public bool BelongsToRoom(string roomCode, string sessionId) =>
        rooms.TryGetValue(roomCode, out var room) && room.Contains(sessionId);

    public IReadOnlyList<Session> PeersOf(string roomCode, string sessionId)
    {
        if (!rooms.TryGetValue(roomCode, out var room))
        {
            throw new RoomNotFoundException(roomCode);
        }
        return room.PeersOf(sessionId);
    }

    private static string NewRoomCode() =>
        RandomNumberGenerator.GetString(CodeAlphabet, CodeLength);
}
