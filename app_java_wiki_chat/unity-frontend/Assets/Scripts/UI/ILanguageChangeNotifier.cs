using System.Threading.Tasks;
using SecureLearning.Client.Localization;

namespace SecureLearning.Client.UI
{
    /// <summary>
    /// Seam between the (pure, testable) <see cref="LanguageToggleController"/> and the
    /// network layer, so the controller's toggle logic can be unit tested without a
    /// live backend or Unity's PlayMode.
    /// </summary>
    public interface ILanguageChangeNotifier
    {
        Task NotifyAsync(Language language);
    }

    /// <summary>Used when no room/session is active yet — the toggle still works locally.</summary>
    public class NoOpLanguageChangeNotifier : ILanguageChangeNotifier
    {
        public Task NotifyAsync(Language language) => Task.CompletedTask;
    }
}
