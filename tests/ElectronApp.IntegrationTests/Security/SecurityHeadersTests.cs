using ElectronApp.IntegrationTests.Infrastructure;

namespace ElectronApp.IntegrationTests.Security;

public sealed class SecurityHeadersTests(ElectronAppWebApplicationFactory factory)
    : IClassFixture<ElectronAppWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task RootRequest_ShouldIncludeSecurityHeaders()
    {
        var response = await _client.GetAsync("/", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        Assert.True(response.Headers.TryGetValues("X-Content-Type-Options", out var nosniff));
        Assert.Contains("nosniff", nosniff);

        Assert.True(response.Headers.TryGetValues("X-Frame-Options", out var xfo));
        Assert.Contains("DENY", xfo);

        Assert.True(response.Headers.Contains("Content-Security-Policy"));
    }

    [Fact]
    public async Task ErrorEndpoint_ShouldReturnProblemDetails()
    {
        var response = await _client.GetAsync("/error", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
