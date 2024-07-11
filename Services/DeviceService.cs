#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Services;

using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

/// <summary>
/// Service for managing GPS tracking devices.
/// </summary>
public interface IDeviceService
{
    Task<Device> RegisterDeviceAsync(Device device);
    Task<Device?> GetDeviceAsync(string deviceId);
    Task<Device?> GetDeviceByImeiAsync(string imei);
    Task<IEnumerable<Device>> GetAllDevicesAsync();
    Task<IEnumerable<Device>> GetOnlineDevicesAsync();
    Task<bool> UpdateDeviceAsync(Device device);
    Task<bool> DeregisterDeviceAsync(string deviceId);
    Task UpdateDeviceHeartbeatAsync(string deviceId, string? ipAddress = null, int port = 0);
    Task<IEnumerable<Device>> GetOfflineDevicesAsync(TimeSpan timeout);
    Task<DeviceStatusDto?> GetDeviceStatusAsync(string deviceId);
    Task<IEnumerable<DeviceStatusDto>> GetAllDeviceStatusesAsync();
}

/// <summary>
/// Implementation of device service.
/// </summary>
public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _repository;
    private readonly ILocationDataRepository _locationRepository;

    public DeviceService(IUnitOfWork unitOfWork)
    {
        _repository = unitOfWork.Devices;
        _locationRepository = unitOfWork.LocationData;
    }

    /// <summary>
    /// Registers a new device in the system.
    /// </summary>
    public async Task<Device> RegisterDeviceAsync(Device device)
    {
        if (device is null)
            throw new ArgumentNullException(nameof(device));

        if (!device.IsValid())
            throw new ValidationException("Device validation failed", nameof(device));

        var existing = await _repository.GetByImeiAsync(device.Imei).ConfigureAwait(false);
        if (existing is not null)
            throw new DeviceException($"Device with IMEI {device.Imei} already exists", device.Id);

        device.Id ??= Guid.NewGuid().ToString();
        device.LastSeen = DateTime.UtcNow;
        device.Status = DeviceStatus.Offline;

        return await _repository.CreateAsync(device).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a device by its ID.
    /// </summary>
    public async Task<Device?> GetDeviceAsync(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        return await _repository.GetByIdAsync(deviceId).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a device by its IMEI.
    /// </summary>
    public async Task<Device?> GetDeviceByImeiAsync(string imei)
    {
        if (string.IsNullOrWhiteSpace(imei))
            throw new ArgumentException("IMEI cannot be empty", nameof(imei));

        return await _repository.GetByImeiAsync(imei).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all registered devices.
    /// </summary>
    public async Task<IEnumerable<Device>> GetAllDevicesAsync()
    {
        return await _repository.GetAllAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all currently online devices.
    /// </summary>
    public async Task<IEnumerable<Device>> GetOnlineDevicesAsync()
    {
        return await _repository.GetByStatusAsync(DeviceStatus.Online).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates device information.
    /// </summary>
    public async Task<bool> UpdateDeviceAsync(Device device)
    {
        if (device is null)
            throw new ArgumentNullException(nameof(device));

        var existing = await _repository.GetByIdAsync(device.Id).ConfigureAwait(false);
        if (existing is null)
            throw new DeviceException($"Device {device.Id} not found", device.Id);

        if (!device.IsValid())
            throw new ValidationException("Device validation failed", nameof(device));

        await _repository.UpdateAsync(device).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Deregisters a device from the system.
    /// </summary>
    public async Task<bool> DeregisterDeviceAsync(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        var device = await _repository.GetByIdAsync(deviceId).ConfigureAwait(false);
        if (device is null)
            throw new DeviceException($"Device {deviceId} not found", deviceId);

        device.IsActive = false;
        await _repository.UpdateAsync(device).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Updates device heartbeat and status.
    /// </summary>
    public async Task UpdateDeviceHeartbeatAsync(string deviceId, string? ipAddress = null, int port = 0)
    {
        var device = await _repository.GetByIdAsync(deviceId).ConfigureAwait(false);
        if (device is null)
            throw new DeviceException($"Device {deviceId} not found", deviceId);

        device.UpdateHeartbeat(ipAddress, port);
        await _repository.UpdateAsync(device).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets devices that are offline based on heartbeat timeout.
    /// </summary>
    public async Task<IEnumerable<Device>> GetOfflineDevicesAsync(TimeSpan timeout)
    {
        return await _repository.GetOfflineDevicesAsync(timeout).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the connection status snapshot for a single device.
    /// Returns null when the device is not found.
    /// </summary>
    public async Task<DeviceStatusDto?> GetDeviceStatusAsync(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        var device = await _repository.GetByIdAsync(deviceId).ConfigureAwait(false);
        return device is null ? null : ToStatusDto(device);
    }

    /// <summary>
    /// Gets the connection status snapshot for every registered device.
    /// Operators can use this to monitor fleet health at a glance.
    /// </summary>
    public async Task<IEnumerable<DeviceStatusDto>> GetAllDeviceStatusesAsync()
    {
        var devices = await _repository.GetAllAsync().ConfigureAwait(false);
        return devices.Select(ToStatusDto).ToList();
    }

    private static DeviceStatusDto ToStatusDto(Device device) => new()
    {
        DeviceId = device.Id,
        Imei = device.Imei,
        DeviceName = device.DeviceName,
        IsConnected = device.Status == DeviceStatus.Online,
        LastSeen = device.LastSeen,
        ConnectionCount = device.ConnectionCount,
        Status = device.Status
    };
}
