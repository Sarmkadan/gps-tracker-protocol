#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;

namespace GpsTrackerProtocol.Utilities;

/// <summary>
/// Provides JSON serialization and deserialization extensions for string operations.
/// </summary>
public static class StringExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a string to its JSON string representation.
    /// </summary>
    /// <param name="value">The string to serialize. If <see langword="null"/>, returns <c>"null"</c>.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the input value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this string value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a string from its JSON string representation.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>The deserialized string, or <see langword="null"/> if the input is null, empty, or contains only whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the input is not valid JSON.</exception>
    public static string? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<string>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a string from its JSON string representation.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized string if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out string? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            value = null;
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<string>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}