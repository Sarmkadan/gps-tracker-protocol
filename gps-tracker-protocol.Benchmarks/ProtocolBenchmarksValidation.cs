using System;
using System.Collections.Generic;
using System.Linq;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

namespace GpsTrackerProtocol.Benchmarks;

/// <summary>
/// Validation helpers for ProtocolBenchmarks to ensure benchmark setup is valid
/// before execution. Validates all required dependencies and test data.
/// </summary>
public static class ProtocolBenchmarksValidation
{
    /// <summary>
    /// Validates a ProtocolBenchmarks instance and returns any validation errors.
    /// </summary>
    /// <param name="value">The ProtocolBenchmarks instance to validate</param>
    /// <returns>List of validation error messages, empty if valid</returns>
    public static IReadOnlyList<string> Validate(this ProtocolBenchmarks value)
    {
        var errors = new List<string>();

        if (value == null)
        {
            errors.Add("ProtocolBenchmarks instance cannot be null");
            return errors.AsReadOnly();
        }

        // Validate private fields that are used in benchmarks
        var providerField = typeof(ProtocolBenchmarks).GetField(
            "_provider",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var parserServiceField = typeof(ProtocolBenchmarks).GetField(
            "_parserService",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var deviceServiceField = typeof(ProtocolBenchmarks).GetField(
            "_deviceService",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var locationServiceField = typeof(ProtocolBenchmarks).GetField(
            "_locationService",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var analyticsServiceField = typeof(ProtocolBenchmarks).GetField(
            "_analyticsService",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var gt06FrameField = typeof(ProtocolBenchmarks).GetField(
            "_gt06Frame",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var h02FrameField = typeof(ProtocolBenchmarks).GetField(
            "_h02Frame",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var tk103FrameField = typeof(ProtocolBenchmarks).GetField(
            "_tk103Frame",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var frameBatchField = typeof(ProtocolBenchmarks).GetField(
            "_frameBatch",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var testDeviceField = typeof(ProtocolBenchmarks).GetField(
            "_testDevice",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Validate IServiceProvider
        if (providerField?.GetValue(value) == null)
        {
            errors.Add("Service provider (_provider) has not been initialized. Setup() must be called first.");
        }

        // Validate services
        if (parserServiceField?.GetValue(value) == null)
        {
            errors.Add("Parser service (_parserService) has not been initialized. Setup() must be called first.");
        }

        if (deviceServiceField?.GetValue(value) == null)
        {
            errors.Add("Device service (_deviceService) has not been initialized. Setup() must be called first.");
        }

        if (locationServiceField?.GetValue(value) == null)
        {
            errors.Add("Location service (_locationService) has not been initialized. Setup() must be called first.");
        }

        if (analyticsServiceField?.GetValue(value) == null)
        {
            errors.Add("Analytics service (_analyticsService) has not been initialized. Setup() must be called first.");
        }

        // Validate test frames
        var gt06Frame = gt06FrameField?.GetValue(value) as GpsFrame;
        if (gt06Frame == null)
        {
            errors.Add("GT06 test frame (_gt06Frame) has not been initialized. Setup() must be called first.");
        }
        else
        {
            ValidateGpsFrame(gt06Frame, "GT06 test frame (_gt06Frame)", errors);
        }

        var h02Frame = h02FrameField?.GetValue(value) as GpsFrame;
        if (h02Frame == null)
        {
            errors.Add("H02 test frame (_h02Frame) has not been initialized. Setup() must be called first.");
        }
        else
        {
            ValidateGpsFrame(h02Frame, "H02 test frame (_h02Frame)", errors);
        }

        var tk103Frame = tk103FrameField?.GetValue(value) as GpsFrame;
        if (tk103Frame == null)
        {
            errors.Add("TK103 test frame (_tk103Frame) has not been initialized. Setup() must be called first.");
        }
        else
        {
            ValidateGpsFrame(tk103Frame, "TK103 test frame (_tk103Frame)", errors);
        }

        // Validate frame batch
        var frameBatch = frameBatchField?.GetValue(value) as List<GpsFrame>;
        if (frameBatch == null || frameBatch.Count == 0)
        {
            errors.Add("Frame batch (_frameBatch) has not been initialized or is empty. Setup() must be called first.");
        }
        else if (frameBatch.Count != 100)
        {
            errors.Add($"Frame batch (_frameBatch) contains {frameBatch.Count} frames, expected 100. Setup() may not have completed successfully.");
        }
        else
        {
            // Validate each frame in batch
            for (int i = 0; i < frameBatch.Count; i++)
            {
                ValidateGpsFrame(frameBatch[i], $"Frame batch item [{i}] (_frameBatch[{i}])", errors);
            }
        }

        // Validate test device
        var testDevice = testDeviceField?.GetValue(value) as Device;
        if (testDevice == null)
        {
            errors.Add("Test device (_testDevice) has not been initialized. Setup() must be called first.");
        }
        else
        {
            ValidateDevice(testDevice, "Test device (_testDevice)", errors);
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a GPS frame instance.
    /// </summary>
    private static void ValidateGpsFrame(GpsFrame frame, string fieldName, List<string> errors)
    {
        if (frame == null)
        {
            errors.Add($"{fieldName} is null");
            return;
        }

        if (frame.RawData == null || frame.RawData.Length == 0)
        {
            errors.Add($"{fieldName}.RawData is null or empty");
        }

        if (frame.Protocol == ProtocolType.Unknown)
        {
            errors.Add($"{fieldName}.Protocol is ProtocolType.Unknown");
        }

        if (frame.ReceivedAt == default)
        {
            errors.Add($"{fieldName}.ReceivedAt has default DateTime value");
        }
        else if (frame.ReceivedAt > DateTime.UtcNow.AddMinutes(1))
        {
            errors.Add($"{fieldName}.ReceivedAt is in the future ({(DateTime.UtcNow - frame.ReceivedAt).TotalMinutes} minutes ahead)");
        }
        else if (frame.ReceivedAt < DateTime.UtcNow.AddYears(-1))
        {
            errors.Add($"{fieldName}.ReceivedAt is more than 1 year in the past");
        }
    }

    /// <summary>
    /// Validates a Device instance.
    /// </summary>
    private static void ValidateDevice(Device device, string fieldName, List<string> errors)
    {
        if (device == null)
        {
            errors.Add($"{fieldName} is null");
            return;
        }

        if (string.IsNullOrWhiteSpace(device.Imei))
        {
            errors.Add($"{fieldName}.Imei is null or whitespace");
        }
        else if (device.Imei.Length < 10 || device.Imei.Length > 20)
        {
            errors.Add($"{fieldName}.Imei has invalid length {device.Imei.Length}, expected 10-20 characters");
        }

        if (string.IsNullOrWhiteSpace(device.DeviceName))
        {
            errors.Add($"{fieldName}.DeviceName is null or whitespace");
        }

        if (device.Protocol == ProtocolType.Unknown)
        {
            errors.Add($"{fieldName}.Protocol is ProtocolType.Unknown");
        }

        if (!device.IsActive)
        {
            errors.Add($"{fieldName}.IsActive is false, expected true for benchmark device");
        }
    }

    /// <summary>
    /// Checks if a ProtocolBenchmarks instance is valid.
    /// </summary>
    /// <param name="value">The ProtocolBenchmarks instance to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this ProtocolBenchmarks value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a ProtocolBenchmarks instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The ProtocolBenchmarks instance to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails with detailed error messages</exception>
    public static void EnsureValid(this ProtocolBenchmarks value)
    {
        var errors = value.Validate();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ProtocolBenchmarks validation failed:{Environment.NewLine}- " +
                string.Join($"{Environment.NewLine}- ", errors));
        }
    }
}
