namespace SecureLearning.Backend.Translation;

/// <summary>
/// Calls django-ai-worker's <c>POST /api/chat/translate</c>. Any failure (worker down,
/// network error, malformed response) is swallowed and logged — see <see cref="ITranslationGateway"/>.
/// </summary>
public sealed class AiWorkerTranslationGateway(HttpClient httpClient, ILogger<AiWorkerTranslationGateway> logger)
    : ITranslationGateway
{
    private sealed record TranslateRequest(string Text, string SourceLang, string TargetLang);
    private sealed record TranslateResponse(string? Translation);

    public async Task<string?> TranslateAsync(string text, string sourceLang, string targetLang,
        CancellationToken ct = default)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync("/api/chat/translate",
                new TranslateRequest(text, sourceLang, targetLang), ct);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<TranslateResponse>(cancellationToken: ct);
            return body?.Translation;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            logger.LogWarning(e, "AI worker translation of {Source} -> {Target} failed", sourceLang, targetLang);
            return null;
        }
    }
}
