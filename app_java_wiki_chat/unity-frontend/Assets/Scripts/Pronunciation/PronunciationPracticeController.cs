using System.Threading.Tasks;

namespace SecureLearning.Client.Pronunciation
{
    /// <summary>
    /// US-1.2: press to hear the target text (TTS), then speak it back (STT) and get
    /// scored. Pure orchestration over the two service interfaces plus the scorer, so
    /// it's testable without any native platform audio.
    /// </summary>
    public class PronunciationPracticeController
    {
        private readonly ITextToSpeechService textToSpeech;
        private readonly ISpeechToTextService speechToText;

        public PronunciationPracticeController(ITextToSpeechService textToSpeech, ISpeechToTextService speechToText)
        {
            this.textToSpeech = textToSpeech;
            this.speechToText = speechToText;
        }

        public Task<bool> PlayTargetAsync(string targetText, string languageCode) =>
            textToSpeech.SpeakAsync(targetText, languageCode);

        public async Task<PronunciationAttempt> PracticeAsync(string targetText, string languageCode)
        {
            string transcript = await speechToText.ListenAndTranscribeAsync(languageCode);
            double score = PronunciationScorer.Score(targetText, transcript);
            return new PronunciationAttempt(targetText, transcript, score);
        }
    }

    public readonly struct PronunciationAttempt
    {
        public string TargetText { get; }
        public string Transcript { get; }
        public double Score { get; }

        public PronunciationAttempt(string targetText, string transcript, double score)
        {
            TargetText = targetText;
            Transcript = transcript;
            Score = score;
        }
    }
}
