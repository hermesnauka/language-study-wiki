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

        [Test]
        public async Task TogglingSwitchesToPolishThenBackToEnglish()
        {
            var controller = new LanguageToggleController(new RecordingNotifier());

            await controller.ToggleAsync();
            Assert.That(controller.Current, Is.EqualTo(Language.Polish));

            await controller.ToggleAsync();
            Assert.That(controller.Current, Is.EqualTo(Language.English));
        }

        [Test]
        public async Task TogglingRaisesLanguageChangedAndNotifiesTheBackend()
        {
            var notifier = new RecordingNotifier();
            var controller = new LanguageToggleController(notifier);
            Language? raised = null;
            controller.LanguageChanged += lang => raised = lang;

            await controller.ToggleAsync();

            Assert.That(raised, Is.EqualTo(Language.Polish));
            Assert.That(notifier.Notified, Is.EqualTo(new[] { Language.Polish }));
        }
    }
}
