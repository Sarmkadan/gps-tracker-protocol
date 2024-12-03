# GpsTracker Protocol

<!-- (rest of README.md remains the same) -->

## IRateLimiter

The `IRateLimiter` interface defines a contract for rate limiting, which is used to prevent device spam and protect system resources. It provides methods to check if a request is allowed and to get the remaining tokens for a device.

Example usage:

```csharp
using GpsTrackerProtocol.Infrastructure;

public class RateLimiterExample
{
  public void Demo()
  {
    // Create a rate limiter instance
    var rateLimiter = new RateLimitingService(new RateLimitConfig());

    // Check if a request is allowed for a device
    var deviceId = "device123";
    if (rateLimiter.AllowRequest(deviceId))
    {
      Console.WriteLine("Request allowed");
    }
    else
    {
      Console.WriteLine("Request denied");
    }

    // Get the remaining tokens for a device
    var remainingTokens = rateLimiter.GetRemainingTokens(deviceId);
    Console.WriteLine($"Remaining tokens: {remainingTokens}");
  }
}
```

The `IRateLimiter` interface is implemented by the `RateLimitingService` class, which uses a token bucket algorithm to manage rate limiting.

<!-- (rest of README.md remains the same) -->
