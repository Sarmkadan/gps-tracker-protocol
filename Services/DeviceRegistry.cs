#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Services;

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// In-memory registry of connected GPS devices, keyed by IMEI.
/// Tracks online/offline state, last heartbeat, and connection metadata.
/// </summary>
public class DeviceRegistry
{
    private readonly ConcurrentDictionary<string, Device> _devices = new();
    private readonly ILogger<DeviceRegistry> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="DeviceRegistry"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostics.</param>
    public DeviceRegistry(ILogger<DeviceRegistry> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers a device or updates its record if it already exists.
    /// Sets the device status to <see cref="DeviceStatus.Online"/> and refreshes LastSeen.
    /// </summary>
    /// <param name="imei">The device IMEI (unique hardware identifier).</param>
    /// <param name="protocol">The protocol the device speaks.</param>
    /// <param name="ipAddress">Remote IP address of the device connection.</param>
    /// <returns>The registered or updated <see cref="Device"/>.</returns>
    public Device RegisterOrUpdate(string imei, ProtocolType protocol, string? ipAddress = null)
    {
        var device = _devices.AddOrUpdate(
            imei,
            _ =>
            {
                _logger.LogInformation("New device registered: IMEI={Imei}, Protocol={Protocol}", imei, protocol);
                return new Device
                {
                    Id = Guid.NewGuid().ToString(),
                    Imei = imei,
                    Protocol = protocol,
                    Status = DeviceStatus.Online,
                    LastSeen = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    IsActive = true,
                    RegistrationDate = DateTime.UtcNow,
                    ConnectionCount = 1
                };
            },
            (_, existing) =>
            {
                existing.Status = DeviceStatus.Online;
                existing.LastSeen = DateTime.UtcNow;
                existing.IpAddress = ipAddress;
                existing.ConnectionCount++;
                return existing;
            });

        return device;
    }

    /// <summary>
    /// Looks up a device by its IMEI.
    /// </summary>
    /// <param name="imei">The IMEI to search for.</param>
    /// <returns>The device if found, otherwise null.</returns>
    public Device? GetByImei(string imei)
    {
        _devices.TryGetValue(imei, out var device);
        return device;
    }

    /// <summary>
    /// Returns all currently registered devices.
    /// </summary>
    public IReadOnlyCollection<Device> GetAll() => _devices.Values.ToList().AsReadOnly();

    /// <summary>
    /// Marks devices as offline if they have not been seen within the given timeout.
    /// Intended to be called periodically by a background health-check timer.
    /// </summary>
    /// <param name="timeout">Maximum allowed silence duration before marking offline.</param>
    /// <returns>Number of devices marked offline.</returns>
    public int MarkStaleDevicesOffline(TimeSpan timeout)
    {
        int count = 0;
        var cutoff = DateTime.UtcNow - timeout;

        foreach (var kvp in _devices)
        {
            if (kvp.Value.Status == DeviceStatus.Online && kvp.Value.LastSeen < cutoff)
            {
                kvp.Value.Status = DeviceStatus.Offline;
                count++;
                _logger.LogInformation("Device {Imei} marked offline (last seen {LastSeen})",
                    kvp.Key, kvp.Value.LastSeen);
            }
        }

        return count;
    }

    /// <summary>
    /// Removes a device from the registry entirely.
    /// </summary>
    /// <param name="imei">The IMEI of the device to remove.</param>
    /// <returns>True if the device was found and removed.</returns>
    public bool Unregister(string imei)
    {
        bool removed = _devices.TryRemove(imei, out _);
        if (removed)
            _logger.LogInformation("Device unregistered: IMEI={Imei}", imei);
        return removed;
    }
}
