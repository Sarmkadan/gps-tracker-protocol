#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace GpsTrackerProtocol.Domain;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for <see cref="GpsTrackerException"/> and derived types.
/// </summary>
public static class GpsTrackerExceptionJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes a <see cref="GpsTrackerException"/> to JSON string.
    /// </summary>
    /// <param name="value">The exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the exception.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this GpsTrackerException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a <see cref="GpsTrackerException"/> from JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>The deserialized exception, or <see langword="null"/> if JSON is <see langword="null"/> or empty.</returns>
    /// <exception cref="JsonException">Thrown when JSON deserialization fails for all attempted exception types.</exception>
    public static GpsTrackerException? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            // First try to deserialize as base GpsTrackerException
            var result = JsonSerializer.Deserialize<GpsTrackerException>(json, _jsonOptions);

            return result ?? TryDeserializeDerivedType(json);
        }
        catch (JsonException)
        {
            // If deserialization fails, try to deserialize as a derived type
            return TryDeserializeDerivedType(json);
        }
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="GpsTrackerException"/> from JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">The deserialized exception, or <see langword="null"/> if deserialization fails.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out GpsTrackerException? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    private static GpsTrackerException? TryDeserializeDerivedType(string json)
    {
        try
        {
            return json switch
            {
                _ when JsonSerializer.Deserialize<ParseException>(json, _jsonOptions) is { } parseException => parseException,
                _ when JsonSerializer.Deserialize<ChecksumException>(json, _jsonOptions) is { } checksumException => checksumException,
                _ when JsonSerializer.Deserialize<DeviceException>(json, _jsonOptions) is { } deviceException => deviceException,
                _ when JsonSerializer.Deserialize<CommandException>(json, _jsonOptions) is { } commandException => commandException,
                _ when JsonSerializer.Deserialize<ValidationException>(json, _jsonOptions) is { } validationException => validationException,
                _ when JsonSerializer.Deserialize<RepositoryException>(json, _jsonOptions) is { } repositoryException => repositoryException,
                _ when JsonSerializer.Deserialize<TimeoutException>(json, _jsonOptions) is { } timeoutException => timeoutException,
                _ => null
            };
        }
        catch (JsonException)
        {
            // Silently fail - all attempts failed
            return null;
        }
    }
}