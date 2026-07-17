namespace SecureLearning.Backend.Translation;

/// <summary>
/// Best-effort AI translation (FR-3), mirroring django-ai-worker's TranslationProvider:
/// never throws, returns null when unavailable so the chat flow always completes — a
/// translation gap just means the original text is relayed untranslated.
/// </summary>
public interface ITranslationGateway
{
    Task<string?> TranslateAsync(string text, string sourceLang, string targetLang, CancellationToken ct = default);
}

/// <summary>Used when no AI worker is configured — every message is relayed untranslated.</summary>
public sealed class NoOpTranslationGateway : ITranslationGateway
{
    public Task<string?> TranslateAsync(string text, string sourceLang, string targetLang, CancellationToken ct = default) =>
        Task.FromResult<string?>(null);
}
