using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SecureLearning.Client.Api.Dto;
using SecureLearning.Client.Knowledge;

namespace SecureLearning.Client.Tests
{
    public class GraphLevelBuilderTests
    {
        [Test]
        public void BuildsOneLevelPerTermWithItsTranslations()
        {
            var export = new GraphExportDto
            {
                terms = new List<TermDto>
                {
                    new() { id = 1, text = "cat", language = "en", reading = "" },
                    new() { id = 2, text = "kot", language = "pl", reading = "" },
                },
                edges = new List<GraphEdgeDto>
                {
                    new() { from = 1, to = 2, source = "MARKDOWN", confidence = 1.0f },
                    new() { from = 2, to = 1, source = "MARKDOWN", confidence = 1.0f },
                }
            };

            var levels = GraphLevelBuilder.Build(export);

            Assert.That(levels, Has.Count.EqualTo(2));
            VocabLevel catLevel = levels.Single(l => l.TermId == 1);
            Assert.That(catLevel.Translations, Has.Count.EqualTo(1));
            Assert.That(catLevel.Translations[0].Text, Is.EqualTo("kot"));
            Assert.That(catLevel.Translations[0].IsAiGenerated, Is.False);
        }

        [Test]
        public void AiSourcedEdgesAreFlagged()
        {
            var export = new GraphExportDto
            {
                terms = new List<TermDto>
                {
                    new() { id = 1, text = "cat", language = "en", reading = "" },
                    new() { id = 2, text = "chat", language = "fr", reading = "" },
                },
                edges = new List<GraphEdgeDto>
                {
                    new() { from = 1, to = 2, source = "AI", confidence = 0.8f },
                }
            };

            var levels = GraphLevelBuilder.Build(export);

            VocabLevel catLevel = levels.Single(l => l.TermId == 1);
            Assert.That(catLevel.Translations[0].IsAiGenerated, Is.True);
        }

        [Test]
        public void TermWithNoTranslationsGetsAnEmptyLevel()
        {
            var export = new GraphExportDto
            {
                terms = new List<TermDto> { new() { id = 1, text = "lonely", language = "en", reading = "" } },
                edges = new List<GraphEdgeDto>()
            };

            var levels = GraphLevelBuilder.Build(export);

            Assert.That(levels.Single().Translations, Is.Empty);
        }
    }
}
