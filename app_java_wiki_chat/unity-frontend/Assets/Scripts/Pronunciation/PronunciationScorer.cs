using System;

namespace SecureLearning.Client.Pronunciation
{
    /// <summary>
    /// US-1.2: "returns an accuracy score based on the gamification logic". Pure C#
    /// logic (no UnityEngine dependency) so it's directly unit-testable: normalized
    /// Levenshtein similarity between the target text and the STT transcript, both
    /// case-folded and trimmed so surface formatting differences don't count against
    /// the learner.
    /// </summary>
    public static class PronunciationScorer
    {
        /// <returns>A score in [0, 1] — 1.0 is a perfect match, 0.0 is completely different.</returns>
        public static double Score(string expectedText, string transcribedText)
        {
            string expected = Normalize(expectedText);
            string actual = Normalize(transcribedText);

            if (expected.Length == 0 && actual.Length == 0)
            {
                return 1.0;
            }
            if (expected.Length == 0 || actual.Length == 0)
            {
                return 0.0;
            }

            int distance = LevenshteinDistance(expected, actual);
            int maxLength = Math.Max(expected.Length, actual.Length);
            return 1.0 - (double)distance / maxLength;
        }

        private static string Normalize(string text) => (text ?? string.Empty).Trim().ToLowerInvariant();

        private static int LevenshteinDistance(string a, string b)
        {
            var previousRow = new int[b.Length + 1];
            var currentRow = new int[b.Length + 1];
            for (int j = 0; j <= b.Length; j++)
            {
                previousRow[j] = j;
            }

            for (int i = 1; i <= a.Length; i++)
            {
                currentRow[0] = i;
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    currentRow[j] = Math.Min(Math.Min(currentRow[j - 1] + 1, previousRow[j] + 1), previousRow[j - 1] + cost);
                }
                (previousRow, currentRow) = (currentRow, previousRow);
            }

            return previousRow[b.Length];
        }
    }
}
