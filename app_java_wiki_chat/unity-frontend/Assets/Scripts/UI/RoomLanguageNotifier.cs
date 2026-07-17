using System.Threading.Tasks;
using SecureLearning.Client.Api;
using SecureLearning.Client.Localization;

namespace SecureLearning.Client.UI
{
    /// <summary>Adapts <see cref="RoomApiClient"/> to <see cref="ILanguageChangeNotifier"/> for one active room/session.</summary>
    public class RoomLanguageNotifier : ILanguageChangeNotifier
    {
        private readonly RoomApiClient roomApiClient;
        private readonly string roomCode;
        private readonly string sessionId;

        public RoomLanguageNotifier(RoomApiClient roomApiClient, string roomCode, string sessionId)
        {
            this.roomApiClient = roomApiClient;
            this.roomCode = roomCode;
            this.sessionId = sessionId;
        }

        public Task NotifyAsync(Language language) =>
            roomApiClient.NotifyLanguageChangeAsync(roomCode, sessionId, language.ToCode());
    }
}
