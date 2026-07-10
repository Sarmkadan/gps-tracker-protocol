# IRouteReplayService

Provides functionality to replay and summarize recorded GPS journey data, enabling playback of routes with configurable speed and timing.

## API

### `RouteReplaySummary`

A record containing metadata about a replayed journey, including duration, distance, and timestamps.

### `RouteReplayService`

A service implementation of `IRouteReplayService` that handles the replay logic for GPS tracks.

### `async Task<RouteReplayResult> ReplayJourneyAsync`

Replays a recorded GPS journey with the specified parameters.

**Parameters:**
- `journeyId` (string): The unique identifier of the journey to replay.
- `speedMultiplier` (double, optional): A multiplier applied to the original journey speed. Defaults to `1.0` (original speed).
- `startTime` (DateTimeOffset?, optional): The UTC time at which the replay should start. If `null`, starts immediately.
- `cancellationToken` (CancellationToken, optional): A token to monitor for replay cancellation.

**Return value:**
A `RouteReplayResult` containing the replayed route data and metadata.

**Exceptions:**
- Throws `ArgumentException` if `journeyId` is null or empty.
- Throws `InvalidOperationException` if the journey cannot be found or is invalid.
- Throws `OperationCanceledException` if the operation is canceled via `cancellationToken`.

### `async Task<RouteReplaySummary> GetReplaySummaryAsync`

Retrieves a summary of a previously replayed journey.

**Parameters:**
- `replayId` (string): The unique identifier of the replay session.
- `cancellationToken` (CancellationToken, optional): A token to monitor for summary retrieval cancellation.

**Return value:**
A `RouteReplaySummary` containing metadata about the replayed journey.

**Exceptions:**
- Throws `ArgumentException` if `replayId` is null or empty.
- Throws `KeyNotFoundException` if the replay session does not exist.
- Throws `OperationCanceledException` if the operation is canceled via `cancellationToken`.

## Usage

### Replaying a journey with a custom speed multiplier
