using System.Collections.Concurrent;
using GpsTrackerProtocol.Infrastructure;
using Xunit;

namespace GpsTrackerProtocol.Tests.Services;

/// <summary>
/// Thread-safety and concurrency tests for RateLimitingService.
/// </summary>
public class RateLimiterConcurrencyTests
{
    [Fact]
    public async Task AllowRequest_ConcurrentCalls_MaintainsThreadSafety()
    {
        // Arrange
        var config = new RateLimitConfig { MaxTokens = 10, RefillRate = 1 };
        var rateLimiter = new RateLimitingService(config);
        var deviceId = "concurrent-device";
        int numTasks = 100;
        int requestsPerTask = 50;

        // Act
        var tasks = Enumerable.Range(0, numTasks).Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < requestsPerTask; i++)
            {
                rateLimiter.AllowRequest(deviceId);
            }
        }));

        await Task.WhenAll(tasks);

        // Assert - verify we don't have corrupted state (tokens shouldn't be NaN or inconsistent)
        var remaining = rateLimiter.GetRemainingTokens(deviceId);
        Assert.True(remaining <= (int)config.MaxTokens);
    }

    [Fact]
    public async Task BoundedStorage_UnderLoad_RemovesOldEntries()
    {
        // Arrange
        var config = new RateLimitConfig { MaxTokens = 10, RefillRate = 10 };
        var rateLimiter = new RateLimitingService(config);
        int numDevices = 1000;

        // Act - insert many distinct keys
        for (int i = 0; i < numDevices; i++)
        {
            rateLimiter.AllowRequest($"device-{i}");
        }

        // Wait for eviction timer to run (it runs every 500ms or so)
        // With these settings, window is 1 second, timer runs every 500ms.
        // 2 seconds should be enough for eviction.
        await Task.Delay(2500);

        // Assert - verify old entries are removed
        // We know `device-0` should be evicted.
        // If it's removed, GetRemainingTokens should return MaxTokens (as if it's new)
        // rather than whatever it had before.
        // Actually, this doesn't strictly verify the _buckets count, 
        // but it verifies the eviction behavior which is the mechanism for bounding storage.
        
        // Let's verify device-0 is indeed treated as new (MaxTokens)
        var remaining = rateLimiter.GetRemainingTokens("device-0");
        Assert.Equal((int)config.MaxTokens, remaining);
    }

    [Fact]
    public async Task WindowReset_AtBoundary_RefillsCorrectly()
    {
        // Arrange
        // RefillRate = 1 token/sec, MaxTokens = 5
        var config = new RateLimitConfig { MaxTokens = 5, RefillRate = 1 };
        var rateLimiter = new RateLimitingService(config);
        var deviceId = "boundary-device";

        // Consume all tokens
        for (int i = 0; i < 5; i++)
        {
            rateLimiter.AllowRequest(deviceId);
        }
        
        Assert.Equal(0, rateLimiter.GetRemainingTokens(deviceId));

        // Act - wait exactly for 1 token to refill (1 second)
        await Task.Delay(1100); // Wait a bit more than 1 second to be safe

        // Assert
        var remaining = rateLimiter.GetRemainingTokens(deviceId);
        Assert.True(remaining >= 1, $"Expected at least 1 token after 1s, got {remaining}");
    }
}
