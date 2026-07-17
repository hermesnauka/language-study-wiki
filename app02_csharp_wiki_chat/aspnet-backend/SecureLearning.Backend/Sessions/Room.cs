namespace SecureLearning.Backend.Sessions;

/// <summary>
/// Exactly two anonymous <see cref="Session"/>s (US-3.1). Rooms live only in memory —
/// an ephemeral pairing of two opaque ids has no state worth surviving a restart, and
/// keeping it out of storage means nothing to leak if the history store is compromised.
/// </summary>
internal sealed class Room(string roomCode)
{
    public const int Capacity = 2;

    private readonly List<Session> sessions = new(Capacity);
    private readonly Lock guard = new();

    public string RoomCode { get; } = roomCode;

    public Session AddParticipant()
    {
        lock (guard)
        {
            if (sessions.Count >= Capacity)
            {
                throw new RoomFullException(RoomCode);
            }
            var session = new Session(Guid.NewGuid().ToString(), RoomCode, DateTimeOffset.UtcNow);
            sessions.Add(session);
            return session;
        }
    }

    public bool Contains(string sessionId)
    {
        lock (guard)
        {
            return sessions.Any(s => s.SessionId == sessionId);
        }
    }

    public IReadOnlyList<Session> PeersOf(string sessionId)
    {
        lock (guard)
        {
            return sessions.Where(s => s.SessionId != sessionId).ToList();
        }
    }
}
