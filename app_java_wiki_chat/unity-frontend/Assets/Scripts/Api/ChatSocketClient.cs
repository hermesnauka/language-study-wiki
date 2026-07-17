using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecureLearning.Client.Api.Dto;

namespace SecureLearning.Client.Api
{
    /// <summary>
    /// FR-3 / US-3.1: client for spring-backend's /ws/chat/{roomCode} gateway.
    ///
    /// Uses System.Net.WebSockets.ClientWebSocket (available under Unity's default
    /// .NET Standard 2.1 API compatibility level on Desktop/Mobile). This does NOT
    /// work in WebGL builds — WebGL requires a JavaScript-bridge WebSocket plugin
    /// (e.g. NativeWebSocket) instead; swap the transport behind this same class if
    /// a WebGL build is ever needed.
    ///
    /// <see cref="MessageReceived"/> fires from the background receive loop, NOT the
    /// Unity main thread — subscribers touching UnityEngine APIs must marshal back
    /// onto the main thread themselves (e.g. via a thread-safe queue drained in Update()).
    /// </summary>
    public class ChatSocketClient : IDisposable
    {
        public event Action<ChatWireMessageDto> MessageReceived;
        public event Action<Exception> ConnectionFaulted;

        private readonly string wsBaseUrl;
        private ClientWebSocket socket;
        private CancellationTokenSource receiveLoopCts;

        public ChatSocketClient(BackendConfig config)
        {
            wsBaseUrl = config.springBackendWsBaseUrl.TrimEnd('/');
        }

        public bool IsConnected => socket?.State == WebSocketState.Open;

        public async Task ConnectAsync(string roomCode, string sessionId)
        {
            socket = new ClientWebSocket();
            var uri = new Uri($"{wsBaseUrl}/ws/chat/{roomCode}?sessionId={Uri.EscapeDataString(sessionId)}");
            await socket.ConnectAsync(uri, CancellationToken.None);

            receiveLoopCts = new CancellationTokenSource();
            _ = ReceiveLoopAsync(receiveLoopCts.Token);
        }

        public async Task SendAsync(ChatWireMessageDto message)
        {
            string json = JsonConvert.SerializeObject(message);
            byte[] payload = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[8192];
            try
            {
                while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                    string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var message = JsonConvert.DeserializeObject<ChatWireMessageDto>(json);
                    MessageReceived?.Invoke(message);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on Dispose()/CloseAsync() — not a fault.
            }
            catch (Exception e)
            {
                ConnectionFaulted?.Invoke(e);
            }
        }

        public async Task CloseAsync()
        {
            receiveLoopCts?.Cancel();
            if (socket is { State: WebSocketState.Open })
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
            }
        }

        public void Dispose()
        {
            receiveLoopCts?.Cancel();
            socket?.Dispose();
        }
    }
}
