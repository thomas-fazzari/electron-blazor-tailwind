using Bogus;
using ElectronApp.Features.Desktop;
using ElectronApp.Security;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace ElectronApp.UnitTests.Security;

public sealed class ExternalLinkServiceTests
{
    [Fact]
    public async Task TryOpenAsync_WithMalformedUrl_ShouldReturnFalse()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var desktopBridge = Substitute.For<IDesktopBridge>();
        var sut = BuildSut(desktopBridge);

        var result = await sut.TryOpenAsync("not-a-url", cancellationToken);

        Assert.False(result);
        await desktopBridge
            .DidNotReceiveWithAnyArgs()
            .OpenExternalAsync(Arg.Any<Uri>(), cancellationToken);
    }

    [Fact]
    public async Task TryOpenAsync_WithNonHttpsUrl_ShouldReturnFalse()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var desktopBridge = Substitute.For<IDesktopBridge>();
        var sut = BuildSut(desktopBridge);

        var result = await sut.TryOpenAsync(
            "http://github.com/ElectronNET/Electron.NET",
            cancellationToken
        );

        Assert.False(result);
        await desktopBridge
            .DidNotReceiveWithAnyArgs()
            .OpenExternalAsync(Arg.Any<Uri>(), cancellationToken);
    }

    [Fact]
    public async Task TryOpenAsync_WithHostOutsideAllowlist_ShouldReturnFalse()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var desktopBridge = Substitute.For<IDesktopBridge>();
        var host = new Faker().Internet.DomainName();
        var sut = BuildSut(desktopBridge);

        var result = await sut.TryOpenAsync($"https://{host}/docs", cancellationToken);

        Assert.False(result);
        await desktopBridge
            .DidNotReceiveWithAnyArgs()
            .OpenExternalAsync(Arg.Any<Uri>(), cancellationToken);
    }

    [Fact]
    public async Task TryOpenAsync_WithAllowedUrl_ShouldUseDesktopBridge()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var desktopBridge = Substitute.For<IDesktopBridge>();
        desktopBridge.OpenExternalAsync(Arg.Any<Uri>(), Arg.Any<CancellationToken>()).Returns(true);
        var sut = BuildSut(desktopBridge);

        var result = await sut.TryOpenAsync(
            "https://github.com/ElectronNET/Electron.NET",
            cancellationToken
        );

        Assert.True(result);
        await desktopBridge
            .Received(1)
            .OpenExternalAsync(Arg.Is<Uri>(uri => uri.Host == "github.com"), cancellationToken);
    }

    private static ExternalLinkService BuildSut(IDesktopBridge desktopBridge)
    {
        var options = Options.Create(
            new ExternalLinksOptions
            {
                RequireHttps = true,
                AllowedHosts = ["github.com", "www.github.com"],
            }
        );

        return new ExternalLinkService(
            options,
            desktopBridge,
            NullLogger<ExternalLinkService>.Instance
        );
    }
}
