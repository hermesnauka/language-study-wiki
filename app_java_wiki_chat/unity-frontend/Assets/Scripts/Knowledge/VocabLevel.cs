using System.Collections.Generic;

namespace SecureLearning.Client.Knowledge
{
    /// <summary>One playable level: a term in its native language plus every cross-language translation found for it.</summary>
    public class VocabLevel
    {
        public int TermId { get; }
        public string Text { get; }
        public string Language { get; }
        public string Reading { get; }
        public IReadOnlyList<VocabTranslation> Translations { get; }

        public VocabLevel(int termId, string text, string language, string reading, IReadOnlyList<VocabTranslation> translations)
        {
            TermId = termId;
            Text = text;
            Language = language;
            Reading = reading;
            Translations = translations;
        }
    }

    public readonly struct VocabTranslation
    {
        public int TermId { get; }
        public string Text { get; }
        public string Language { get; }
        public bool IsAiGenerated { get; }

        public VocabTranslation(int termId, string text, string language, bool isAiGenerated)
        {
            TermId = termId;
            Text = text;
            Language = language;
            IsAiGenerated = isAiGenerated;
        }
    }
}
