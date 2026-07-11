#nullable enable
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
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dict">The dictionary to search.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="defaultValue">The default value to return if the key is not found.</param>
    /// <returns>The value associated with the specified key, or the default value if the key is not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dict"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    public static TValue? GetValueOrDefault<TKey, TValue>(
        this Dictionary<TKey, TValue> dict, TKey key, TValue? defaultValue = default)
    where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dict);
        ArgumentNullException.ThrowIfNull(key);

        return dict.TryGetValue(key, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Safely gets and parses value to integer.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <param name="dict">The dictionary to search.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="defaultValue">The default value to return if parsing fails.</param>
    /// <returns>The parsed integer value, or the default value if parsing fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dict"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    public static int GetIntOrDefault<TKey>(this Dictionary<TKey, object> dict, TKey key, int defaultValue = 0)
    where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dict);
        ArgumentNullException.ThrowIfNull(key);

        if (dict.TryGetValue(key, out var value))
        {
            return value switch
            {
                int intVal => intVal,
                _ when int.TryParse(value?.ToString(), out var intVal) => intVal,
                _ => defaultValue
            };
        }

        return defaultValue;
    }

    /// <summary>
    /// Safely gets and parses value to double.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <param name="dict">The dictionary to search.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="defaultValue">The default value to return if parsing fails.</param>
    /// <returns>The parsed double value, or the default value if parsing fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dict"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    public static double GetDoubleOrDefault<TKey>(this Dictionary<TKey, object> dict, TKey key, double defaultValue = 0)
    where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dict);
        ArgumentNullException.ThrowIfNull(key);

        if (dict.TryGetValue(key, out var value))
        {
            return value switch
            {
                double doubleVal => doubleVal,
                _ when double.TryParse(value?.ToString(), out var doubleVal) => doubleVal,
                _ => defaultValue
            };
        }

        return defaultValue;
    }

    /// <summary>
    /// Safely gets and parses value to string.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <param name="dict">The dictionary to search.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>The string representation of the value, or empty string if not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dict"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    public static string GetStringOrEmpty<TKey>(this Dictionary<TKey, object> dict, TKey key)
    where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dict);
        ArgumentNullException.ThrowIfNull(key);

        return dict.TryGetValue(key, out var value) ? value?.ToString() ?? "" : "";
    }

    /// <summary>
    /// Safely gets and parses value to boolean.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <param name="dict">The dictionary to search.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="defaultValue">The default value to return if parsing fails.</param>
    /// <returns>The parsed boolean value, or the default value if parsing fails.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dict"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    public static bool GetBoolOrDefault<TKey>(this Dictionary<TKey, object> dict, TKey key, bool defaultValue = false)
    where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dict);
        ArgumentNullException.ThrowIfNull(key);

        if (dict.TryGetValue(key, out var value))
        {
            return value switch
            {
                bool boolVal => boolVal,
                _ when bool.TryParse(value?.ToString(), out var boolVal) => boolVal,
                _ => defaultValue
            };
        }

        return defaultValue;
    }

    /// <summary>
    /// Merges another dictionary into this one.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionaries.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionaries.</typeparam>
    /// <param name="target">The target dictionary to merge into.</param>
    /// <param name="source">The source dictionary to merge from.</param>
    /// <param name="overwrite">Whether to overwrite existing values in the target dictionary.</param>
    /// <exception cref="ArgumentNullException"><paramref name="target"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static void Merge<TKey, TValue>(
        this Dictionary<TKey, TValue> target, Dictionary<TKey, TValue> source, bool overwrite = true)
    where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(source);

        foreach (var kvp in source)
        {
            if (!target.ContainsKey(kvp.Key) || overwrite)
            {
                target[kvp.Key] = kvp.Value;
            }
        }
    }

    /// <summary>
    /// Flattens nested dictionary into flat key-value pairs.
    /// Keys are concatenated with dots (e.g., "parent.child").
    /// </summary>
    /// <param name="dict">The dictionary to flatten.</param>
    /// <param name="prefix">The prefix to prepend to keys.</param>
    /// <returns>A flattened dictionary with dot-separated keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dict"/> is <see langword="null"/>.</exception>
    public static Dictionary<string, object> Flatten(
        this Dictionary<string, object> dict, string prefix = "")
    {
        ArgumentNullException.ThrowIfNull(dict);

        var result = new Dictionary<string, object>();

        foreach (var kvp in dict)
        {
            var key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";

            if (kvp.Value is Dictionary<string, object> nested)
            {
                foreach (var nestedKvp in nested.Flatten(key))
                {
                    result.Add(nestedKvp.Key, nestedKvp.Value);
                }
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
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <param name="dict">The dictionary to convert.</param>
    /// <returns>A query string representation of the dictionary.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dict"/> is <see langword="null"/>.</exception>
    public static string ToQueryString<TKey>(this Dictionary<TKey, object> dict)
    where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dict);

        return string.Join("&", dict.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key.ToString()!)}={Uri.EscapeDataString(kvp.Value?.ToString() ?? "")}"));
    }
}