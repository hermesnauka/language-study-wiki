namespace SecureLearning.Client.Localization
{
    /// <summary>US-1.1: the two UI languages the flag-toggle button switches between.</summary>
    public enum Language
    {
        English,
        Polish
    }

    public static class LanguageCodes
    {
        public static string ToCode(this Language language) => language switch
        {
            Language.English => "en",
            Language.Polish => "pl",
            _ => "en"
        };
    }
}
