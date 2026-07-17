using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecureLearning.Client.Api.Dto;
using UnityEngine.Networking;

namespace SecureLearning.Client.Api
{
    /// <summary>
    /// FR-5 / AGENTS.md "Unity Sync Agent": pulls the Karpathy-style knowledge graph
    /// straight from django-ai-worker to populate playable levels.
    /// </summary>
    public class GraphApiClient
    {
        private readonly string baseUrl;

        public GraphApiClient(BackendConfig config)
        {
            baseUrl = config.aiWorkerHttpBaseUrl.TrimEnd('/');
        }

        public async Task<GraphExportDto> ExportGraphAsync()
        {
            using var request = UnityWebRequest.Get($"{baseUrl}/api/graph/export");
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException(
                    $"GET {request.url} failed ({request.responseCode}): {request.error}");
            }
            return JsonConvert.DeserializeObject<GraphExportDto>(request.downloadHandler.text);
        }
    }
}
