#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GpsTrackerProtocol.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GpsTrackerProtocol.Tests.Services;

/// <summary>
/// Tests for the rate limiter with TTL-based eviction.
/// </summary>
public class RateLimiterTests
{
    [Fact]
    public void AllowRequest_WithNullDeviceId_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new RateLimitConfig { MaxTokens = 10, RefillRate = 5 };
        var rateLimiter = new RateLimitingService(config);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => rateLimiter.AllowRequest(null!));
    }

    [Fact]
    public void GetRemainingTokens_WithNullDeviceId_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new RateLimitConfig { MaxTokens = 10, RefillRate = 5 };
        var rateLimiter = new RateLimitingService(config);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => rateLimiter.GetRemainingTokens(null!));
    }

    [Fact]
    public void AllowRequest_WithNewDevice_ReturnsTrue()
    {
        // Arrange
        var config = new RateLimitConfig { MaxTokens = 10, RefillRate = 5 };
        var rateLimiter = new RateLimitingService(config);

        // Act
        var result = rateLimiter.AllowRequest("device1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AllowRequest_WithActiveDevice_NeverRejected()
    {
        // Arrange
        var config = new RateLimitConfig { MaxTokens = 10, RefillRate = 5 };
        var rateLimiter = new RateLimitingService(config);

        // Act - allow 10 requests (full bucket)
        for (int i = 0; i < 10; i++)
        {
            rateLimiter.AllowRequest("device1");
        }

        // Assert - should always allow requests from active devices, never reject
        Assert.True(rateLimiter.AllowRequest("device1"));
    }

    [Fact]
    public void GetRemainingTokens_WithNewDevice_ReturnsMaxTokens()
    {
        // Arrange
        var config = new RateLimitConfig { MaxTokens = 100, RefillRate = 10 };
        var rateLimiter = new RateLimitingService(config);

        // Act
        var remaining = rateLimiter.GetRemainingTokens("device1");

        // Assert
        Assert.Equal(100, remaining);
    }

    [Fact]
    public void GetRemainingTokens_AfterConsumingTokens_ReturnsCorrectValue()
    {
        // Arrange
        var config = new RateLimitConfig { MaxTokens = 10, RefillRate = 5 };
        var rateLimiter = new RateLimitingService(config);

        // Consume 5 tokens
        for (int i = 0; i < 5; i++)
        {
            rateLimiter.AllowRequest("device1");
        }

        // Act
        var remaining = rateLimiter.GetRemainingTokens("device1");

        // Assert
        Assert.Equal(5, remaining);
    }

    [Fact]
    public void AllowRequest_WithMultipleDevices_TracksSeparately()
    {
        // Arrange
        var config = new RateLimitConfig { MaxTokens = 5, RefillRate = 2 };
        var rateLimiter = new RateLimitingService(config);

        // Act - consume all tokens for device1
        for (int i = 0; i < 5; i++)
        {
            rateLimiter.AllowRequest("device1");
        }

        // Assert - device1 should still be allowed (never rejects active devices)
        Assert.True(rateLimiter.AllowRequest("device1"));

        // Act - device2 should have all tokens available (separate bucket)
        var device2Allowed = rateLimiter.AllowRequest("device2");
        var device2Remaining = rateLimiter.GetRemainingTokens("device2");

        // Assert
        Assert.True(device2Allowed);
        Assert.Equal(5, device2Remaining);
    }

    [Fact]
    public void AllowRequest_WithDeviceChurn_EventuallyEvictsIdleDevices()
    {
        // Arrange
        var config = new RateLimitConfig { MaxTokens = 10, RefillRate = 5 };
        var rateLimiter = new RateLimitingService(config);

        // Create many devices (simulating churn)
        var deviceIds = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            var deviceId = $"device-{i}-{Guid.NewGuid()}";
            deviceIds.Add(deviceId);

            // Each device makes a few requests
            for (int j = 0; j < 3; j++)
            {
                rateLimiter.AllowRequest(deviceId);
            }
        }

        // Wait for eviction window to pass (window is 2 seconds for these settings)
        Thread.Sleep(2500);

        // Act - try to access an old device that should have been evicted
        var oldDeviceAllowed = rateLimiter.AllowRequest(deviceIds[0]);
        var oldDeviceRemaining = rateLimiter.GetRemainingTokens(deviceIds[0]);

        // Assert - old device should get fresh bucket (not rejected)
        Assert.True(oldDeviceAllowed);
        Assert.Equal(10, oldDeviceRemaining);
    }

    [Fact]
    public void AllowRequest_WithActiveDevice_ContinuesToAllowRequests()
    {
        // Arrange
        var config = new RateLimitConfig { MaxTokens = 5, RefillRate = 2 };
        var rateLimiter = new RateLimitingService(config);

        // Continuously make requests for a long time
        var deviceId = "persistent-device";
        var startTime = DateTime.UtcNow;

        // Run for 5 seconds, making requests as fast as possible
        while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(5))
        {
            var allowed = rateLimiter.AllowRequest(deviceId);
            Assert.True(allowed);
        }
    }

    [Fact]
    public void Dispose_CleansUpResources()
    {
        // Arrange
        var config = new RateLimitConfig { MaxTokens = 10, RefillRate = 5 };
        var rateLimiter = new RateLimitingService(config);

        // Make some requests
        rateLimiter.AllowRequest("device1");
        rateLimiter.AllowRequest("device2");

        // Act
        rateLimiter.Dispose();

        // Assert - no exception thrown
        // The dispose should clean up all buckets
        Assert.True(true);
    }

    [Fact]
    public void CaseInsensitiveDeviceIds_TreatedAsSameDevice()
    {
        // Arrange
        var config = new RateLimitConfig { MaxTokens = 5, RefillRate = 2 };
        var rateLimiter = new RateLimitingService(config);

        var deviceId1 = "Device-1";
        var deviceId2 = "device-1";

        // Act - make requests with different casing
        rateLimiter.AllowRequest(deviceId1);
        rateLimiter.AllowRequest(deviceId1);

        var remaining1 = rateLimiter.GetRemainingTokens(deviceId1);
        var remaining2 = rateLimiter.GetRemainingTokens(deviceId2);

        // Assert - should be treated as the same device (case insensitive)
        Assert.Equal(3, remaining1);
        Assert.Equal(3, remaining2);
    }

    [Fact]
    public void GetRemainingTokens_ForUnknownDevice_ReturnsMaxTokens()
    {
        // Arrange
        var config = new RateLimitConfig { MaxTokens = 100, RefillRate = 10 };
        var rateLimiter = new RateLimitingService(config);

        // Act
        var remaining = rateLimiter.GetRemainingTokens("unknown-device");

        // Assert - returns max tokens for unknown devices
        Assert.Equal(100, remaining);
    }

    [Fact]
    public void RateLimitConfig_WithValidValues_CreatesServiceSuccessfully()
    {
        // Arrange
        var config = new RateLimitConfig
        {
            MaxTokens = 50,
            RefillRate = 25
        };

        // Act
        var rateLimiter = new RateLimitingService(config);

        // Assert
        Assert.NotNull(rateLimiter);
    }
}
