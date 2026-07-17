using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using SecureLearning.Backend.Translation;

namespace SecureLearning.Backend.Tests;

public class AiWorkerTranslationGatewayTests
{
    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return responder(request);
        }
    }

    private static AiWorkerTranslationGateway Gateway(StubHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("http://ai-worker.test") },
            NullLogger<AiWorkerTranslationGateway>.Instance);

    [Fact]
    public async Task ReturnsTheTranslationAndSendsCamelCaseJson()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"translation":"kot"}""", Encoding.UTF8, "application/json")
        });

        string? result = await Gateway(handler).TranslateAsync("cat", "en", "pl");

        Assert.Equal("kot", result);
        Assert.Equal("/api/chat/translate", handler.LastRequest!.RequestUri!.AbsolutePath);
        // django-ai-worker expects camelCase field names — matches the Java client's wire format.
        Assert.Contains("\"sourceLang\"", handler.LastRequestBody);
        Assert.Contains("\"targetLang\"", handler.LastRequestBody);
    }

    [Fact]
    public async Task NullTranslationFromWorkerIsPassedThroughAsNull()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"translation":null}""", Encoding.UTF8, "application/json")
        });

        Assert.Null(await Gateway(handler).TranslateAsync("cat", "en", "pl"));
    }

    [Fact]
    public async Task WorkerErrorsNeverThrowTheyJustReturnNull()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        Assert.Null(await Gateway(handler).TranslateAsync("cat", "en", "pl"));
    }

    [Fact]
    public async Task NetworkFailureNeverThrowsItJustReturnsNull()
    {
        var handler = new StubHandler(_ => throw new HttpRequestException("connection refused"));

        Assert.Null(await Gateway(handler).TranslateAsync("cat", "en", "pl"));
    }
}
