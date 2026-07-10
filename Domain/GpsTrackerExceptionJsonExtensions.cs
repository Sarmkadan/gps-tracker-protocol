#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace GpsTrackerProtocol.Domain;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for GpsTrackerException and derived types.
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
    /// Serializes a GpsTrackerException to JSON string.
    /// </summary>
    /// <param name="value">The exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the exception.</returns>
    public static string ToJson(this GpsTrackerException value, bool indented = false)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a GpsTrackerException from JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>The deserialized exception, or null if JSON is null or empty.</returns>
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

            if (result != null)
            {
                return result;
            }

            // If deserialization returns null, try to deserialize as a derived type
            // We'll need to handle this by checking the $type discriminator or try each derived type
            return TryDeserializeDerivedType(json);
        }
        catch (JsonException)
        {
            // If deserialization fails, try to deserialize as a derived type
            return TryDeserializeDerivedType(json);
        }
    }

    /// <summary>
    /// Attempts to deserialize a GpsTrackerException from JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">The deserialized exception, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out GpsTrackerException? value)
    {
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
        catch (ArgumentNullException)
        {
            value = null;
            return false;
        }
    }

    private static GpsTrackerException? TryDeserializeDerivedType(string json)
    {
        try
        {
            // Try to deserialize as ParseException
            var parseException = JsonSerializer.Deserialize<ParseException>(json, _jsonOptions);
            if (parseException != null)
            {
                return parseException;
            }

            // Try to deserialize as ChecksumException
            var checksumException = JsonSerializer.Deserialize<ChecksumException>(json, _jsonOptions);
            if (checksumException != null)
            {
                return checksumException;
            }

            // Try to deserialize as DeviceException
            var deviceException = JsonSerializer.Deserialize<DeviceException>(json, _jsonOptions);
            if (deviceException != null)
            {
                return deviceException;
            }

            // Try to deserialize as CommandException
            var commandException = JsonSerializer.Deserialize<CommandException>(json, _jsonOptions);
            if (commandException != null)
            {
                return commandException;
            }

            // Try to deserialize as ValidationException
            var validationException = JsonSerializer.Deserialize<ValidationException>(json, _jsonOptions);
            if (validationException != null)
            {
                return validationException;
            }

            // Try to deserialize as RepositoryException
            var repositoryException = JsonSerializer.Deserialize<RepositoryException>(json, _jsonOptions);
            if (repositoryException != null)
            {
                return repositoryException;
            }

            // Try to deserialize as TimeoutException
            var timeoutException = JsonSerializer.Deserialize<TimeoutException>(json, _jsonOptions);
            if (timeoutException != null)
            {
                return timeoutException;
            }
        }
        catch (JsonException)
        {
            // Silently fail - all attempts failed
        }

        return null;
    }
}