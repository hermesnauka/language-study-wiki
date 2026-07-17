using NUnit.Framework;
using SecureLearning.Client.Pronunciation;

namespace SecureLearning.Client.Tests
{
    public class PronunciationScorerTests
    {
        [Test]
        public void IdenticalTextScoresOne()
        {
            Assert.That(PronunciationScorer.Score("hello", "hello"), Is.EqualTo(1.0));
        }

        [Test]
        public void ScoringIsCaseAndWhitespaceInsensitive()
        {
            Assert.That(PronunciationScorer.Score("Hello", "  hello  "), Is.EqualTo(1.0));
        }

        [Test]
        public void CompletelyDifferentTextScoresZero()
        {
            Assert.That(PronunciationScorer.Score("cat", "dog"), Is.EqualTo(0.0).Within(0.001));
        }

        [Test]
        public void PartialMatchScoresBetweenZeroAndOne()
        {
            double score = PronunciationScorer.Score("hello", "hallo");

            Assert.That(score, Is.GreaterThan(0.0));
            Assert.That(score, Is.LessThan(1.0));
        }

        [Test]
        public void EmptyTranscriptAgainstNonEmptyTargetScoresZero()
        {
            Assert.That(PronunciationScorer.Score("hello", ""), Is.EqualTo(0.0));
        }

        [Test]
        public void BothEmptyScoresOne()
        {
            Assert.That(PronunciationScorer.Score("", ""), Is.EqualTo(1.0));
        }
    }
}
