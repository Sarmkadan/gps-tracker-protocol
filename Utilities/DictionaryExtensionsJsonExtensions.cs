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
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson(this Dictionary<string, object> value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes dictionary from JSON string.
        /// </summary>
        /// <param name="json">JSON string to deserialize.</param>
        /// <returns>Deserialized dictionary, or <see langword="null"/> if deserialization fails.</returns>
        /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>, empty, or whitespace.</exception>
        public static Dictionary<string, object>? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

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
        /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>, empty, or whitespace.</exception>
        public static bool TryFromJson(string json, out Dictionary<string, object>? value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            try
            {
                value = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}