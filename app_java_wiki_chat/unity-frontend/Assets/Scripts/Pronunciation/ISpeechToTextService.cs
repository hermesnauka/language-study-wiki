using System.Threading.Tasks;

namespace SecureLearning.Client.Pronunciation
{
    /// <summary>
    /// FR-7: capture microphone input and transcribe it for pronunciation checks.
    /// Real transcription needs a native STT plugin or a backend speech API — see
    /// <see cref="ITextToSpeechService"/>'s docs for why this is an interim seam.
    /// </summary>
    public interface ISpeechToTextService
    {
        Task<string> ListenAndTranscribeAsync(string languageCode);
    }

    public class NullSpeechToTextService : ISpeechToTextService
    {
        public Task<string> ListenAndTranscribeAsync(string languageCode) => Task.FromResult(string.Empty);
    }
}
