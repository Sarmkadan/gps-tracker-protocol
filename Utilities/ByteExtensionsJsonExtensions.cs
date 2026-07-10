#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace GpsTrackerProtocol.Utilities;

/// <summary>
/// System.Text.Json serialization extensions for byte array operations.
/// Provides JSON serialization/deserialization helpers for ByteExtensions type.
/// </summary>
public static class ByteExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes byte array to JSON string.
    /// </summary>
    /// <param name="value">The byte array to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation.</returns>
    public static string ToJson(this byte[] value, bool indented = false)
    {
        if (value is null)
            return "[]";

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes byte array from JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>Deserialized byte array, or null if deserialization fails.</returns>
    public static byte[]? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<byte[]>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize byte array from JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">Output parameter containing deserialized byte array.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJson(string json, out byte[]? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            value = JsonSerializer.Deserialize<byte[]>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}