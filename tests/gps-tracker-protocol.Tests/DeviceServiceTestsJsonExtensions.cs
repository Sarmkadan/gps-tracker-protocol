#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace gps_tracker_protocol.Tests
{
    /// <summary>
    /// JSON serialization extensions for DeviceServiceTests.
    /// </summary>
    public static class DeviceServiceTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Serializes the DeviceServiceTests instance to a JSON string.
        /// </summary>
        /// <param name="value">The DeviceServiceTests instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>A JSON string representation of the DeviceServiceTests instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this DeviceServiceTests value, bool indented = false)
            => JsonSerializer.Serialize(value ?? throw new ArgumentNullException(nameof(value)), GetJsonOptions(indented));

        /// <summary>
        /// Deserializes a JSON string to a DeviceServiceTests instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A DeviceServiceTests instance, or null if the JSON is invalid or whitespace.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        public static DeviceServiceTests? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<DeviceServiceTests>(json, _jsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a DeviceServiceTests instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The resulting DeviceServiceTests instance, or null if deserialization fails.</param>
        /// <returns>True if deserialization succeeds; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        public static bool TryFromJson(string json, out DeviceServiceTests? value)
        {
            ArgumentNullException.ThrowIfNull(json);
            value = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<DeviceServiceTests>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the JSON serialization options with the specified indentation setting.
        /// </summary>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>Configured JsonSerializerOptions instance.</returns>
        private static JsonSerializerOptions GetJsonOptions(bool indented)
            => indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions;
    }
}