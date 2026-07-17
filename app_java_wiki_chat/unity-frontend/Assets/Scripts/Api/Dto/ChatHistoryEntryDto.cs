namespace SecureLearning.Client.Api.Dto
{
    /// <summary>Mirrors spring-backend's chat.dto.ChatHistoryEntry (US-3.2 export).</summary>
    public class ChatHistoryEntryDto
    {
        public string senderSessionId;
        public string text;
        public string createdAt;
    }
}
