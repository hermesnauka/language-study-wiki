using System.Security.Cryptography;
using SecureLearning.Backend.Chat;
using SecureLearning.Backend.Crypto;
using SecureLearning.Backend.Sessions;
using SecureLearning.Backend.Translation;

namespace SecureLearning.Backend.Tests;

public class ChatRelayTests
{
    private sealed class FixedTranslation(string? result) : ITranslationGateway
    {
        public Task<string?> TranslateAsync(string text, string sourceLang, string targetLang,
            CancellationToken ct = default) => Task.FromResult(result);
    }

    private sealed class RecordingSink : IPeerSink
    {
        public List<string> Payloads { get; } = [];

        public Task SendAsync(string sessionId, string payload, CancellationToken ct)
        {
            Payloads.Add(payload);
            return Task.CompletedTask;
        }
    }

    private readonly RoomService rooms = new();
    private readonly InMemoryChatMessageRepository repository = new();
    private readonly ChatHistoryService history;

    public ChatRelayTests()
    {
        string testKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        history = new ChatHistoryService(repository, new AesGcmEncryptionService(testKey));
    }

    [Fact]
    public async Task MessageIsTranslatedPersistedAndRelayedToThePeerOnly()
    {
        var relay = new ChatRelay(rooms, new FixedTranslation("kot"), history);
        var sender = rooms.CreateRoom();
        var peer = rooms.Join(sender.RoomCode);
        var senderSink = new RecordingSink();
        var peerSink = new RecordingSink();
        relay.Peers.Register(sender.SessionId, senderSink);
        relay.Peers.Register(peer.SessionId, peerSink);

        await relay.HandleIncomingAsync(sender.RoomCode, sender.SessionId,
            """{"text":"cat","sourceLang":"en","targetLang":"pl"}""");

        Assert.Empty(senderSink.Payloads);
        string relayed = Assert.Single(peerSink.Payloads);
        Assert.Contains("kot", relayed);
        Assert.Equal(["cat"], history.History(sender.RoomCode).Select(e => e.Text));
    }

    [Fact]
    public async Task UntranslatableMessageFallsBackToTheOriginalText()
    {
        var relay = new ChatRelay(rooms, new FixedTranslation(null), history);
        var sender = rooms.CreateRoom();
        var peer = rooms.Join(sender.RoomCode);
        var peerSink = new RecordingSink();
        relay.Peers.Register(peer.SessionId, peerSink);

        await relay.HandleIncomingAsync(sender.RoomCode, sender.SessionId,
            """{"text":"hola","sourceLang":"es","targetLang":"pl"}""");

        Assert.Contains("hola", Assert.Single(peerSink.Payloads));
    }

    [Fact]
    public async Task UnregisteredPeerIsSimplySkipped()
    {
        var relay = new ChatRelay(rooms, new FixedTranslation(null), history);
        var sender = rooms.CreateRoom();
        rooms.Join(sender.RoomCode);

        // Peer has no live socket — the message is still persisted, nothing throws.
        await relay.HandleIncomingAsync(sender.RoomCode, sender.SessionId,
            """{"text":"anyone there?","sourceLang":"en","targetLang":"pl"}""");

        Assert.Single(history.History(sender.RoomCode));
    }
}
