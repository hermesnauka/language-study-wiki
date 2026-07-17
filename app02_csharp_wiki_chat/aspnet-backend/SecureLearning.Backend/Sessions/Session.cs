namespace SecureLearning.Backend.Sessions;

/// <summary>
/// An anonymous chat participant (SR-2). Carries nothing but an opaque, randomly
/// generated id and a room assignment — no username, email, or IP address is ever
/// attached, so even a compromised backend has no PII to leak.
/// </summary>
public sealed record Session(string SessionId, string RoomCode, DateTimeOffset JoinedAt);
