namespace SecureLearning.Backend.Chat;

/// <summary>
/// One chat turn, at rest (SR-4). Only ciphertext is stored — no plaintext, and the
/// only participant reference is the opaque sender session id (SR-2), so a store dump
/// alone reveals neither what was said nor who said it.
/// </summary>
public sealed record ChatMessage(
    string RoomCode,
    string SenderSessionId,
    byte[] Ciphertext,
    byte[] Nonce,
    DateTimeOffset CreatedAt);
