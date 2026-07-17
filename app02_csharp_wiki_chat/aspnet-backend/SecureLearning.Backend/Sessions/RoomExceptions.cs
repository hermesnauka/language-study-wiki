namespace SecureLearning.Backend.Sessions;

/// <summary>Thrown when a third participant tries to join a room already holding 2 sessions (US-3.1).</summary>
public sealed class RoomFullException(string roomCode)
    : InvalidOperationException($"Room {roomCode} already has 2 participants");

/// <summary>Thrown when a room code does not correspond to any active room.</summary>
public sealed class RoomNotFoundException(string roomCode)
    : KeyNotFoundException($"No such room: {roomCode}");
