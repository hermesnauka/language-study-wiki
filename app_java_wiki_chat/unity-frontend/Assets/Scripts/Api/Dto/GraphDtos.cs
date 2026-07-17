using System.Collections.Generic;

namespace SecureLearning.Client.Api.Dto
{
    /// <summary>Mirrors django-ai-worker's knowledge.views._term_to_dict.</summary>
    public class TermDto
    {
        public int id;
        public string text;
        public string language;
        public string reading;
    }

    /// <summary>One edge as returned by GET /api/graph/export.</summary>
    public class GraphEdgeDto
    {
        public int from;
        public int to;
        public string source;
        public float confidence;
    }

    /// <summary>Mirrors django-ai-worker's knowledge.views.graph_export response body.</summary>
    public class GraphExportDto
    {
        public List<TermDto> terms;
        public List<GraphEdgeDto> edges;
    }
}
