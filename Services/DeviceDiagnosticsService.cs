#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Services;

using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Aggregates health and status metrics for individual devices and the whole fleet.
/// Designed for use by an operator dashboard, alerting system, or REST diagnostics endpoint.
/// </summary>
public interface IDeviceDiagnosticsService
{
    /// <summary>
    /// Builds a complete <see cref="DeviceDiagnosticsReport"/> for the specified device.
    /// Returns <c>null</c> when the device does not exist.
    /// </summary>
    Task<DeviceDiagnosticsReport?> GetDiagnosticsAsync(string deviceId);

    /// <summary>
    /// Builds a <see cref="FleetHealthReport"/> that aggregates diagnostics across every
    /// registered device.
    /// </summary>
    Task<FleetHealthReport> GetFleetHealthReportAsync();

    /// <summary>
    /// Runs a lightweight self-test for the specified device and returns the result.
    /// The result is also embedded in the next call to <see cref="GetDiagnosticsAsync"/>.
    /// Returns <c>null</c> when the device does not exist.
    /// </summary>
    Task<DeviceSelfTestResult?> RunSelfTestAsync(string deviceId);
}

/// <summary>
/// Implementation of <see cref="IDeviceDiagnosticsService"/>.
/// </summary>
public class DeviceDiagnosticsService : IDeviceDiagnosticsService
{
    private const int LowBatteryThreshold  = 20;  // percent
    private const int WeakSignalThreshold  = -90; // dBm
    private const int OfflineThresholdMins = 10;

    private readonly IDeviceRepository _deviceRepository;
    private readonly ILocationDataRepository _locationRepository;
    private readonly IJourneyRepository _journeyRepository;
    private readonly ILogger<DeviceDiagnosticsService> _logger;

    // Stores the most recent self-test per device so it surfaces in the next GetDiagnosticsAsync call.
    private readonly Dictionary<string, DeviceSelfTestResult> _selfTestCache = new();
    private readonly object _cacheLock = new();

    /// <summary>Initialises the service with required dependencies.</summary>
    public DeviceDiagnosticsService(
        IUnitOfWork unitOfWork,
        ILogger<DeviceDiagnosticsService> logger)
    {
        _deviceRepository   = unitOfWork.Devices;
        _locationRepository = unitOfWork.LocationData;
        _journeyRepository  = unitOfWork.Journeys;
        _logger             = logger;
    }

    /// <inheritdoc/>
    public async Task<DeviceDiagnosticsReport?> GetDiagnosticsAsync(string deviceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);

        var device = await _deviceRepository.GetByIdAsync(deviceId).ConfigureAwait(false);
        if (device is null)
            return null;

        var locationTask  = _locationRepository.GetByDeviceIdAsync(deviceId);
        var journeyTask   = _journeyRepository.GetByDeviceIdAsync(deviceId);
        var latestLocTask = _locationRepository.GetLatestByDeviceIdAsync(deviceId);

        await Task.WhenAll(locationTask, journeyTask, latestLocTask).ConfigureAwait(false);

        var locations = await locationTask;
        var journeys  = await journeyTask;
        var latest    = await latestLocTask;

        var locationList = locations.ToList();
        var journeyList  = journeys.ToList();

        var totalDistance = journeyList
            .Where(j => j.Status == 1)
            .Sum(j => j.GetTotalDistance());

        DeviceSelfTestResult? selfTest;
        lock (_cacheLock)
            _selfTestCache.TryGetValue(deviceId, out selfTest);

        var report = new DeviceDiagnosticsReport
        {
            DeviceId              = device.Id,
            DeviceName            = device.DeviceName,
            Imei                  = device.Imei,
            Protocol              = device.Protocol,
            Status                = device.Status,
            IsOnline              = device.Status == DeviceStatus.Online,
            LastSeen              = device.LastSeen,
            TimeSinceLastContact  = DateTime.UtcNow - device.LastSeen,
            TotalPacketsReceived  = device.ConnectionCount,
            IpAddress             = device.IpAddress,
            BatteryLevel          = device.BatteryLevel,
            SignalStrength        = device.SignalStrength,
            SignalQuality         = ClassifySignal(device.SignalStrength),
            TotalLocationPoints   = locationList.Count,
            LastLocation          = latest,
            TotalDistanceKm       = totalDistance,
            TotalJourneys         = journeyList.Count,
            ActiveJourneys        = journeyList.Count(j => j.Status == 0),
            SelfTest              = selfTest
        };

        _logger.LogDebug("Diagnostics report generated for device {DeviceId}", deviceId);
        return report;
    }

    /// <inheritdoc/>
    public async Task<FleetHealthReport> GetFleetHealthReportAsync()
    {
        var devices = (await _deviceRepository.GetAllAsync().ConfigureAwait(false)).ToList();

        var reportTasks = devices.Select(d => GetDiagnosticsAsync(d.Id));
        var reports = (await Task.WhenAll(reportTasks).ConfigureAwait(false))
            .OfType<DeviceDiagnosticsReport>()
            .ToList();

        var offlineTimeout = TimeSpan.FromMinutes(OfflineThresholdMins);

        return new FleetHealthReport
        {
            TotalDevices       = devices.Count,
            OnlineDevices      = reports.Count(r => r.IsOnline),
            OfflineDevices     = reports.Count(r => !r.IsOnline),
            LowBatteryDevices  = reports.Count(r => r.BatteryLevel is >= 0 and < LowBatteryThreshold),
            WeakSignalDevices  = reports.Count(r => r.SignalStrength != 0 && r.SignalStrength < WeakSignalThreshold),
            DeviceReports      = reports
        };
    }

    /// <inheritdoc/>
    public async Task<DeviceSelfTestResult?> RunSelfTestAsync(string deviceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);

        var device = await _deviceRepository.GetByIdAsync(deviceId).ConfigureAwait(false);
        if (device is null)
            return null;

        var warnings = new List<string>();

        var connectivityOk = device.Status == DeviceStatus.Online
                          || DateTime.UtcNow - device.LastSeen < TimeSpan.FromMinutes(OfflineThresholdMins);

        var batteryOk = device.BatteryLevel < 0 || device.BatteryLevel >= LowBatteryThreshold;
        var signalOk  = device.SignalStrength == 0 || device.SignalStrength >= WeakSignalThreshold;

        var hasLocation = await _locationRepository.GetLatestByDeviceIdAsync(deviceId).ConfigureAwait(false) is not null;

        if (!connectivityOk)
            warnings.Add($"Device has not reported in over {OfflineThresholdMins} minutes.");
        if (!batteryOk)
            warnings.Add($"Battery low: {device.BatteryLevel}% (threshold {LowBatteryThreshold}%).");
        if (!signalOk)
            warnings.Add($"Weak signal: {device.SignalStrength} dBm (threshold {WeakSignalThreshold} dBm).");
        if (!hasLocation)
            warnings.Add("No location data recorded for this device.");

        var result = new DeviceSelfTestResult
        {
            ConnectivityOk = connectivityOk,
            BatteryOk      = batteryOk,
            SignalOk        = signalOk,
            LocationDataOk = hasLocation,
            Warnings       = warnings
        };

        lock (_cacheLock)
            _selfTestCache[deviceId] = result;

        _logger.LogInformation(
            "Self-test for device {DeviceId}: {Status}",
            deviceId, result.AllOk ? "PASS" : "WARN");

        return result;
    }

    // ── helpers ────────────────────────────────────────────────────────────────

    private static string ClassifySignal(int dBm) => dBm switch
    {
        0                         => "Unknown",
        >= -70                    => "Excellent",
        >= -80                    => "Good",
        >= -90                    => "Fair",
        _                         => "Poor"
    };
}
