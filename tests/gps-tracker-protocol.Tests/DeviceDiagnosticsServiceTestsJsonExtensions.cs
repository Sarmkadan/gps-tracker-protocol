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
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
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
        public static string ToJson(this DeviceDiagnosticsServiceTests value, bool indented = false)
        {
            if (value == null)
            {
                return "{}";
            }

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions)
                {
                    WriteIndented = true
                }
                : _jsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a DeviceDiagnosticsServiceTests instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A DeviceDiagnosticsServiceTests instance, or null if the JSON is invalid.</returns>
        public static DeviceDiagnosticsServiceTests? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<DeviceDiagnosticsServiceTests>(json, _jsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a DeviceDiagnosticsServiceTests instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The resulting DeviceDiagnosticsServiceTests instance, or null if deserialization fails.</param>
        /// <returns>True if deserialization succeeds; otherwise, false.</returns>
        public static bool TryFromJson(string json, out DeviceDiagnosticsServiceTests? value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<DeviceDiagnosticsServiceTests>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
