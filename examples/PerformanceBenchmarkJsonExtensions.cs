using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpsTracker.Protocol.Examples
{
    public static class PerformanceBenchmarkJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        public static string ToJson(this PerformanceBenchmark value, bool indented = false)
        {
            if (value == null)
            {
                return "{}";
            }

            var options = _jsonOptions with { WriteIndented = indented };
            return JsonSerializer.Serialize(value, options);
        }

        public static PerformanceBenchmark? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<PerformanceBenchmark>(json, _jsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static bool TryFromJson(string json, out PerformanceBenchmark? value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<PerformanceBenchmark>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}