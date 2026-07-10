# InMemoryRepository

A generic in-memory repository implementation for storing and querying entities, primarily used for location data in the GPS tracker protocol system. It provides asynchronous CRUD operations and specialized queries for location-based data, with all operations executing against an in-memory collection.

## API

### `public virtual async Task<T?> GetByIdAsync(int id)`

Retrieves an entity by its unique identifier. Returns `null` if no entity with the specified ID exists.

- **Parameters**: `id` – The unique identifier of the entity to retrieve.
- **Return value**: The entity of type `T` if found; otherwise, `null`.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if `id` is negative.

---

### `public virtual async Task<IEnumerable<T>> GetAllAsync()`

Retrieves all entities stored in the repository.

- **Return value**: An enumerable collection of all entities of type `T`.
- **Exceptions**: None.

---

### `public virtual async Task<T> CreateAsync(T entity)`

Adds a new entity to the repository. The entity must not already exist (based on its ID).

- **Parameters**: `entity` – The entity to add.
- **Return value**: The added entity of type `T`.
- **Exceptions**: Throws `ArgumentNullException` if `entity` is `null`. Throws `InvalidOperationException` if an entity with the same ID already exists.

---

### `public virtual async Task<T> UpdateAsync(T entity)`

Updates an existing entity in the repository. The entity must already exist (based on its ID).

- **Parameters**: `entity` – The updated entity.
- **Return value**: The updated entity of type `T`.
- **Exceptions**: Throws `ArgumentNullException` if `entity` is `null`. Throws `KeyNotFoundException` if no entity with the specified ID exists.

---
### `public virtual async Task<bool> DeleteAsync(int id)`

Removes an entity from the repository by its unique identifier.

- **Parameters**: `id` – The unique identifier of the entity to remove.
- **Return value**: `true` if the entity was found and removed; otherwise, `false`.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if `id` is negative.

---
### `public virtual async Task<bool> ExistsAsync(int id)`

Checks whether an entity with the specified identifier exists in the repository.

- **Parameters**: `id` – The unique identifier to check.
- **Return value**: `true` if an entity with the specified `id` exists; otherwise, `false`.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if `id` is negative.

---
### `public async Task<IEnumerable<LocationData>> GetByDeviceIdAsync(string deviceId)`

Retrieves all location data entries associated with a specific device.

- **Parameters**: `deviceId` – The unique identifier of the device.
- **Return value**: An enumerable collection of `LocationData` entries for the specified device.
- **Exceptions**: Throws `ArgumentException` if `deviceId` is `null` or whitespace.

---
### `public async Task<IEnumerable<LocationData>> GetByTimeRangeAsync(DateTime start, DateTime end)`

Retrieves all location data entries within a specified time range.

- **Parameters**:
  - `start` – The start of the time range (inclusive).
  - `end` – The end of the time range (inclusive).
- **Return value**: An enumerable collection of `LocationData` entries within the specified time range.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if `start` is after `end`.

---
### `public async Task<IEnumerable<LocationData>> GetByDeviceAndTimeRangeAsync(string deviceId, DateTime start, DateTime end)`

Retrieves all location data entries for a specific device within a specified time range.

- **Parameters**:
  - `deviceId` – The unique identifier of the device.
  - `start` – The start of the time range (inclusive).
  - `end` – The end of the time range (inclusive).
- **Return value**: An enumerable collection of `LocationData` entries matching the criteria.
- **Exceptions**:
  - Throws `ArgumentException` if `deviceId` is `null` or whitespace.
  - Throws `ArgumentOutOfRangeException` if `start` is after `end`.

---
### `public async Task<LocationData?> GetLatestByDeviceIdAsync(string deviceId)`

Retrieves the most recent location data entry for a specific device.

- **Parameters**: `deviceId` – The unique identifier of the device.
- **Return value**: The latest `LocationData` entry for the specified device, or `null` if none exists.
- **Exceptions**: Throws `ArgumentException` if `deviceId` is `null` or whitespace.

---
### `public async Task<IEnumerable<LocationData>> GetWithinRadiusAsync(double latitude, double longitude, double radiusKm)`

Retrieves all location data entries within a specified geographic radius from a given coordinate.

- **Parameters**:
  - `latitude` – The latitude of the center point.
  - `longitude` – The longitude of the center point.
  - `radiusKm` – The radius in kilometers.
- **Return value**: An enumerable collection of `LocationData` entries within the specified radius.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if `radiusKm` is negative.

---
### `public async Task<int> DeleteOlderThanAsync(DateTime threshold)`

Removes all location data entries older than a specified threshold date.

- **Parameters**: `threshold` – The cutoff date (exclusive).
- **Return value**: The number of entries removed.
- **Exceptions**: None.

## Usage

### Example 1: Basic CRUD operations
