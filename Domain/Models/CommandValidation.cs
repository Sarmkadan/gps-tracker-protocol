#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="Command"/> instances.
/// </summary>
public static class CommandValidation
{
    /// <summary>
    /// Validates a command and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The command to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    public static IReadOnlyList<string> Validate(this Command value)
    {
        if (value is null)
        {
            return ["Command cannot be null."];
        }

        var errors = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("Command.Id cannot be null or whitespace.");
        }
        else if (value.Id == Guid.Empty.ToString())
        {
            errors.Add("Command.Id cannot be an empty GUID.");
        }

        // Validate DeviceId
        if (string.IsNullOrWhiteSpace(value.DeviceId))
        {
            errors.Add("Command.DeviceId cannot be null or whitespace.");
        }
        else if (value.DeviceId.Length > 64)
        {
            errors.Add("Command.DeviceId cannot exceed 64 characters.");
        }

        // Validate Type
        if (value.Type == GpsTrackerProtocol.Domain.CommandType.Unknown)
        {
            errors.Add("Command.Type cannot be Unknown.");
        }

        // Validate Parameters
        if (value.Parameters is null)
        {
            errors.Add("Command.Parameters cannot be null.");
        }
        else
        {
            if (value.Parameters.Count > 20)
            {
                errors.Add("Command.Parameters cannot contain more than 20 entries.");
            }

            // Validate type-specific parameters
            switch (value.Type)
            {
                case GpsTrackerProtocol.Domain.CommandType.SetGpsInterval:
                    if (!value.Parameters.TryGetValue("interval", out var intervalObj) || intervalObj is not int)
                    {
                        errors.Add("Command for SetGpsInterval requires a valid 'interval' parameter of type int.");
                    }
                    else if (intervalObj is int interval && (interval <= 0 || interval > 86400))
                    {
                        errors.Add("Command.SetGpsInterval interval must be between 1 and 86400 seconds.");
                    }
                    break;

                case GpsTrackerProtocol.Domain.CommandType.SetReportingServer:
                    if (!value.Parameters.TryGetValue("server_ip", out var serverIpObj) || serverIpObj is not string serverIp || string.IsNullOrWhiteSpace(serverIp))
                    {
                        errors.Add("Command for SetReportingServer requires a valid 'server_ip' parameter of type string.");
                    }
                    else if (serverIp.Length > 255)
                    {
                        errors.Add("Command.SetReportingServer server_ip cannot exceed 255 characters.");
                    }

                    if (!value.Parameters.TryGetValue("port", out var portObj) || portObj is not int)
                    {
                        errors.Add("Command for SetReportingServer requires a valid 'port' parameter of type int.");
                    }
                    else if (portObj is int port && (port < 1 || port > 65535))
                    {
                        errors.Add("Command.SetReportingServer port must be between 1 and 65535.");
                    }
                    break;

                case GpsTrackerProtocol.Domain.CommandType.SetGeofence:
                    if (!value.Parameters.TryGetValue("latitude", out var latObj) || latObj is not double)
                    {
                        errors.Add("Command for SetGeofence requires a valid 'latitude' parameter of type double.");
                    }
                    else if (latObj is double lat && (lat < -90.0 || lat > 90.0))
                    {
                        errors.Add("Command.SetGeofence latitude must be between -90.0 and 90.0.");
                    }

                    if (!value.Parameters.TryGetValue("longitude", out var lonObj) || lonObj is not double)
                    {
                        errors.Add("Command for SetGeofence requires a valid 'longitude' parameter of type double.");
                    }
                    else if (lonObj is double lon && (lon < -180.0 || lon > 180.0))
                    {
                        errors.Add("Command.SetGeofence longitude must be between -180.0 and 180.0.");
                    }

                    if (!value.Parameters.TryGetValue("radius", out var radiusObj) || radiusObj is not int)
                    {
                        errors.Add("Command for SetGeofence requires a valid 'radius' parameter of type int.");
                    }
                    else if (radiusObj is int radius && (radius <= 0 || radius > 100000))
                    {
                        errors.Add("Command.SetGeofence radius must be between 1 and 100000 meters.");
                    }
                    break;
            }
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("Command.CreatedAt cannot be the default DateTime value.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("Command.CreatedAt cannot be in the future.");
        }

        // Validate ExecutedAt
        if (value.ExecutedAt.HasValue && value.ExecutedAt.Value > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("Command.ExecutedAt cannot be in the future.");
        }

        // Validate Status
        if (value.Status < GpsTrackerProtocol.Domain.CommandStatus.Pending || value.Status > GpsTrackerProtocol.Domain.CommandStatus.TimedOut)
        {
            errors.Add("Command.Status has an invalid value.");
        }

        // Validate Priority
        if (value.Priority < 0 || value.Priority > 100)
        {
            errors.Add("Command.Priority must be between 0 and 100.");
        }

        // Validate RetryCount
        if (value.RetryCount < 0)
        {
            errors.Add("Command.RetryCount cannot be negative.");
        }
        else if (value.RetryCount > value.MaxRetries + 5) // Allow some buffer
        {
            errors.Add("Command.RetryCount exceeds reasonable maximum for MaxRetries setting.");
        }

        // Validate CommandType string
        if (string.IsNullOrWhiteSpace(value.CommandType))
        {
            errors.Add("Command.CommandType cannot be null or whitespace.");
        }
        else if (value.CommandType.Length > 64)
        {
            errors.Add("Command.CommandType cannot exceed 64 characters.");
        }

        // Validate Payload
        if (value.Payload is null)
        {
            errors.Add("Command.Payload cannot be null.");
        }
        else if (value.Payload.Length > 1024)
        {
            errors.Add("Command.Payload cannot exceed 1024 characters.");
        }

        // Validate SentTime
        if (value.SentTime.HasValue && value.SentTime.Value > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("Command.SentTime cannot be in the future.");
        }

        // Validate MaxRetries
        if (value.MaxRetries < 0 || value.MaxRetries > 100)
        {
            errors.Add("Command.MaxRetries must be between 0 and 100.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the command is valid.
    /// </summary>
    /// <param name="value">The command to check.</param>
    /// <returns>True if the command is valid; otherwise, false.</returns>
    public static bool IsValid(this Command value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the command is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The command to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the command has validation errors.</exception>
    public static void EnsureValid(this Command value)
    {
        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Command validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}