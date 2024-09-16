#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using NSubstitute;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Services;

namespace GpsTrackerProtocol.Tests;

public class RouteReplayServiceTests
{
    private static Journey BuildJourney(string id, int waypointCount, int status = 1)
    {
        var baseTime = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var journey  = new Journey
        {
            Id        = id,
            DeviceId  = "device-replay",
            Status    = status,
            StartTime = baseTime
        };

        for (var i = 0; i < waypointCount; i++)
        {
            journey.Waypoints.Add(new LocationData
            {
                DeviceId  = "device-replay",
                Latitude  = 51.5 + i * 0.01,
                Longitude = -0.1 + i * 0.01,
                Speed     = 50,
                Bearing   = 90,
                Timestamp = baseTime.AddMinutes(i * 5)
            });
        }

        if (status == 1)
            journey.EndTime = journey.Waypoints[^1].Timestamp;

        return journey;
    }

    private static RouteReplayService BuildSut(Journey journey)
    {
        var repo = Substitute.For<IJourneyRepository>();
        repo.GetByIdAsync(journey.Id).Returns(journey);

        var uow = Substitute.For<IUnitOfWork>();
        uow.Journeys.Returns(repo);

        return new RouteReplayService(uow, Substitute.For<ILogger<RouteReplayService>>());
    }

    [Fact]
    public async Task ReplayJourneyAsync_ShouldReturnCorrectFrameCount()
    {
        var journey = BuildJourney("j-1", 5);
        var sut     = BuildSut(journey);

        var result = await sut.ReplayJourneyAsync("j-1");

        result.Frames.Should().HaveCount(5);
        result.JourneyId.Should().Be("j-1");
    }

    [Fact]
    public async Task ReplayJourneyAsync_ShouldCompressTimestampsWithSpeedMultiplier()
    {
        var journey = BuildJourney("j-2", 3);
        var sut     = BuildSut(journey);

        var result = await sut.ReplayJourneyAsync("j-2", new ReplayOptions { SpeedMultiplier = 2.0 });

        // Original duration is 2 × 5 = 10 min; at 2× speed the replay duration should be 5 min
        result.ReplayDuration.Should().BeCloseTo(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(1));
        result.OriginalDuration.Should().BeCloseTo(TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task ReplayJourneyAsync_ShouldThrow_WhenJourneyIsOngoing()
    {
        var journey = BuildJourney("j-3", 3, status: 0);
        var sut     = BuildSut(journey);

        await sut.Invoking(s => s.ReplayJourneyAsync("j-3"))
                 .Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*ongoing*");
    }

    [Fact]
    public async Task ReplayJourneyAsync_ShouldThrow_WhenJourneyHasFewerThanTwoWaypoints()
    {
        var journey = BuildJourney("j-4", 1);
        var sut     = BuildSut(journey);

        await sut.Invoking(s => s.ReplayJourneyAsync("j-4"))
                 .Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*fewer than 2*");
    }

    [Fact]
    public async Task ReplayJourneyAsync_ShouldSliceWaypoints_WhenStartAndEndIndexSet()
    {
        var journey = BuildJourney("j-5", 6);
        var sut     = BuildSut(journey);

        var result = await sut.ReplayJourneyAsync("j-5", new ReplayOptions { StartIndex = 1, EndIndex = 3 });

        result.Frames.Should().HaveCount(3);
    }

    [Fact]
    public async Task ReplayJourneyAsync_ShouldRebaseTimestamps_WhenRebaseToUtcSet()
    {
        var journey  = BuildJourney("j-6", 3);
        var sut      = BuildSut(journey);
        var rebaseAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = await sut.ReplayJourneyAsync("j-6", new ReplayOptions { RebaseToUtc = rebaseAt });

        result.Frames[0].ReplayTimestamp.Should().Be(rebaseAt);
        result.Frames[1].ReplayTimestamp.Should().BeAfter(rebaseAt);
    }

    [Fact]
    public async Task GetReplaySummaryAsync_ShouldReturnCorrectWaypointCount()
    {
        var journey = BuildJourney("j-7", 4);
        var sut     = BuildSut(journey);

        var summary = await sut.GetReplaySummaryAsync("j-7");

        summary.WaypointCount.Should().Be(4);
        summary.JourneyId.Should().Be("j-7");
    }
}
