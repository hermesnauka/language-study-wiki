using UnityEngine;

namespace SecureLearning.Client.Api
{
    /// <summary>
    /// Backend endpoints (spring-backend for session/chat, django-ai-worker for the
    /// knowledge graph — see AGENTS.md's "Unity Sync Agent"). A ScriptableObject so
    /// per-environment (local/dev/staging) assets can be swapped without code changes.
    /// </summary>
    [CreateAssetMenu(menuName = "SecureLearning/Backend Config", fileName = "BackendConfig")]
    public class BackendConfig : ScriptableObject
    {
        [Tooltip("e.g. http://localhost:8080")]
        public string springBackendHttpBaseUrl = "http://localhost:8080";

        [Tooltip("e.g. ws://localhost:8080")]
        public string springBackendWsBaseUrl = "ws://localhost:8080";

        [Tooltip("e.g. http://localhost:8000")]
        public string aiWorkerHttpBaseUrl = "http://localhost:8000";
    }
}
