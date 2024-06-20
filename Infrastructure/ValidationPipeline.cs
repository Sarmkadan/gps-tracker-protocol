// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Infrastructure;

using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Validation pipeline for GPS frames and location data.
/// Chains multiple validators and collects all validation errors.
/// </summary>
public interface IValidator<T>
{
    ValidationResult Validate(T item);
}

public interface IValidationPipeline
{
    ValidationResult ValidateFrame(GpsFrame frame);
    ValidationResult ValidateLocation(LocationData location);
    ValidationResult ValidateDevice(Device device);
}

public class ValidationPipeline : IValidationPipeline
{
    private readonly IValidator<GpsFrame> _frameValidator;
    private readonly IValidator<LocationData> _locationValidator;
    private readonly IValidator<Device> _deviceValidator;

    public ValidationPipeline(
        IValidator<GpsFrame> frameValidator,
        IValidator<LocationData> locationValidator,
        IValidator<Device> deviceValidator)
    {
        _frameValidator = frameValidator;
        _locationValidator = locationValidator;
        _deviceValidator = deviceValidator;
    }

    public ValidationResult ValidateFrame(GpsFrame frame)
    {
        return _frameValidator.Validate(frame);
    }

    public ValidationResult ValidateLocation(LocationData location)
    {
        return _locationValidator.Validate(location);
    }

    public ValidationResult ValidateDevice(Device device)
    {
        return _deviceValidator.Validate(device);
    }
}

public class FrameValidator : IValidator<GpsFrame>
{
    public ValidationResult Validate(GpsFrame frame)
    {
        var errors = new List<string>();

        if (frame == null)
            errors.Add("Frame cannot be null");
        else
        {
            if (frame.RawData == null || frame.RawData.Length == 0)
                errors.Add("Raw data cannot be empty");
            if (string.IsNullOrWhiteSpace(frame.FrameId))
                errors.Add("Frame ID is required");
            if (frame.Protocol == ProtocolType.Unknown)
                errors.Add("Protocol type must be known");
            if (frame.ReceivedAt > DateTime.UtcNow.AddSeconds(5))
                errors.Add("Received timestamp cannot be in the future");
        }

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}

public class LocationValidator : IValidator<LocationData>
{
    private const double MaxLatitude = 90.0;
    private const double MinLatitude = -90.0;
    private const double MaxLongitude = 180.0;
    private const double MinLongitude = -180.0;

    public ValidationResult Validate(LocationData location)
    {
        var errors = new List<string>();

        if (location == null)
            errors.Add("Location cannot be null");
        else
        {
            if (string.IsNullOrWhiteSpace(location.DeviceId))
                errors.Add("Device ID is required");
            if (location.Latitude < MinLatitude || location.Latitude > MaxLatitude)
                errors.Add($"Latitude must be between {MinLatitude} and {MaxLatitude}");
            if (location.Longitude < MinLongitude || location.Longitude > MaxLongitude)
                errors.Add($"Longitude must be between {MinLongitude} and {MaxLongitude}");
            if (location.Speed < 0)
                errors.Add("Speed cannot be negative");
            if (location.Bearing < 0 || location.Bearing > 360)
                errors.Add("Bearing must be between 0 and 360");
            if (location.SatelliteCount < 0)
                errors.Add("Satellite count cannot be negative");
            if (location.Timestamp > DateTime.UtcNow.AddSeconds(5))
                errors.Add("Timestamp cannot be in the future");
        }

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}

public class DeviceValidator : IValidator<Device>
{
    public ValidationResult Validate(Device device)
    {
        var errors = new List<string>();

        if (device == null)
            errors.Add("Device cannot be null");
        else
        {
            if (string.IsNullOrWhiteSpace(device.Id))
                errors.Add("Device ID is required");
            if (string.IsNullOrWhiteSpace(device.Imei))
                errors.Add("IMEI is required");
            if (!IsValidImei(device.Imei))
                errors.Add("Invalid IMEI format");
            if (device.Protocol == ProtocolType.Unknown)
                errors.Add("Protocol type must be known");
        }

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }

    private bool IsValidImei(string imei)
    {
        return imei.Length >= 10 && imei.Length <= 20 && imei.All(char.IsDigit);
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();

    public string GetErrorMessage() => string.Join("; ", Errors);
}
