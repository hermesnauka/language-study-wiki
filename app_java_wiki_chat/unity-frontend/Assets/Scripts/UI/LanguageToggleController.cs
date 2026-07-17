using System;
using System.Threading.Tasks;
using SecureLearning.Client.Localization;

namespace SecureLearning.Client.UI
{
    /// <summary>
    /// US-1.1: flip between English/Polish. Pure C# (no UnityEngine dependency) so it
    /// can be unit tested directly; <see cref="FlagToggleView"/> is the thin MonoBehaviour
    /// adapter that wires this to an actual scene's Button/Text components.
    /// </summary>
    public class LanguageToggleController
    {
        private readonly ILanguageChangeNotifier notifier;

        public Language Current { get; private set; }
        public event Action<Language> LanguageChanged;

        public LanguageToggleController(ILanguageChangeNotifier notifier, Language initial = Language.English)
        {
            this.notifier = notifier;
            Current = initial;
        }

        public async Task ToggleAsync()
        {
            Current = Current == Language.English ? Language.Polish : Language.English;
            LanguageChanged?.Invoke(Current);
            await notifier.NotifyAsync(Current);
        }
    }
}
