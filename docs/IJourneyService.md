# IJourneyService

Defines the contract for managing journey tracking operations, including starting, tracking, completing, and querying journeys with waypoints.

## API

### `StartJourneyAsync`

Initiates a new journey with the current timestamp. The journey is considered ongoing until completed or explicitly abandoned.

**Returns**
A `Task<Journey>` representing the newly created journey.

**Exceptions**
Throws if the user already has an ongoing journey or if the service is unavailable.

---

### `GetOngoingJourneyAsync`

Retrieves the currently active journey, if one exists.

**Returns**
A `Task<Journey?>` containing the ongoing journey or `null` if none is active.

**Exceptions**
Throws if the service is unavailable.

---

### `AddWaypointAsync`

Appends a new waypoint to the ongoing journey with the current timestamp and provided coordinates.

**Parameters**
- `latitude` (double): Latitude of the waypoint.
- `longitude` (double): Longitude of the waypoint.

**Returns**
A `Task<bool>` indicating whether the waypoint was successfully added.

**Exceptions**
Throws if no journey is ongoing or if the coordinates are invalid.

---

### `CompleteJourneyAsync`

Finalizes the ongoing journey, marking it as completed with the current timestamp.

**Returns**
A `Task<Journey>` representing the completed journey.

**Exceptions**
Throws if no journey is ongoing or if the service is unavailable.

---
### `GetJourneyHistoryAsync`

Fetches all completed journeys in chronological order, most recent first.

**Returns**
A `Task<IEnumerable<Journey>>` containing the list of journeys.

**Exceptions**
Throws if the service is unavailable.

---
### `GetJourneyAsync`

Retrieves a specific journey by its unique identifier.

**Parameters**
- `journeyId` (Guid): The unique identifier of the journey.

**Returns**
A `Task<Journey?>` containing the requested journey or `null` if not found.

**Exceptions**
Throws if the identifier is invalid or the service is unavailable.

---
### `GetTotalDistanceAsync`

Calculates the total distance traveled across all completed journeys.

**Returns**
A `Task<double>` representing the total distance in kilometers.

**Exceptions**
Throws if the service is unavailable.

---
### `CleanupOldJourneysAsync`

Removes journeys older than the specified threshold in days, including their waypoints.

**Parameters**
- `olderThanDays` (int): The age threshold in days.

**Returns**
A `Task<int>` indicating the number of journeys removed.

**Exceptions**
Throws if the threshold is negative or the service is unavailable.

## Usage

### Starting and Completing a Journey
