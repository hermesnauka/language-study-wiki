using NUnit.Framework;
using SecureLearning.Client.Localization;

namespace SecureLearning.Client.Tests
{
    public class LocalizationServiceTests
    {
        [Test]
        public void TranslatesToTheRequestedLanguage()
        {
            var service = new LocalizationService();
            service.AddEntry("greeting", "Hello", "Cześć");

            Assert.That(service.Translate("greeting", Language.English), Is.EqualTo("Hello"));
            Assert.That(service.Translate("greeting", Language.Polish), Is.EqualTo("Cześć"));
        }

        [Test]
        public void UnknownKeyFallsBackToTheKeyItself()
        {
            var service = new LocalizationService();

            Assert.That(service.Translate("missing", Language.English), Is.EqualTo("missing"));
        }
    }
}
