# RouteReplayServiceTests

Test suite for the `RouteReplayService` class, validating its ability to replay stored GPS journeys by streaming waypoint frames with configurable timing compression, time rebasing, and index slicing. These tests cover the core replay pipeline, summary retrieval, and the service's guard clauses against invalid journey states.

## API

### public async Task ReplayJourneyAsync_ShouldReturnCorrectFrameCount

Verifies that `ReplayJourneyAsync` produces exactly the expected number of frames when replaying a complete journey. The frame count must match the number of waypoints in the source journey.

**Parameters:** None (self-contained test method).

**Return Value:** A `Task` representing the asynchronous test operation.

**Throws:** Assertion failures if the observed frame count diverges from the expected count.

---

### public async Task ReplayJourneyAsync_ShouldCompressTimestampsWithSpeedMultiplier

Confirms that applying a speed multiplier greater than 1.0 compresses the interval between consecutive frame timestamps proportionally. A multiplier of *n* should reduce elapsed time between frames by a factor of *n*.

**Parameters:** None.

**Return Value:** A `Task` representing the asynchronous test operation.

**Throws:** Assertion failures if timestamp deltas do not reflect the requested speed multiplier.

---

### public async Task ReplayJourneyAsync_ShouldThrow_WhenJourneyIsOngoing

Ensures that `ReplayJourneyAsync` throws an appropriate exception when the target journey has not yet been completed (i.e., is still recording). The service must reject replays of in-progress journeys.

**Parameters:** None.

**Return Value:** A `Task` representing the asynchronous test operation.

**Throws:** The test expects the method under test to throw; the test itself fails if no exception is raised or if the wrong exception type is thrown.

---

### public async Task ReplayJourneyAsync_ShouldThrow_WhenJourneyHasFewerThanTwoWaypoints

Validates that `ReplayJourneyAsync` throws when the journey contains fewer than two waypoints. A replay requires at least a start and an end point to produce meaningful frames.

**Parameters:** None.

**Return Value:** A `Task` representing the asynchronous test operation.

**Throws:** The test expects the method under test to throw; the test itself fails if no exception is raised or if the wrong exception type is thrown.

---

### public async Task ReplayJourneyAsync_ShouldSliceWaypoints_WhenStartAndEndIndexSet

Demonstrates that supplying start and end indices to `ReplayJourneyAsync` restricts the replay to the specified waypoint range. Frames outside the slice must be excluded, and the frame count must reflect only the sliced segment.

**Parameters:** None.

**Return Value:** A `Task` representing the asynchronous test operation.

**Throws:** Assertion failures if the replay includes waypoints outside the requested range or omits waypoints within it.

---

### public async Task ReplayJourneyAsync_ShouldRebaseTimestamps_WhenRebaseToUtcSet

Checks that when a `rebaseToUtc` parameter is provided, all emitted frame timestamps are shifted so that the first frame aligns with the given UTC instant. Subsequent frames must preserve relative offsets from that new origin.

**Parameters:** None.

**Return Value:** A `Task` representing the asynchronous test operation.

**Throws:** Assertion failures if the first timestamp does not match the rebase target or if relative deltas are distorted.

---

### public async Task GetReplaySummaryAsync_ShouldReturnCorrectWaypointCount

Tests that `GetReplaySummaryAsync` returns a summary object whose waypoint count equals the total number of waypoints in the stored journey. The summary must accurately reflect the underlying data.

**Parameters:** None.

**Return Value:** A `Task` representing the asynchronous test operation.

**Throws:** Assertion failures if the summary's waypoint count is incorrect.

## Usage

### Example 1: Verifying frame count for a complete replay

```csharp
[TestMethod]
public async Task ReplayJourneyAsync_ShouldReturnCorrectFrameCount()
{
    // Arrange
    var journey = new Journey
    {
        Id = Guid.NewGuid(),
        State = JourneyState.Completed,
        Waypoints = GenerateWaypoints(count: 10)
    };
    var service = new RouteReplayService(journeyRepository);

    // Act
    var frames = await service.ReplayJourneyAsync(journey.Id).ToListAsync();

    // Assert
    Assert.AreEqual(10, frames.Count);
}
```

### Example 2: Confirming exception for an ongoing journey

```csharp
[TestMethod]
public async Task ReplayJourneyAsync_ShouldThrow_WhenJourneyIsOngoing()
{
    // Arrange
    var journey = new Journey
    {
        Id = Guid.NewGuid(),
        State = JourneyState.Ongoing,
        Waypoints = GenerateWaypoints(count: 5)
    };
    var service = new RouteReplayService(journeyRepository);

    // Act & Assert
    await Assert.ThrowsExceptionAsync<InvalidOperationException>(
        () => service.ReplayJourneyAsync(journey.Id).ToListAsync()
    );
}
```

## Notes

- **Edge Cases:** Journeys with exactly two waypoints are the minimum valid input for replay; tests explicitly reject journeys with one or zero waypoints. Slicing with start and end indices that are equal or inverted should be handled according to the service's contract (typically resulting in an empty range or an argument exception), though these specific boundary scenarios are not covered by the listed members.
- **Thread Safety:** These test methods are designed for single-threaded test execution. The underlying `RouteReplayService` may or may not be thread-safe; the tests do not exercise concurrent replay calls on the same journey. Consumers should assume instance methods are not safe for parallel invocation unless explicitly documented by the service itself.
- **Time Manipulation:** The speed multiplier and rebase operations are independent; applying both simultaneously should compose correctly (compressed intervals relative to the rebased origin). Tests validate each transformation in isolation.
