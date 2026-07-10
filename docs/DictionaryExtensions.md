# DictionaryExtensions
Provides a set of convenience extension methods for working with `IDictionary` instances in the GPS‑Tracker‑Protocol library. The members simplify common operations such as safe value retrieval with defaults, merging dictionaries, flattening nested structures, and converting a dictionary to a query string.

## API
### GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
- **Purpose**: Returns the value associated with `key` if it exists; otherwise returns the default value for `TValue`.
- **Parameters**:
  - `dictionary`: The dictionary to search.
  - `key`: The key whose value is to be retrieved.
- **Return value**: The value for `key`, or `default(TValue)` when the key is absent.
- **Exceptions**: `ArgumentNullException` if `dictionary` is `null`.

### GetIntOrDefault<TKey>(this IDictionary<TKey, object> dictionary, TKey key)
- **Purpose**: Attempts to retrieve the value for `key` and convert it to an `Int32`. Returns `0` if the key is missing, the value is `null`, or conversion fails.
- **Parameters**:
  - `dictionary`: The dictionary containing the value.
  - `key`: The key to look up.
- **Return value**: The integer representation of the value, or `0` on failure/default.
- **Exceptions**: `ArgumentNullException` if `dictionary` is `null`.

### GetDoubleOrDefault<TKey>(this IDictionary<TKey, object> dictionary, TKey key)
- **Purpose**: Attempts to retrieve the value for `key` and convert it to a `Double`. Returns `0.0` if the key is missing, the value is `null`, or conversion fails.
- **Parameters**:
  - `dictionary`: The dictionary to inspect.
  - `key`: The key whose value is desired.
- **Return value**: The double representation of the value, or `0.0` on failure/default.
- **Exceptions**: `ArgumentNullException` if `dictionary` is `null`.

### GetStringOrEmpty<TKey>(this IDictionary<TKey, object> dictionary, TKey key)
- **Purpose**: Retrieves the string value for `key`. Returns `string.Empty` if the key is missing or the associated value is `null`.
- **Parameters**:
  - `dictionary`: The dictionary to query.
  - `key`: The key to locate.
- **Return value**: The string value for `key`, or `string.Empty` when absent/null.
- **Exceptions**: `ArgumentNullException` if `dictionary` is `null`.

### GetBoolOrDefault<TKey>(this IDictionary<TKey, object> dictionary, TKey key)
- **Purpose**: Attempts to retrieve the value for `key` and interpret it as a Boolean. Returns `false` if the key is missing, the value is `null`, or the value cannot be parsed as a Boolean.
- **Parameters**:
  - `dictionary`: The dictionary to read from.
  - `key`: The key whose value is desired.
- **Return value**: The Boolean value for `key`, or `false` on failure/default.
- **Exceptions**: `ArgumentNullException` if `dictionary` is `null`.

### Merge<TKey, TValue>(this IDictionary<TKey, TValue> target, IDictionary<TKey, TValue> source)
- **Purpose**: Copies all entries from `source` into `target`. If a key already exists in `target`, its value is overwritten with the value from `source`.
- **Parameters**:
  - `target`: The dictionary that will receive the merged entries.
  - `source`: The dictionary whose entries are to be merged.
- **Return value**: None (the operation modifies `target` in place).
- **Exceptions**: 
  - `ArgumentNullException` if `target` is `null`.
  - `ArgumentNullException` if `source` is `null`.

### Flatten(this IDictionary<string, object> dictionary)
- **Purpose**: Produces a new dictionary where any nested `IDictionary<string, object>` values are flattened into the result using dot‑notation for keys (e.g., `{"a": {"b": 1}}` becomes `{"a.b": 1}`). Non‑dictionary values are copied unchanged.
- **Parameters**:
  - `dictionary`: The dictionary to flatten.
- **Return value**: A new `Dictionary<string, object>` containing the flattened key‑value pairs.
- **Exceptions**: `ArgumentNullException` if `dictionary` is `null`.

### ToQueryString<TKey>(this IDictionary<TKey, object> dictionary)
- **Purpose**: Serializes the dictionary into a URL query string (`application/x-www-form-urlencoded`). Keys and values are UTF‑8 encoded and joined with `&`. Entries with a `null` value are omitted.
- **Parameters**:
  - `dictionary`: The dictionary to serialize.
- **Return value**: A query string such as `"key1=value1&key2=value2"`. Returns an empty string if the dictionary contains no usable entries.
- **Exceptions**: `ArgumentNullException` if `dictionary` is `null`.

## Usage
```csharp
using System.Collections.Generic;

// Example 1: Safe retrieval and merging
var settings = new Dictionary<string, object>
{
    ["timeout"] = 30,
    ["retries"] = "3",
    ["enabled"] = true
};

int timeout = settings.GetIntOrDefault("timeout");          // 30
int maxRetries = settings.GetIntOrDefault("maxRetries");   // 0 (missing)
string enabledStr = settings.GetStringOrEmpty("enabled");  // "True"
bool isEnabled = settings.GetBoolOrDefault("enabled");     // true

var overrides = new Dictionary<string, object> { ["retries"] = "5", ["verbose"] = false };
settings.Merge(overrides);
// settings now contains: timeout=30, retries=5, enabled=true, verbose=false
```

```csharp
using System.Collections.Generic;

// Example 2: Flattening nested data and building a query string
var nested = new Dictionary<string, object>
{
    ["user"] = new Dictionary<string, object>
    {
        ["id"] = 42,
        ["name"] = "Alice",
        ["address"] = new Dictionary<string, object>
        {
            ["city"] = "Wonderland",
            ["zip"] = "12345"
        }
    },
    ["active"] = true
};

var flat = nested.Flatten();
// flat contains:
//   "user.id" -> 42
//   "user.name" -> "Alice"
//   "user.address.city" -> "Wonderland"
//   "user.address.zip" -> "12345"
//   "active" -> true

string query = flat.ToQueryString();
// query => "user.id=42&user.name=Alice&user.address.city=Wonderland&user.address.zip=12345&active=True"
```

## Notes
- All methods that accept a dictionary argument throw `ArgumentNullException` when the argument is `null`; they do not perform any additional null‑checking on keys or values unless explicitly described.
- `GetIntOrDefault`, `GetDoubleOrDefault`, `GetStringOrEmpty`, and `GetBoolOrDefault` return their respective default values (`0`, `0.0`, `string.Empty`, `false`) when the key is absent, the associated value is `null`, or the value cannot be converted/parsed.
- `Merge` modifies the `target` dictionary; callers must ensure exclusive access to `target` if concurrent modifications are possible. The method itself does not lock or synchronize access.
- `Flatten` only flattens entries whose values are of type `IDictionary<string, object>`; other nested types (e.g., arrays, custom classes) are copied as‑is without further decomposition.
- `ToQueryString` skips entries with a `null` value. It uses `Uri.EscapeDataString` to encode keys and values, ensuring the result is safe for inclusion in a URL. The method does not add a leading `?`.
- None of the extension methods retain internal state; they are thread‑safe for concurrent read‑only operations on the source dictionary, provided the dictionary itself is not being modified during the call. Operations that mutate a dictionary (`Merge`) require external synchronization.
