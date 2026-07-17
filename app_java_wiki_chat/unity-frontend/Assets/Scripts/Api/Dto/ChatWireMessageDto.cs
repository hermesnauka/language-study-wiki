namespace SecureLearning.Client.Api.Dto
{
    /// <summary>JSON wire format for spring-backend's /ws/chat/{roomCode}, both directions.</summary>
    public class ChatWireMessageDto
    {
        public string text;
        public string sourceLang;
        public string targetLang;
    }
}
