#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpsTrackerProtocol.Tests
{
    /// <summary>
    /// System.Text.Json serialization extensions for DeviceDiagnosticsServiceTests.
    /// </summary>
    public static class DeviceDiagnosticsServiceTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Serializes the DeviceDiagnosticsServiceTests instance to a JSON string.
        /// </summary>
        /// <param name="value">The DeviceDiagnosticsServiceTests instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>A JSON string representation of the DeviceDiagnosticsServiceTests instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
        public static string ToJson(this DeviceDiagnosticsServiceTests value, bool indented = false)
            => value is null
                ? throw new ArgumentNullException(nameof(value))
                : JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);

        /// <summary>
        /// Deserializes a JSON string to a DeviceDiagnosticsServiceTests instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A DeviceDiagnosticsServiceTests instance, or <see langword="null"/> if the JSON is invalid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
        public static DeviceDiagnosticsServiceTests? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            return string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<DeviceDiagnosticsServiceTests>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a DeviceDiagnosticsServiceTests instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The resulting DeviceDiagnosticsServiceTests instance, or <see langword="null"/> if deserialization fails.</param>
        /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
        public static bool TryFromJson(string json, out DeviceDiagnosticsServiceTests? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            value = null;

            return !string.IsNullOrWhiteSpace(json)
                && (value = JsonSerializer.Deserialize<DeviceDiagnosticsServiceTests>(json, _jsonOptions)) is not null;
        }
    }
}