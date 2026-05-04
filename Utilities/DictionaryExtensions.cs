// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Utilities;

/// <summary>
/// Dictionary extension methods for safe and convenient access.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Gets value from dictionary safely with default value.
    /// </summary>
    public static TValue? GetValueOrDefault<TKey, TValue>(
        this Dictionary<TKey, TValue> dict, TKey key, TValue? defaultValue = default)
        where TKey : notnull
    {
        return dict.ContainsKey(key) ? dict[key] : defaultValue;
    }

    /// <summary>
    /// Safely gets and parses value to integer.
    /// </summary>
    public static int GetIntOrDefault<TKey>(this Dictionary<TKey, object> dict, TKey key, int defaultValue = 0)
        where TKey : notnull
    {
        if (dict.TryGetValue(key, out var value))
        {
            if (value is int intVal)
                return intVal;
            if (int.TryParse(value?.ToString(), out intVal))
                return intVal;
        }
        return defaultValue;
    }

    /// <summary>
    /// Safely gets and parses value to double.
    /// </summary>
    public static double GetDoubleOrDefault<TKey>(this Dictionary<TKey, object> dict, TKey key, double defaultValue = 0)
        where TKey : notnull
    {
        if (dict.TryGetValue(key, out var value))
        {
            if (value is double doubleVal)
                return doubleVal;
            if (double.TryParse(value?.ToString(), out doubleVal))
                return doubleVal;
        }
        return defaultValue;
    }

    /// <summary>
    /// Safely gets and parses value to string.
    /// </summary>
    public static string GetStringOrEmpty<TKey>(this Dictionary<TKey, object> dict, TKey key)
        where TKey : notnull
    {
        return dict.TryGetValue(key, out var value) ? value?.ToString() ?? "" : "";
    }

    /// <summary>
    /// Safely gets and parses value to boolean.
    /// </summary>
    public static bool GetBoolOrDefault<TKey>(this Dictionary<TKey, object> dict, TKey key, bool defaultValue = false)
        where TKey : notnull
    {
        if (dict.TryGetValue(key, out var value))
        {
            if (value is bool boolVal)
                return boolVal;
            if (bool.TryParse(value?.ToString(), out boolVal))
                return boolVal;
        }
        return defaultValue;
    }

    /// <summary>
    /// Merges another dictionary into this one.
    /// </summary>
    public static void Merge<TKey, TValue>(
        this Dictionary<TKey, TValue> target, Dictionary<TKey, TValue> source, bool overwrite = true)
        where TKey : notnull
    {
        foreach (var kvp in source)
        {
            if (!target.ContainsKey(kvp.Key) || overwrite)
                target[kvp.Key] = kvp.Value;
        }
    }

    /// <summary>
    /// Flattens nested dictionary into flat key-value pairs.
    /// Keys are concatenated with dots (e.g., "parent.child").
    /// </summary>
    public static Dictionary<string, object> Flatten(
        this Dictionary<string, object> dict, string prefix = "")
    {
        var result = new Dictionary<string, object>();

        foreach (var kvp in dict)
        {
            var key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";

            if (kvp.Value is Dictionary<string, object> nested)
            {
                foreach (var nestedKvp in nested.Flatten(key))
                    result.Add(nestedKvp.Key, nestedKvp.Value);
            }
            else
            {
                result[key] = kvp.Value;
            }
        }

        return result;
    }

    /// <summary>
    /// Converts dictionary to query string format.
    /// </summary>
    public static string ToQueryString<TKey>(this Dictionary<TKey, object> dict)
        where TKey : notnull
    {
        return string.Join("&", dict.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key.ToString())}={Uri.EscapeDataString(kvp.Value?.ToString() ?? "")}"));
    }
}
