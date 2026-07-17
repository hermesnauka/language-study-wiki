using Newtonsoft.Json;
using NUnit.Framework;
using SecureLearning.Client.Api.Dto;

namespace SecureLearning.Client.Tests
{
    /// <summary>Confirms the DTO's JSON shape matches spring-backend's ChatWireMessage record field names exactly.</summary>
    public class ChatWireMessageDtoTests
    {
        [Test]
        public void RoundTripsThroughJsonWithTheExpectedFieldNames()
        {
            var message = new ChatWireMessageDto { text = "cat", sourceLang = "en", targetLang = "pl" };

            string json = JsonConvert.SerializeObject(message);
            var roundTripped = JsonConvert.DeserializeObject<ChatWireMessageDto>(json);

            StringAssert.Contains("\"text\":\"cat\"", json);
            StringAssert.Contains("\"sourceLang\":\"en\"", json);
            StringAssert.Contains("\"targetLang\":\"pl\"", json);
            Assert.That(roundTripped.text, Is.EqualTo("cat"));
            Assert.That(roundTripped.sourceLang, Is.EqualTo("en"));
            Assert.That(roundTripped.targetLang, Is.EqualTo("pl"));
        }
    }
}
