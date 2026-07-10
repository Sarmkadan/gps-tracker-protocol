using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpsTrackerProtocol.Utilities
{
    public static class DictionaryExtensionsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes dictionary to JSON string.
        /// </summary>
        /// <param name="value">The dictionary to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>JSON string representation.</returns>
        public static string ToJson(this Dictionary<string, object> value, bool indented = false)
        {
            if (value == null)
            {
                return "{}";
            }

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes dictionary from JSON string.
        /// </summary>
        /// <param name="json">JSON string to deserialize.</param>
        /// <returns>Deserialized dictionary, or null if deserialization fails.</returns>
        public static Dictionary<string, object>? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json, _jsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize dictionary from JSON string.
        /// </summary>
        /// <param name="json">JSON string to deserialize.</param>
        /// <param name="value">Output parameter containing deserialized dictionary.</param>
        /// <returns>True if deserialization succeeds; otherwise, false.</returns>
        public static bool TryFromJson(string json, out Dictionary<string, object>? value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}