#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Utilities;

/// <summary>
/// Validation helpers for StringExtensions methods and parsed values.
/// Provides comprehensive validation for NMEA parsing and device ID manipulation.
/// </summary>
public static class StringExtensionsValidation
{
    /// <summary>
    /// Validates parsed numeric values from StringExtensions parsing methods.
    /// </summary>
    /// <param name="parsedValue">The parsed double value to validate.</param>
    /// <param name="originalString">The original string that was parsed.</param>
    /// <param name="defaultValue">The default value used when parsing failed.</param>
    /// <returns>List of validation problems; empty list if valid.</returns>
    public static IReadOnlyList<string> Validate(
        double parsedValue,
        string originalString,
        double defaultValue = 0)
    {
        ArgumentNullException.ThrowIfNull(originalString);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(originalString))
        {
            problems.Add("Original string is null, empty, or whitespace");
        }

        if (parsedValue == defaultValue && !string.IsNullOrWhiteSpace(originalString))
        {
            problems.Add("Parsing returned default value, indicating potential parsing failure");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parsed integer values from StringExtensions parsing methods.
    /// </summary>
    /// <param name="parsedValue">The parsed integer value to validate.</param>
    /// <param name="originalString">The original string that was parsed.</param>
    /// <param name="defaultValue">The default value used when parsing failed.</param>
    /// <returns>List of validation problems; empty list if valid.</returns>
    public static IReadOnlyList<string> Validate(
        int parsedValue,
        string originalString,
        int defaultValue = 0)
    {
        ArgumentNullException.ThrowIfNull(originalString);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(originalString))
        {
            problems.Add("Original string is null, empty, or whitespace");
        }

        if (parsedValue == defaultValue && !string.IsNullOrWhiteSpace(originalString))
        {
            problems.Add("Parsing returned default value, indicating potential parsing failure");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates an IMEI string format.
    /// </summary>
    /// <param name="imeiValue">The IMEI string to validate.</param>
    /// <returns>List of validation problems; empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateImei(string imeiValue)
    {
        ArgumentNullException.ThrowIfNull(imeiValue);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(imeiValue))
        {
            problems.Add("IMEI string is null, empty, or whitespace");
        }
        else if (imeiValue.Length < 15 || imeiValue.Length > 20)
        {
            problems.Add("IMEI must be between 15 and 20 digits long");
        }
        else if (!imeiValue.All(char.IsDigit))
        {
            problems.Add("IMEI must contain only digits");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a device ID string format.
    /// </summary>
    /// <param name="deviceIdValue">The device ID string to validate.</param>
    /// <returns>List of validation problems; empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateDeviceId(string deviceIdValue)
    {
        ArgumentNullException.ThrowIfNull(deviceIdValue);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(deviceIdValue))
        {
            problems.Add("Device ID string is null, empty, or whitespace");
        }
        else if (deviceIdValue.Length > 50)
        {
            problems.Add("Device ID must be 50 characters or less");
        }
        else if (!deviceIdValue.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'))
        {
            problems.Add("Device ID must contain only alphanumeric characters, dashes, or underscores");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates truncation parameters.
    /// </summary>
    /// <param name="maxLength">The maximum length for truncation.</param>
    /// <param name="suffix">The suffix to append when truncating.</param>
    /// <returns>List of validation problems; empty list if valid.</returns>
    public static IReadOnlyList<string> Validate(int maxLength, string suffix = "...")
    {
        ArgumentNullException.ThrowIfNull(suffix);

        var problems = new List<string>();

        if (maxLength <= 0)
        {
            problems.Add("maxLength must be greater than zero");
        }

        if (string.IsNullOrWhiteSpace(suffix))
        {
            problems.Add("Suffix is null, empty, or whitespace");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a hex string for conversion to byte array.
    /// </summary>
    /// <param name="hexValue">The hex string to validate.</param>
    /// <returns>List of validation problems; empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateHex(string hexValue)
    {
        ArgumentNullException.ThrowIfNull(hexValue);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(hexValue))
        {
            problems.Add("Hex string is null, empty, or whitespace");
        }
        else
        {
            var cleaned = hexValue.Replace("-", "").Replace(" ", "");
            if (cleaned.Length % 2 != 0)
            {
                problems.Add("Hex string must have an even number of characters after removing separators");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a device ID string for sanitization.
    /// </summary>
    /// <param name="deviceId">The device ID string to validate.</param>
    /// <returns>List of validation problems; empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateDeviceIdForSanitization(string deviceId)
    {
        ArgumentNullException.ThrowIfNull(deviceId);
        return ValidateDeviceId(deviceId);
    }

    /// <summary>
    /// Validates a hex color string.
    /// </summary>
    /// <param name="colorValue">The hex color string to validate.</param>
    /// <returns>List of validation problems; empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateColor(string colorValue)
    {
        ArgumentNullException.ThrowIfNull(colorValue);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(colorValue))
        {
            problems.Add("Color string is null, empty, or whitespace");
        }
        else
        {
            var trimmed = colorValue.TrimStart('#');
            if (trimmed.Length != 6 && trimmed.Length != 8)
            {
                problems.Add("Color must be 6 or 8 hex characters (excluding # prefix)");
            }
            else if (!trimmed.All(c => "0123456789ABCDEFabcdef".Contains(c)))
            {
                problems.Add("Color must contain only hexadecimal characters");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the parsed double value is valid.
    /// </summary>
    /// <param name="parsedValue">The parsed double value.</param>
    /// <param name="originalString">The original string that was parsed.</param>
    /// <param name="defaultValue">The default value used when parsing failed.</param>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    public static bool IsValid(
        double parsedValue,
        string originalString,
        double defaultValue = 0)
    {
        return Validate(parsedValue, originalString, defaultValue).Count == 0;
    }

    /// <summary>
    /// Determines whether the parsed integer value is valid.
    /// </summary>
    /// <param name="parsedValue">The parsed integer value.</param>
    /// <param name="originalString">The original string that was parsed.</param>
    /// <param name="defaultValue">The default value used when parsing failed.</param>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    public static bool IsValid(
        int parsedValue,
        string originalString,
        int defaultValue = 0)
    {
        return Validate(parsedValue, originalString, defaultValue).Count == 0;
    }

    /// <summary>
    /// Determines whether the IMEI string is valid.
    /// </summary>
    /// <param name="imeiValue">The IMEI string to validate.</param>
    /// <returns>True if the IMEI is valid; otherwise, false.</returns>
    public static bool IsValidImei(string imeiValue)
    {
        return ValidateImei(imeiValue).Count == 0;
    }

    /// <summary>
    /// Determines whether the device ID string is valid.
    /// </summary>
    /// <param name="deviceIdValue">The device ID string to validate.</param>
    /// <returns>True if the device ID is valid; otherwise, false.</returns>
    public static bool IsValidDeviceId(string deviceIdValue)
    {
        return ValidateDeviceId(deviceIdValue).Count == 0;
    }

    /// <summary>
    /// Determines whether the truncation parameters are valid.
    /// </summary>
    /// <param name="maxLength">The maximum length for truncation.</param>
    /// <param name="suffix">The suffix to append when truncating.</param>
    /// <returns>True if the parameters are valid; otherwise, false.</returns>
    public static bool IsValid(int maxLength, string suffix = "...")
    {
        return Validate(maxLength, suffix).Count == 0;
    }

    /// <summary>
    /// Determines whether the hex string is valid for conversion.
    /// </summary>
    /// <param name="hexValue">The hex string to validate.</param>
    /// <returns>True if the hex string is valid; otherwise, false.</returns>
    public static bool IsValidHex(string hexValue)
    {
        return ValidateHex(hexValue).Count == 0;
    }

    /// <summary>
    /// Determines whether the color string is valid.
    /// </summary>
    /// <param name="colorValue">The hex color string to validate.</param>
    /// <returns>True if the color is valid; otherwise, false.</returns>
    public static bool IsValidColor(string colorValue)
    {
        return ValidateColor(colorValue).Count == 0;
    }

    /// <summary>
    /// Ensures that the parsed double value is valid, throwing an exception if not.
    /// </summary>
    /// <param name="parsedValue">The parsed double value.</param>
    /// <param name="originalString">The original string that was parsed.</param>
    /// <param name="defaultValue">The default value used when parsing failed.</param>
    /// <exception cref="ArgumentException">Thrown if the value is invalid.</exception>
    public static void EnsureValid(
        double parsedValue,
        string originalString,
        double defaultValue = 0)
    {
        var problems = Validate(parsedValue, originalString, defaultValue);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Parsed double value validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Ensures that the parsed integer value is valid, throwing an exception if not.
    /// </summary>
    /// <param name="parsedValue">The parsed integer value.</param>
    /// <param name="originalString">The original string that was parsed.</param>
    /// <param name="defaultValue">The default value used when parsing failed.</param>
    /// <exception cref="ArgumentException">Thrown if the value is invalid.</exception>
    public static void EnsureValid(
        int parsedValue,
        string originalString,
        int defaultValue = 0)
    {
        var problems = Validate(parsedValue, originalString, defaultValue);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Parsed integer value validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Ensures that the IMEI string is valid, throwing an exception if not.
    /// </summary>
    /// <param name="imeiValue">The IMEI string to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the IMEI is invalid.</exception>
    public static void EnsureValidImei(string imeiValue)
    {
        var problems = ValidateImei(imeiValue);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"IMEI validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Ensures that the device ID string is valid, throwing an exception if not.
    /// </summary>
    /// <param name="deviceIdValue">The device ID string to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the device ID is invalid.</exception>
    public static void EnsureValidDeviceId(string deviceIdValue)
    {
        var problems = ValidateDeviceId(deviceIdValue);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Device ID validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Ensures that the truncation parameters are valid, throwing an exception if not.
    /// </summary>
    /// <param name="maxLength">The maximum length for truncation.</param>
    /// <param name="suffix">The suffix to append when truncating.</param>
    /// <exception cref="ArgumentException">Thrown if the parameters are invalid.</exception>
    public static void EnsureValid(int maxLength, string suffix = "...")
    {
        var problems = Validate(maxLength, suffix);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Truncation parameter validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Ensures that the hex string is valid for conversion, throwing an exception if not.
    /// </summary>
    /// <param name="hexValue">The hex string to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the hex string is invalid.</exception>
    public static void EnsureValidHex(string hexValue)
    {
        var problems = ValidateHex(hexValue);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Hex string validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Ensures that the color string is valid, throwing an exception if not.
    /// </summary>
    /// <param name="colorValue">The hex color string to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the color is invalid.</exception>
    public static void EnsureValidColor(string colorValue)
    {
        var problems = ValidateColor(colorValue);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Color validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}