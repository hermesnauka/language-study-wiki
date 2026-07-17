using System.Net.WebSockets;
using System.Text;
using SecureLearning.Backend.Chat;
using SecureLearning.Backend.Crypto;
using SecureLearning.Backend.Sessions;
using SecureLearning.Backend.Translation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<RoomService>();
builder.Services.AddSingleton<IChatMessageRepository, InMemoryChatMessageRepository>();
builder.Services.AddSingleton<IEncryptionService>(sp => new AesGcmEncryptionService(
    builder.Configuration["App:Crypto:SymmetricKeyBase64"] ?? Environment.GetEnvironmentVariable("APP_CRYPTO_KEY"),
    sp.GetRequiredService<ILogger<AesGcmEncryptionService>>()));
builder.Services.AddSingleton<ChatHistoryService>();
builder.Services.AddSingleton<ChatRelay>();

string aiWorkerBaseUrl = builder.Configuration["App:AiWorker:BaseUrl"]
    ?? Environment.GetEnvironmentVariable("AI_WORKER_BASE_URL") ?? "";
if (string.IsNullOrWhiteSpace(aiWorkerBaseUrl))
{
    builder.Services.AddSingleton<ITranslationGateway, NoOpTranslationGateway>();
}
else
{
    builder.Services.AddHttpClient<ITranslationGateway, AiWorkerTranslationGateway>(
        client => client.BaseAddress = new Uri(aiWorkerBaseUrl));
}

var app = builder.Build();
app.UseWebSockets();

// --- Session management (SR-2): opaque ids only, no PII anywhere ---

app.MapPost("/api/rooms", (RoomService rooms) =>
{
    var session = rooms.CreateRoom();
    return Results.Ok(new { roomCode = session.RoomCode, sessionId = session.SessionId });
});

app.MapPost("/api/rooms/{roomCode}/join", (string roomCode, RoomService rooms) =>
{
    try
    {
        var session = rooms.Join(roomCode);
        return Results.Ok(new { roomCode = session.RoomCode, sessionId = session.SessionId });
    }
    catch (RoomNotFoundException e)
    {
        return Results.NotFound(e.Message);
    }
    catch (RoomFullException e)
    {
        return Results.Conflict(e.Message);
    }
});

// US-1.1: "context switches emit a secure state-change event to the backend" — a
// participant-only, unpersisted signal (no PII, nothing worth storing).
app.MapPost("/api/rooms/{roomCode}/language", (string roomCode, HttpRequest request, RoomService rooms) =>
{
    string? sessionId = request.Headers["X-Session-Id"].FirstOrDefault();
    if (sessionId is null || !rooms.BelongsToRoom(roomCode, sessionId))
    {
        return Results.StatusCode(StatusCodes.Status403Forbidden);
    }
    return Results.NoContent();
});

// US-3.2: securely retrieve a room's bilingual history — participants of that room only.
app.MapGet("/api/rooms/{roomCode}/history", (string roomCode, HttpRequest request,
    RoomService rooms, ChatHistoryService history) =>
{
    string? sessionId = request.Headers["X-Session-Id"].FirstOrDefault();
    if (sessionId is null || !rooms.BelongsToRoom(roomCode, sessionId))
    {
        return Results.StatusCode(StatusCodes.Status403Forbidden);
    }
    return Results.Ok(history.History(roomCode)
        .Select(e => new { senderSessionId = e.SenderSessionId, text = e.Text, createdAt = e.CreatedAt }));
});

// --- Real-time 2-party chat gateway (FR-3, US-3.1) ---

app.Map("/ws/chat/{roomCode}", async (HttpContext context, string roomCode,
    RoomService rooms, ChatRelay relay) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }
    string? sessionId = context.Request.Query["sessionId"].FirstOrDefault();
    if (sessionId is null || !rooms.BelongsToRoom(roomCode, sessionId))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }

    using var socket = await context.WebSockets.AcceptWebSocketAsync();
    relay.Peers.Register(sessionId, new WebSocketPeerSink(socket));
    try
    {
        var buffer = new byte[8192];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(buffer, context.RequestAborted);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                break;
            }
            string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
            await relay.HandleIncomingAsync(roomCode, sessionId, json, context.RequestAborted);
        }
    }
    catch (OperationCanceledException)
    {
        // Client disconnected — expected, not an error.
    }
    finally
    {
        relay.Peers.Unregister(sessionId);
    }
});

app.Run();

/// <summary>Adapts a live WebSocket to <see cref="IPeerSink"/> for <see cref="ChatRelay"/>.</summary>
internal sealed class WebSocketPeerSink(WebSocket socket) : IPeerSink
{
    public Task SendAsync(string sessionId, string payload, CancellationToken ct) =>
        socket.State == WebSocketState.Open
            ? socket.SendAsync(Encoding.UTF8.GetBytes(payload), WebSocketMessageType.Text, true, ct)
            : Task.CompletedTask;
}

// Exposes the entry point to WebApplicationFactory-based integration tests.
public partial class Program;
