# ICachingService

A service interface for caching device data, locations, journeys, and commands in a distributed cache. It provides synchronous and asynchronous methods for setting, retrieving, and managing cached values, along with utilities for generating standardized cache keys.

## API

### `public CachingService(IInMemoryDistributedCache cache)`
Initializes a new instance of the `CachingService` with the specified in-memory distributed cache.

- **Parameters**
  - `cache`: The in-memory distributed cache instance used for storage.

### `public void Set<T>(string key, T value, TimeSpan? expiresIn = null)`
Stores a value in the cache with an optional expiration time.

- **Type Parameters**
  - `T`: The type of the value to cache.
- **Parameters**
  - `key`: The cache key.
  - `value`: The value to cache.
  - `expiresIn`: Optional time span after which the cache entry expires. If `null`, the entry does not expire.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` or `value` is `null`.
  - Throws `ArgumentException` if `key` is empty or whitespace.

### `public bool TryGet<T>(string key, out T value)`
Attempts to retrieve a cached value.

- **Type Parameters**
  - `T`: The type of the cached value.
- **Parameters**
  - `key`: The cache key.
  - `value`: Output parameter for the retrieved value. Set to `default` if the key does not exist.
- **Return Value**
  - `true` if the key exists and the value was retrieved; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.
  - Throws `ArgumentException` if `key` is empty or whitespace.

### `public void Remove(string key)`
Removes a cached entry by its key.

- **Parameters**
  - `key`: The cache key to remove.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.
  - Throws `ArgumentException` if `key` is empty or whitespace.

### `public void Clear()`
Removes all cached entries.

### `public IEnumerable<string> GetAllKeys()`
Retrieves all keys currently stored in the cache.

- **Return Value**
  - An enumerable of cache keys.

### `public object Value { get; }`
Gets the raw cached value associated with the current cache entry. Used internally for serialization/deserialization.

### `public DateTime? ExpiresAt { get; }`
Gets the expiration timestamp of the current cache entry, if set.

### `public DateTime CreatedAt { get; }`
Gets the creation timestamp of the current cache entry.

### `public static string GetDeviceKey(string deviceId)`
Generates a cache key for a device.

- **Parameters**
  - `deviceId`: The unique identifier of the device.
- **Return Value**
  - A cache key in the format `"device:{deviceId}"`.
- **Exceptions**
  - Throws `ArgumentNullException` if `deviceId` is `null`.
  - Throws `ArgumentException` if `deviceId` is empty or whitespace.

### `public static string GetDeviceListKey()`
Generates a cache key for the list of all devices.

- **Return Value**
  - A cache key in the format `"devices"`.
- **Exceptions**
  - Never throws.

### `public static string GetLocationKey(string deviceId, DateTime timestamp)`
Generates a cache key for a device's location at a specific timestamp.

- **Parameters**
  - `deviceId`: The unique identifier of the device.
  - `timestamp`: The timestamp of the location.
- **Return Value**
  - A cache key in the format `"location:{deviceId}:{timestamp:yyyyMMddHHmmss}"`.
- **Exceptions**
  - Throws `ArgumentNullException` if `deviceId` is `null`.
  - Throws `ArgumentException` if `deviceId` is empty or whitespace.

### `public static string GetLatestLocationKey(string deviceId)`
Generates a cache key for the latest location of a device.

- **Parameters**
  - `deviceId`: The unique identifier of the device.
- **Return Value**
  - A cache key in the format `"location:latest:{deviceId}"`.
- **Exceptions**
  - Throws `ArgumentNullException` if `deviceId` is `null`.
  - Throws `ArgumentException` if `deviceId` is empty or whitespace.

### `public static string GetLocationHistoryKey(string deviceId)`
Generates a cache key for the location history of a device.

- **Parameters**
  - `deviceId`: The unique identifier of the device.
- **Return Value**
  - A cache key in the format `"location:history:{deviceId}"`.
- **Exceptions**
  - Throws `ArgumentNullException` if `deviceId` is `null`.
  - Throws `ArgumentException` if `deviceId` is empty or whitespace.

### `public static string GetJourneyKey(string deviceId, Guid journeyId)`
Generates a cache key for a device's journey.

- **Parameters**
  - `deviceId`: The unique identifier of the device.
  - `journeyId`: The unique identifier of the journey.
- **Return Value**
  - A cache key in the format `"journey:{deviceId}:{journeyId}"`.
- **Exceptions**
  - Throws `ArgumentNullException` if `deviceId` or `journeyId` is `null`.
  - Throws `ArgumentException` if `deviceId` is empty or whitespace.

### `public static string GetJourneyHistoryKey(string deviceId)`
Generates a cache key for the journey history of a device.

- **Parameters**
  - `deviceId`: The unique identifier of the device.
- **Return Value**
  - A cache key in the format `"journey:history:{deviceId}"`.
- **Exceptions**
  - Throws `ArgumentNullException` if `deviceId` is `null`.
  - Throws `ArgumentException` if `deviceId` is empty or whitespace.

### `public static string GetCommandKey(string deviceId, Guid commandId)`
Generates a cache key for a command sent to a device.

- **Parameters**
  - `deviceId`: The unique identifier of the device.
  - `commandId`: The unique identifier of the command.
- **Return Value**
  - A cache key in the format `"command:{deviceId}:{commandId}"`.
- **Exceptions**
  - Throws `ArgumentNullException` if `deviceId` or `commandId` is `null`.
  - Throws `ArgumentException` if `deviceId` is empty or whitespace.

### `public static string GetDeviceCommandsKey(string deviceId)`
Generates a cache key for the list of commands sent to a device.

- **Parameters**
  - `deviceId`: The unique identifier of the device.
- **Return Value**
  - A cache key in the format `"command:device:{deviceId}"`.
- **Exceptions**
  - Throws `ArgumentNullException` if `deviceId` is `null`.
  - Throws `ArgumentException` if `deviceId` is empty or whitespace.

### `public InMemoryDistributedCache InMemoryDistributedCache { get; }`
Gets the underlying in-memory distributed cache instance.

### `public Task SetAsync<T>(string key, T value, TimeSpan? expiresIn = null)`
Asynchronously stores a value in the cache with an optional expiration time.

- **Type Parameters**
  - `T`: The type of the value to cache.
- **Parameters**
  - `key`: The cache key.
  - `value`: The value to cache.
  - `expiresIn`: Optional time span after which the cache entry expires. If `null`, the entry does not expire.
- **Return Value**
  - A `Task` representing the asynchronous operation.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` or `value` is `null`.
  - Throws `ArgumentException` if `key` is empty or whitespace.

## Usage

### Example 1: Storing and Retrieving a Device Location
