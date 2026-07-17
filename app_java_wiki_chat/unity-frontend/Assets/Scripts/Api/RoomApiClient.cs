using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecureLearning.Client.Api.Dto;
using UnityEngine.Networking;

namespace SecureLearning.Client.Api
{
    /// <summary>Thin client for spring-backend's session/chat-history REST endpoints.</summary>
    public class RoomApiClient
    {
        private readonly string baseUrl;

        public RoomApiClient(BackendConfig config)
        {
            baseUrl = config.springBackendHttpBaseUrl.TrimEnd('/');
        }

        public Task<SessionResponseDto> CreateRoomAsync() =>
            PostAsync<SessionResponseDto>($"{baseUrl}/api/rooms", body: null, sessionId: null);

        public Task<SessionResponseDto> JoinRoomAsync(string roomCode) =>
            PostAsync<SessionResponseDto>($"{baseUrl}/api/rooms/{roomCode}/join", body: null, sessionId: null);

        /// <summary>US-1.1: "context switches emit a secure state-change event to the backend".</summary>
        public Task NotifyLanguageChangeAsync(string roomCode, string sessionId, string language) =>
            PostAsync<object>($"{baseUrl}/api/rooms/{roomCode}/language",
                JsonConvert.SerializeObject(new { language }), sessionId);

        public async Task<ChatHistoryEntryDto[]> GetHistoryAsync(string roomCode, string sessionId)
        {
            using var request = UnityWebRequest.Get($"{baseUrl}/api/rooms/{roomCode}/history");
            request.SetRequestHeader("X-Session-Id", sessionId);
            await request.SendWebRequest();
            ThrowIfFailed(request);
            return JsonConvert.DeserializeObject<ChatHistoryEntryDto[]>(request.downloadHandler.text);
        }

        private static async Task<T> PostAsync<T>(string url, string body, string sessionId)
        {
            using var request = new UnityWebRequest(url, "POST");
            if (body != null)
            {
                byte[] payload = Encoding.UTF8.GetBytes(body);
                request.uploadHandler = new UploadHandlerRaw(payload);
                request.SetRequestHeader("Content-Type", "application/json");
            }
            if (sessionId != null)
            {
                request.SetRequestHeader("X-Session-Id", sessionId);
            }
            request.downloadHandler = new DownloadHandlerBuffer();
            await request.SendWebRequest();
            ThrowIfFailed(request);
            string responseText = request.downloadHandler.text;
            return string.IsNullOrEmpty(responseText) ? default : JsonConvert.DeserializeObject<T>(responseText);
        }

        private static void ThrowIfFailed(UnityWebRequest request)
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException(
                    $"{request.method} {request.url} failed ({request.responseCode}): {request.error}");
            }
        }
    }
}
