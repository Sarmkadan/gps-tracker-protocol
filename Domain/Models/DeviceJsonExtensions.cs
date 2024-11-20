#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using GpsTrackerProtocol.Domain.Models;

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Provides System.Text.Json serialization extensions for the <see cref="Device"/> type.
/// </summary>
public static class DeviceJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};

	/// <summary>
	/// Serializes a Device instance to a JSON string.
	/// </summary>
	/// <param name="value">The device to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the device.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this Device value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonOptions)
			{
				WriteIndented = true
			} : _jsonOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a Device instance from a JSON string.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized Device instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
	/// <exception cref="JsonException">Thrown when the JSON is invalid and cannot be deserialized.</exception>
	public static Device FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(json);

		return JsonSerializer.Deserialize<Device>(json, _jsonOptions)
			?? throw new JsonException("Deserialization returned null for valid JSON input.");
	}

	/// <summary>
	/// Attempts to deserialize a Device instance from a JSON string.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized Device instance if successful.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	public static bool TryFromJson(string json, out Device? value)
	{
		value = null;

		if (string.IsNullOrWhiteSpace(json))
		{
			return false;
		}

		try
		{
			value = JsonSerializer.Deserialize<Device>(json, _jsonOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			return false;
		}
	}
}
