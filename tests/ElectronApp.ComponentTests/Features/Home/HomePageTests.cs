using Bunit;
using ElectronApp.Features.Home;
using ElectronApp.Security;
using Microsoft.Extensions.DependencyInjection;

namespace ElectronApp.ComponentTests.Features.Home;

public sealed class HomePageTests
{
    [Fact]
    public void ClickingGithubLink_ShouldUseExternalLinkService()
    {
        using var context = new BunitContext();
        var fakeService = new FakeExternalLinkService();
        context.Services.AddSingleton<IExternalLinkService>(fakeService);

        var cut = context.Render<HomePage>();
        cut.FindAll("button[type='button']")[0].Click();

        Assert.Contains("https://github.com/thomas-fazzari", fakeService.OpenedUrls);
    }

    private sealed class FakeExternalLinkService : IExternalLinkService
    {
        public List<string> OpenedUrls { get; } = [];

        public Task<bool> TryOpenAsync(string url, CancellationToken cancellationToken = default)
        {
            OpenedUrls.Add(url);
            return Task.FromResult(true);
        }
    }
}
