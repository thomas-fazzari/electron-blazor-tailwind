using Bunit;
using ElectronApp.Features.Home;
using ElectronApp.Security;
using Microsoft.Extensions.DependencyInjection;

namespace ElectronApp.ComponentTests.Features.Home;

public sealed class HomePageTests
{
    [Fact]
    public void ResourceLinks_ShouldRenderAsAnchorsInWebMode()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<IExternalLinkService>(new FakeExternalLinkService());

        var cut = context.Render<HomePage>();
        var links = cut.FindAll("a[target='_blank']");

        Assert.Equal(2, links.Count);
        Assert.Contains(links, a => a.GetAttribute("href") == "https://github.com/thomas-fazzari");
        Assert.Contains(
            links,
            a => a.GetAttribute("href") == "https://github.com/ElectronNET/Electron.NET"
        );
    }

    [Fact]
    public void ResourceLinks_ShouldHaveNoopenerNoreferrer()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<IExternalLinkService>(new FakeExternalLinkService());

        var cut = context.Render<HomePage>();
        var links = cut.FindAll("a[target='_blank']");

        Assert.All(links, a => Assert.Equal("noopener noreferrer", a.GetAttribute("rel")));
    }

    private sealed class FakeExternalLinkService : IExternalLinkService
    {
        public Task<bool> TryOpenAsync(string url, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }
}
