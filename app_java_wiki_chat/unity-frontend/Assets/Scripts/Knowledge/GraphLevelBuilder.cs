using System.Collections.Generic;
using System.Linq;
using SecureLearning.Client.Api.Dto;

namespace SecureLearning.Client.Knowledge
{
    /// <summary>
    /// FR-5 / AGENTS.md "Unity Sync Agent": turns django-ai-worker's graph export JSON
    /// into playable <see cref="VocabLevel"/>s — one per term, edges become that
    /// term's translations. Pure C# so it's unit-testable without a live backend.
    /// </summary>
    public static class GraphLevelBuilder
    {
        public static IReadOnlyList<VocabLevel> Build(GraphExportDto export)
        {
            var termsById = export.terms.ToDictionary(t => t.id);
            var translationsByFromTerm = export.edges
                .Where(e => termsById.ContainsKey(e.to))
                .GroupBy(e => e.from)
                .ToDictionary(g => g.Key, g => g.Select(e =>
                {
                    TermDto to = termsById[e.to];
                    return new VocabTranslation(to.id, to.text, to.language, e.source == "AI");
                }).ToList());

            var levels = new List<VocabLevel>();
            foreach (TermDto term in export.terms)
            {
                translationsByFromTerm.TryGetValue(term.id, out var translations);
                levels.Add(new VocabLevel(term.id, term.text, term.language, term.reading,
                        translations ?? new List<VocabTranslation>()));
            }
            return levels;
        }
    }
}
