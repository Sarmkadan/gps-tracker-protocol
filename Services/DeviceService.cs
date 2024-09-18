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
/// <summary>
    /// Registers a new device in the system.
    /// </summary>
    /// <param name="device">The device to register.</param>
    /// <returns>The registered device.</returns>
    Task<Device> RegisterDeviceAsync(Device device);

    /// <summary>
    /// Gets a device by its ID.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <returns>The device if found, otherwise null.</returns>
    Task<Device?> GetDeviceAsync(string deviceId);

    /// <summary>
    /// Gets a device by its IMEI.
    /// </summary>
    /// <param name="imei">The device IMEI.</param>
    /// <returns>The device if found, otherwise null.</returns>
    Task<Device?> GetDeviceByImeiAsync(string imei);

    /// <summary>
    /// Gets all registered devices.
    /// </summary>
    /// <returns>A collection of devices.</returns>
    Task<IEnumerable<Device>> GetAllDevicesAsync();

    /// <summary>
    /// Gets all currently online devices.
    /// </summary>
    /// <returns>A collection of online devices.</returns>
    Task<IEnumerable<Device>> GetOnlineDevicesAsync();

    /// <summary>
    /// Updates device information.
    /// </summary>
    /// <param name="device">The device to update.</param>
    /// <returns>True if the update was successful, otherwise false.</returns>
    Task<bool> UpdateDeviceAsync(Device device);

    /// <summary>
    /// Deregisters a device from the system.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <returns>True if the deregistration was successful, otherwise false.</returns>
    Task<bool> DeregisterDeviceAsync(string deviceId);

    /// <summary>
    /// Updates device heartbeat and status.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="ipAddress">The IP address of the device.</param>
    /// <param name="port">The port of the device.</param>
    Task UpdateDeviceHeartbeatAsync(string deviceId, string? ipAddress = null, int port = 0);

    /// <summary>
    /// Gets devices that are offline based on heartbeat timeout.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns>A collection of offline devices.</returns>
    Task<IEnumerable<Device>> GetOfflineDevicesAsync(TimeSpan timeout);

    /// <summary>
    /// Gets the connection status snapshot for a single device.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <returns>The device status DTO if found, otherwise null.</returns>
    Task<DeviceStatusDto?> GetDeviceStatusAsync(string deviceId);

    /// <summary>
    /// Gets the connection status snapshot for every registered device.
    /// </summary>
    /// <returns>A collection of device status DTOs.</returns>
    Task<IEnumerable<DeviceStatusDto>> GetAllDeviceStatusesAsync();
}

/// <summary>
/// Implementation of device service.
/// </summary>
public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _repository = null!;
    private readonly ILocationDataRepository _locationRepository = null!;
    private readonly IRepository<Device>? _legacyRepository;

    public DeviceService(IUnitOfWork unitOfWork)
    {
        _repository = unitOfWork.Devices;
        _locationRepository = unitOfWork.LocationData;
    }

    /// <summary>
    /// Constructs the service directly from a generic device repository.
    /// </summary>
    public DeviceService(IRepository<Device> repository)
    {
        _legacyRepository = repository;
    }

    /// <summary>
    /// Registers a device by ID using the generic repository, returning the existing
    /// device if one is already registered under that ID.
    /// </summary>
    public async Task<Device> RegisterDeviceAsync(string deviceId)
    {
        if (_legacyRepository is null)
            throw new InvalidOperationException("Service was not constructed with a generic repository.");

        var existing = await _legacyRepository.GetByIdAsync(deviceId).ConfigureAwait(false);
        if (existing is not null)
            return existing;

        var device = new Device
        {
            Id = deviceId,
            IsActive = true,
            RegistrationDate = DateTime.UtcNow
        };

        await _legacyRepository.AddAsync(device).ConfigureAwait(false);
        return device;
    }

    /// <summary>
    /// Gets a device by its ID using the generic repository.
    /// </summary>
    public async Task<Device?> GetDeviceByIdAsync(string deviceId)
    {
        if (_legacyRepository is null)
            throw new InvalidOperationException("Service was not constructed with a generic repository.");

        return await _legacyRepository.GetByIdAsync(deviceId).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates the active status of a device using the generic repository.
    /// </summary>
    public async Task UpdateDeviceStatusAsync(string deviceId, bool isActive)
    {
        if (_legacyRepository is null)
            throw new InvalidOperationException("Service was not constructed with a generic repository.");

        var device = await _legacyRepository.GetByIdAsync(deviceId).ConfigureAwait(false);
        if (device is null)
            return;

        device.IsActive = isActive;
        await _legacyRepository.UpdateAsync(device).ConfigureAwait(false);
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
        if (_legacyRepository is not null)
            return await _legacyRepository.GetAllAsync().ConfigureAwait(false);

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
