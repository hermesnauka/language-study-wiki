using System.Threading.Tasks;

namespace SecureLearning.Client.Pronunciation
{
    /// <summary>
    /// FR-6: native TTS to read foreign text aloud (e.g. Japanese, Chinese). Vanilla
    /// Unity has no built-in cross-platform TTS — a real implementation wires a native
    /// plugin per platform (Android TextToSpeech, iOS AVSpeechSynthesizer, Windows
    /// SAPI). <see cref="NullTextToSpeechService"/> is the placeholder until that
    /// plugin work happens, following the same interim-seam pattern as
    /// spring-backend's EncryptionService/TranslationGateway.
    /// </summary>
    public interface ITextToSpeechService
    {
        Task<bool> SpeakAsync(string text, string languageCode);
    }

    public class NullTextToSpeechService : ITextToSpeechService
    {
        public Task<bool> SpeakAsync(string text, string languageCode) => Task.FromResult(false);
    }
}
