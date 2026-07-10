# IRateLimiter

The `IRateLimiter` interface provides a standardized mechanism for controlling the frequency of operations, ensuring that system components, particularly those handling GPS tracking data, adhere to defined throughput constraints. It utilizes a token bucket algorithm to balance traffic load, allowing for burst handling while maintaining a consistent average refill rate.

## API

*   **RateLimitingService**: The concrete implementation of the `IRateLimiter` interface responsible for enforcing rate limits.
*   **TokenBucket**: The internal data structure that encapsulates the state of the rate limiter, including capacity, current token count, and refill mechanics.
*   **bool AllowRequest()**: Evaluates whether a request can be processed based on the current bucket state. Returns `true` if the request is permitted; otherwise, returns `false`.
*   **int GetRemainingTokens()**: Returns the number of tokens available for immediate consumption.
*   **bool TryConsumeToken()**: Attempts to remove one token from the bucket. Returns `true` if a token was successfully consumed, and `false` if the request is denied due to insufficient tokens.
*   **int GetCurrentTokens()**: Returns the total number of tokens currently residing in the bucket.
*   **double MaxTokens**: A read-only property representing the maximum capacity of the token bucket.
*   **double RefillRate**: A read-only property indicating the rate at which tokens are replenished in the bucket per unit of time.

## Usage

### Example 1: Checking Request Permission
```csharp
public void ProcessDeviceData(IRateLimiter limiter)
{
    if (limiter.AllowRequest())
    {
        // Proceed with processing GPS data
    }
    else
    {
        // Handle rate-limited scenario (e.g., log or queue for later)
    }
}
```

### Example 2: Consuming Tokens for Rate-Limited Operations
```csharp
public void PerformRateLimitedAction(IRateLimiter limiter)
{
    if (limiter.TryConsumeToken())
    {
        // Perform the restricted operation
    }
    else
    {
        // Operation rejected: maximum throughput reached
    }
}
```

## Notes

*   **Thread Safety**: While `IRateLimiter` defines the interface, implementations are expected to handle concurrent access to the `TokenBucket` state. In high-concurrency environments, ensure the `RateLimitingService` implementation utilizes appropriate locking mechanisms to prevent race conditions during token consumption and refill.
*   **Token Depletion**: The behavior when `GetCurrentTokens()` returns zero is deterministic; `TryConsumeToken()` will immediately return `false`, and `AllowRequest()` will reflect the exhaustion of the bucket.
*   **Configuration**: The `MaxTokens` and `RefillRate` values are typically initialized at the time of `RateLimitingService` instantiation and should be considered immutable during the lifetime of the bucket to ensure predictable throughput.
