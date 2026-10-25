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
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The registered device.</returns>
    Task<Device> RegisterDeviceAsync(Device device, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a device by its ID.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The device if found, otherwise null.</returns>
    Task<Device?> GetDeviceAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a device by its IMEI.
    /// </summary>
    /// <param name="imei">The device IMEI.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The device if found, otherwise null.</returns>
    Task<Device?> GetDeviceByImeiAsync(string imei, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all registered devices.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of devices.</returns>
    Task<IEnumerable<Device>> GetAllDevicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all currently online devices.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of online devices.</returns>
    Task<IEnumerable<Device>> GetOnlineDevicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates device information.
    /// </summary>
    /// <param name="device">The device to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the update was successful, otherwise false.</returns>
    Task<bool> UpdateDeviceAsync(Device device, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deregisters a device from the system.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the deregistration was successful, otherwise false.</returns>
    Task<bool> DeregisterDeviceAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates device heartbeat and status.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="ipAddress">The IP address of the device.</param>
    /// <param name="port">The port of the device.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task UpdateDeviceHeartbeatAsync(string deviceId, string? ipAddress = null, int port = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets devices that are offline based on heartbeat timeout.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of offline devices.</returns>
    Task<IEnumerable<Device>> GetOfflineDevicesAsync(TimeSpan timeout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the connection status snapshot for a single device.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The device status DTO if found, otherwise null.</returns>
    Task<DeviceStatusDto?> GetDeviceStatusAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the connection status snapshot for every registered device.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of device status DTOs.</returns>
    Task<IEnumerable<DeviceStatusDto>> GetAllDeviceStatusesAsync(CancellationToken cancellationToken = default);
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
    public async Task<Device> RegisterDeviceAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        if (_legacyRepository is null)
            throw new InvalidOperationException("Service was not constructed with a generic repository.");

        var existing = await _legacyRepository.GetByIdAsync(deviceId, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
            return existing;

        var device = new Device
        {
            Id = deviceId,
            IsActive = true,
            RegistrationDate = DateTime.UtcNow
        };

        await _legacyRepository.AddAsync(device, cancellationToken).ConfigureAwait(false);
        return device;
    }

    /// <summary>
    /// Gets a device by its ID using the generic repository.
    /// </summary>
    public async Task<Device?> GetDeviceByIdAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        if (_legacyRepository is null)
            throw new InvalidOperationException("Service was not constructed with a generic repository.");

        return await _legacyRepository.GetByIdAsync(deviceId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates the active status of a device using the generic repository.
    /// </summary>
    public async Task UpdateDeviceStatusAsync(string deviceId, bool isActive, CancellationToken cancellationToken = default)
    {
        if (_legacyRepository is null)
            throw new InvalidOperationException("Service was not constructed with a generic repository.");

        var device = await _legacyRepository.GetByIdAsync(deviceId, cancellationToken).ConfigureAwait(false);
        if (device is null)
            return;

        device.IsActive = isActive;
        await _legacyRepository.UpdateAsync(device, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Registers a new device in the system.
    /// </summary>
    public async Task<Device> RegisterDeviceAsync(Device device, CancellationToken cancellationToken = default)
    {
        if (device is null)
            throw new ArgumentNullException(nameof(device));

        if (!device.IsValid())
            throw new ValidationException("Device validation failed", nameof(device));

        var existing = await _repository.GetByImeiAsync(device.Imei, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
            throw new DeviceException($"Device with IMEI {device.Imei} already exists", device.Id);

        device.Id ??= Guid.NewGuid().ToString();
        device.LastSeen = DateTime.UtcNow;
        device.Status = DeviceStatus.Offline;

        return await _repository.CreateAsync(device, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a device by its ID.
    /// </summary>
    public async Task<Device?> GetDeviceAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        return await _repository.GetByIdAsync(deviceId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a device by its IMEI.
    /// </summary>
    public async Task<Device?> GetDeviceByImeiAsync(string imei, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imei))
            throw new ArgumentException("IMEI cannot be empty", nameof(imei));

        return await _repository.GetByImeiAsync(imei, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all registered devices.
    /// </summary>
    public async Task<IEnumerable<Device>> GetAllDevicesAsync(CancellationToken cancellationToken = default)
    {
        if (_legacyRepository is not null)
            return await _legacyRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        return await _repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all currently online devices.
    /// </summary>
    public async Task<IEnumerable<Device>> GetOnlineDevicesAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetByStatusAsync(DeviceStatus.Online, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates device information.
    /// </summary>
    public async Task<bool> UpdateDeviceAsync(Device device, CancellationToken cancellationToken = default)
    {
        if (device is null)
            throw new ArgumentNullException(nameof(device));

        var existing = await _repository.GetByIdAsync(device.Id, cancellationToken).ConfigureAwait(false);
        if (existing is null)
            throw new DeviceException($"Device {device.Id} not found", device.Id);

        if (!device.IsValid())
            throw new ValidationException("Device validation failed", nameof(device));

        await _repository.UpdateAsync(device, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Deregisters a device from the system.
    /// </summary>
    public async Task<bool> DeregisterDeviceAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        var device = await _repository.GetByIdAsync(deviceId, cancellationToken).ConfigureAwait(false);
        if (device is null)
            throw new DeviceException($"Device {deviceId} not found", deviceId);

        device.IsActive = false;
        await _repository.UpdateAsync(device, cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Updates device heartbeat and status.
    /// </summary>
    public async Task UpdateDeviceHeartbeatAsync(string deviceId, string? ipAddress = null, int port = 0, CancellationToken cancellationToken = default)
    {
        var device = await _repository.GetByIdAsync(deviceId, cancellationToken).ConfigureAwait(false);
        if (device is null)
            throw new DeviceException($"Device {deviceId} not found", deviceId);

        device.UpdateHeartbeat(ipAddress, port);
        await _repository.UpdateAsync(device, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets devices that are offline based on heartbeat timeout.
    /// </summary>
    public async Task<IEnumerable<Device>> GetOfflineDevicesAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        return await _repository.GetOfflineDevicesAsync(timeout, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the connection status snapshot for a single device.
    /// Returns null when the device is not found.
    /// </summary>
    public async Task<DeviceStatusDto?> GetDeviceStatusAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        var device = await _repository.GetByIdAsync(deviceId, cancellationToken).ConfigureAwait(false);
        return device is null ? null : ToStatusDto(device);
    }

    /// <summary>
    /// Gets the connection status snapshot for every registered device.
    /// Operators can use this to monitor fleet health at a glance.
    /// </summary>
    public async Task<IEnumerable<DeviceStatusDto>> GetAllDeviceStatusesAsync(CancellationToken cancellationToken = default)
    {
        var devices = await _repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
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
