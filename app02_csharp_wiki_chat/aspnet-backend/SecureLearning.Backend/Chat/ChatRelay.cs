using System.Collections.Concurrent;
using System.Text.Json;
using SecureLearning.Backend.Sessions;
using SecureLearning.Backend.Translation;

namespace SecureLearning.Backend.Chat;

/// <summary>JSON wire format for /ws/chat/{roomCode}, both directions — matches the Java backend's ChatWireMessage.</summary>
public sealed record ChatWireMessage(string Text, string SourceLang, string TargetLang);

/// <summary>Where a relayed message can be delivered. The WebSocket middleware adapts real sockets to this.</summary>
public interface IPeerSink
{
    Task SendAsync(string sessionId, string payload, CancellationToken ct);
}

/// <summary>
/// The 2-party chat pipeline (FR-3, US-3.1), independent of any transport so it is
/// unit-testable: translate (best-effort), persist encrypted, relay to the other
/// participant only — never echoed to the sender, never across rooms.
/// </summary>
public sealed class ChatRelay(
    RoomService roomService,
    ITranslationGateway translation,
    ChatHistoryService history)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public sealed class Registry
    {
        internal ConcurrentDictionary<string, IPeerSink> Sinks { get; } = new();

        public void Register(string sessionId, IPeerSink sink) => Sinks[sessionId] = sink;

        public void Unregister(string sessionId) => Sinks.TryRemove(sessionId, out _);
    }

    public Registry Peers { get; } = new();

    public async Task HandleIncomingAsync(string roomCode, string senderSessionId, string rawJson,
        CancellationToken ct = default)
    {
        var incoming = JsonSerializer.Deserialize<ChatWireMessage>(rawJson, JsonOptions)
            ?? throw new JsonException("Empty chat message");

        history.Save(roomCode, senderSessionId, incoming.Text);

        string outgoingText =
            await translation.TranslateAsync(incoming.Text, incoming.SourceLang, incoming.TargetLang, ct)
            ?? incoming.Text;
        string payload = JsonSerializer.Serialize(
            incoming with { Text = outgoingText }, JsonOptions);

        foreach (var peer in roomService.PeersOf(roomCode, senderSessionId))
        {
            if (Peers.Sinks.TryGetValue(peer.SessionId, out var sink))
            {
                await sink.SendAsync(peer.SessionId, payload, ct);
            }
        }
    }
}
