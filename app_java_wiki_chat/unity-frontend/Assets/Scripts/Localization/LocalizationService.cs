using System.Collections.Generic;

namespace SecureLearning.Client.Localization
{
    /// <summary>
    /// Minimal key -> text lookup for UI strings in both supported languages. Kept
    /// deliberately simple (no Unity Localization package dependency) since only two
    /// languages need switching (US-1.1) — swap for the full package later if more
    /// UI languages are ever added.
    /// </summary>
    public class LocalizationService
    {
        private readonly Dictionary<string, Dictionary<Language, string>> entries = new();

        public void AddEntry(string key, string english, string polish)
        {
            entries[key] = new Dictionary<Language, string>
            {
                [Language.English] = english,
                [Language.Polish] = polish
            };
        }

        public string Translate(string key, Language language)
        {
            if (entries.TryGetValue(key, out var perLanguage) && perLanguage.TryGetValue(language, out var text))
            {
                return text;
            }
            return key;
        }
    }
}
