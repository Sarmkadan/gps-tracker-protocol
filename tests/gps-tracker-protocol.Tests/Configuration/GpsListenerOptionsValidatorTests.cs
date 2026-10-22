#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GpsTrackerProtocol.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace GpsTrackerProtocol.Tests.Configuration;

/// <summary>
/// Tests for <see cref="GpsListenerOptionsValidator"/> and the startup checks that use it.
/// </summary>
public class GpsListenerOptionsValidatorTests
{
    private static ValidateOptionsResult Validate(GpsListenerOptions? options) =>
        new GpsListenerOptionsValidator().Validate(null, options);

    [Fact]
    public void Validate_WithDefaults_ShouldSucceed()
    {
        var result = Validate(new GpsListenerOptions());

        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(65535, 65535)]
    [InlineData(5000, 5001)]
    public void Validate_WithPortsAtOrInsideRange_ShouldSucceed(int tcpPort, int udpPort)
    {
        var result = Validate(new GpsListenerOptions { TcpPort = tcpPort, UdpPort = udpPort });

        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    public void Validate_WithTcpPortOutOfRange_ShouldFailNamingTcpPort(int port)
    {
        var result = Validate(new GpsListenerOptions { TcpPort = port });

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("TcpPort").And.Contain(port.ToString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    public void Validate_WithUdpPortOutOfRange_ShouldFailNamingUdpPort(int port)
    {
        var result = Validate(new GpsListenerOptions { UdpPort = port });

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("UdpPort").And.Contain(port.ToString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_WithNonPositiveMaxConnections_ShouldFail(int maxConnections)
    {
        var result = Validate(new GpsListenerOptions { MaxConnections = maxConnections });

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain("MaxConnections");
    }

    [Theory]
    [InlineData(63)]
    [InlineData(65537)]
    public void Validate_WithReceiveBufferOutsideBounds_ShouldFail(int size)
    {
        var result = Validate(new GpsListenerOptions { ReceiveBufferSize = size });

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain("ReceiveBufferSize");
    }

    [Theory]
    [InlineData(64)]
    [InlineData(65536)]
    public void Validate_WithReceiveBufferAtBounds_ShouldSucceed(int size)
    {
        var result = Validate(new GpsListenerOptions { ReceiveBufferSize = size });

        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNonPositiveIdleTimeout_ShouldFail(int seconds)
    {
        var result = Validate(new GpsListenerOptions { IdleTimeout = TimeSpan.FromSeconds(seconds) });

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain("IdleTimeout");
    }

    [Fact]
    public void Validate_WithIdleTimeoutBeyondCancelAfterLimit_ShouldFail()
    {
        var result = Validate(new GpsListenerOptions { IdleTimeout = TimeSpan.MaxValue });

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().Contain("IdleTimeout");
    }

    [Fact]
    public void Validate_WithNullOptions_ShouldFail()
    {
        var result = Validate(null);

        result.Failed.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEveryFieldInvalid_ShouldReportAllFailures()
    {
        var options = new GpsListenerOptions
        {
            TcpPort = 0,
            UdpPort = 70000,
            MaxConnections = 0,
            ReceiveBufferSize = 8,
            IdleTimeout = TimeSpan.Zero
        };

        var result = Validate(options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().HaveCount(5);
        result.Failures.Should().Contain(f => f.Contains("TcpPort"));
        result.Failures.Should().Contain(f => f.Contains("UdpPort"));
        result.Failures.Should().Contain(f => f.Contains("MaxConnections"));
        result.Failures.Should().Contain(f => f.Contains("ReceiveBufferSize"));
        result.Failures.Should().Contain(f => f.Contains("IdleTimeout"));
    }

    [Fact]
    public void Server_WithInvalidOptions_ShouldThrowListingEveryViolation()
    {
        var options = new GpsListenerOptions { TcpPort = 0, MaxConnections = -1 };

        var act = () => new RealTimeGpsServer(options);

        act.Should().Throw<OptionsValidationException>()
            .Which.Failures.Should().HaveCount(2);
        act.Should().Throw<OptionsValidationException>()
            .WithMessage("*TcpPort*")
            .WithMessage("*MaxConnections*");
    }

    [Fact]
    public void Server_WithValidOptions_ShouldConstruct()
    {
        var act = () => new RealTimeGpsServer(new GpsListenerOptions { TcpPort = 6000, UdpPort = 6001 });

        act.Should().NotThrow();
    }

    [Fact]
    public void AddGpsListenerOptions_WithInvalidValues_ShouldFailWhenResolved()
    {
        var services = new ServiceCollection();
        services.AddGpsListenerOptions(o => o.UdpPort = 0);
        using var provider = services.BuildServiceProvider();

        var act = () => provider.GetRequiredService<IOptions<GpsListenerOptions>>().Value;

        act.Should().Throw<OptionsValidationException>()
            .WithMessage("*UdpPort*");
    }

    [Fact]
    public void AddGpsListenerOptions_WithValidValues_ShouldResolveConfiguredOptions()
    {
        var services = new ServiceCollection();
        services.AddGpsListenerOptions(o => o.TcpPort = 6000);
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<GpsListenerOptions>>().Value;

        options.TcpPort.Should().Be(6000);
        options.UdpPort.Should().Be(5001);
    }
}
