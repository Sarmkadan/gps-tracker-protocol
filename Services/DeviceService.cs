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
        if (device == null)
            throw new ArgumentNullException(nameof(device));

        if (!device.IsValid())
            throw new ValidationException("Device validation failed", nameof(device));

        var existing = await _repository.GetByImeiAsync(device.Imei);
        if (existing != null)
            throw new DeviceException($"Device with IMEI {device.Imei} already exists", device.Id);

        device.Id ??= Guid.NewGuid().ToString();
        device.LastSeen = DateTime.UtcNow;
        device.Status = DeviceStatus.Offline;

        return await _repository.CreateAsync(device);
    }

    /// <summary>
    /// Gets a device by its ID.
    /// </summary>
    public async Task<Device?> GetDeviceAsync(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        return await _repository.GetByIdAsync(deviceId);
    }

    /// <summary>
    /// Gets a device by its IMEI.
    /// </summary>
    public async Task<Device?> GetDeviceByImeiAsync(string imei)
    {
        if (string.IsNullOrWhiteSpace(imei))
            throw new ArgumentException("IMEI cannot be empty", nameof(imei));

        return await _repository.GetByImeiAsync(imei);
    }

    /// <summary>
    /// Gets all registered devices.
    /// </summary>
    public async Task<IEnumerable<Device>> GetAllDevicesAsync()
    {
        return await _repository.GetAllAsync();
    }

    /// <summary>
    /// Gets all currently online devices.
    /// </summary>
    public async Task<IEnumerable<Device>> GetOnlineDevicesAsync()
    {
        return await _repository.GetByStatusAsync(DeviceStatus.Online);
    }

    /// <summary>
    /// Updates device information.
    /// </summary>
    public async Task<bool> UpdateDeviceAsync(Device device)
    {
        if (device == null)
            throw new ArgumentNullException(nameof(device));

        var existing = await _repository.GetByIdAsync(device.Id);
        if (existing == null)
            throw new DeviceException($"Device {device.Id} not found", device.Id);

        if (!device.IsValid())
            throw new ValidationException("Device validation failed", nameof(device));

        await _repository.UpdateAsync(device);
        return true;
    }

    /// <summary>
    /// Deregisters a device from the system.
    /// </summary>
    public async Task<bool> DeregisterDeviceAsync(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        var device = await _repository.GetByIdAsync(deviceId);
        if (device == null)
            throw new DeviceException($"Device {deviceId} not found", deviceId);

        device.IsActive = false;
        await _repository.UpdateAsync(device);
        return true;
    }

    /// <summary>
    /// Updates device heartbeat and status.
    /// </summary>
    public async Task UpdateDeviceHeartbeatAsync(string deviceId, string? ipAddress = null, int port = 0)
    {
        var device = await _repository.GetByIdAsync(deviceId);
        if (device == null)
            throw new DeviceException($"Device {deviceId} not found", deviceId);

        device.UpdateHeartbeat(ipAddress, port);
        await _repository.UpdateAsync(device);
    }

    /// <summary>
    /// Gets devices that are offline based on heartbeat timeout.
    /// </summary>
    public async Task<IEnumerable<Device>> GetOfflineDevicesAsync(TimeSpan timeout)
    {
        return await _repository.GetOfflineDevicesAsync(timeout);
    }
}
