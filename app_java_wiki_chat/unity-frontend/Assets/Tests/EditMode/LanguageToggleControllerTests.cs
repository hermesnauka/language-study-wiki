using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SecureLearning.Client.Localization;
using SecureLearning.Client.UI;

namespace SecureLearning.Client.Tests
{
    public class LanguageToggleControllerTests
    {
        private class RecordingNotifier : ILanguageChangeNotifier
        {
            public readonly List<Language> Notified = new();

            public Task NotifyAsync(Language language)
            {
                Notified.Add(language);
                return Task.CompletedTask;
            }
        }

        [Test]
        public void StartsInEnglishByDefault()
        {
            var controller = new LanguageToggleController(new RecordingNotifier());

            Assert.That(controller.Current, Is.EqualTo(Language.English));
        }

        // Unity Test Framework 1.1.x (the 2022.3 default) rejects async Task test
        // methods, so these block on ToggleAsync instead — safe here because
        // RecordingNotifier completes synchronously.
        [Test]
        public void TogglingSwitchesToPolishThenBackToEnglish()
        {
            var controller = new LanguageToggleController(new RecordingNotifier());

            controller.ToggleAsync().GetAwaiter().GetResult();
            Assert.That(controller.Current, Is.EqualTo(Language.Polish));

            controller.ToggleAsync().GetAwaiter().GetResult();
            Assert.That(controller.Current, Is.EqualTo(Language.English));
        }

        [Test]
        public void TogglingRaisesLanguageChangedAndNotifiesTheBackend()
        {
            var notifier = new RecordingNotifier();
            var controller = new LanguageToggleController(notifier);
            Language? raised = null;
            controller.LanguageChanged += lang => raised = lang;

            controller.ToggleAsync().GetAwaiter().GetResult();

            Assert.That(raised, Is.EqualTo(Language.Polish));
            Assert.That(notifier.Notified, Is.EqualTo(new[] { Language.Polish }));
        }
    }
}
