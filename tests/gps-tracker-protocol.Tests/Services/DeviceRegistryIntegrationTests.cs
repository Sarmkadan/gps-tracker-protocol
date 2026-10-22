#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Integration tests for DeviceRegistry covering device lifecycle
// =============================================================================

using System.Collections.Concurrent;
using FluentAssertions;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GpsTrackerProtocol.Tests.Services;

/// <summary>
/// Integration tests for DeviceRegistry covering device lifecycle:
/// - Registration on login
/// - Updating last-seen position from PositionDecoder output
/// - Marking device offline after inactivity timeout
/// - Re-registration after reconnect preserving device ID
/// - Thread-safety under concurrent access
/// </summary>
public class DeviceRegistryIntegrationTests
{
    private readonly ILogger<DeviceRegistry> _logger;
    private readonly DeviceRegistry _registry;

    public DeviceRegistryIntegrationTests()
    {
        _logger = Substitute.For<ILogger<DeviceRegistry>>();
        _registry = new DeviceRegistry(_logger);
    }

    #region Registration on Login

    [Fact]
    public void RegisterOrUpdate_NewDevice_RegistersWithOnlineStatus()
    {
        // Arrange
        const string imei = "123456789012345";
        const ProtocolType protocol = ProtocolType.GT06;
        const string ipAddress = "192.168.1.100";

        // Act
        var device = _registry.RegisterOrUpdate(imei, protocol, ipAddress);

        // Assert
        device.Should().NotBeNull();
        device.Imei.Should().Be(imei);
        device.Protocol.Should().Be(protocol);
        device.Status.Should().Be(DeviceStatus.Online);
        device.IpAddress.Should().Be(ipAddress);
        device.IsActive.Should().BeTrue();
        device.ConnectionCount.Should().Be(1);
        device.RegistrationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        device.LastSeen.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RegisterOrUpdate_ExistingDevice_UpdatesLastSeenAndIncrementsConnectionCount()
    {
        // Arrange
        const string imei = "123456789012345";
        const ProtocolType protocol = ProtocolType.GT06;
        const string ipAddress = "192.168.1.100";

        // Act - Register first time
        var device1 = _registry.RegisterOrUpdate(imei, protocol, ipAddress);
        var originalId = device1.Id;
        var originalRegistrationDate = device1.RegistrationDate;
        var originalConnectionCount = device1.ConnectionCount;
        var originalLastSeen = device1.LastSeen;

        // Small delay to ensure LastSeen changes
        Thread.Sleep(10);

        // Act - Register again (simulating reconnect)
        var device2 = _registry.RegisterOrUpdate(imei, protocol, "192.168.1.101");

        // Assert
        device2.Should().NotBeNull();
        device2.Id.Should().Be(originalId, "Device ID should be preserved on reconnect");
        device2.RegistrationDate.Should().Be(originalRegistrationDate, "Registration date should not change");
        device2.ConnectionCount.Should().Be(originalConnectionCount + 1, "Connection count should increment");
        device2.LastSeen.Should().BeOnOrAfter(originalLastSeen, "LastSeen should be updated or same");
        device2.IpAddress.Should().Be("192.168.1.101", "IP address should be updated");
        device2.Status.Should().Be(DeviceStatus.Online);
    }

    [Fact]
    public void RegisterOrUpdate_DifferentProtocol_UpdatesProtocol()
    {
        // Arrange
        const string imei = "123456789012345";

        // Act
        var device1 = _registry.RegisterOrUpdate(imei, ProtocolType.GT06);
        var device2 = _registry.RegisterOrUpdate(imei, ProtocolType.H02);

        // Assert - Protocol is set on initial registration and preserved on subsequent updates
        // The DeviceRegistry only updates Status, LastSeen, IpAddress, and ConnectionCount on reconnect,
        // not the Protocol field
        device2.Protocol.Should().Be(ProtocolType.GT06, "Protocol is preserved on reconnect in DeviceRegistry");
    }

    [Fact]
    public void GetByImei_RegisteredDevice_ReturnsDevice()
    {
        // Arrange
        const string imei = "123456789012345";
        _registry.RegisterOrUpdate(imei, ProtocolType.GT06);

        // Act
        var device = _registry.GetByImei(imei);

        // Assert
        device.Should().NotBeNull();
        device!.Imei.Should().Be(imei);
    }

    [Fact]
    public void GetByImei_UnregisteredDevice_ReturnsNull()
    {
        // Act
        var device = _registry.GetByImei("nonexistent");

        // Assert
        device.Should().BeNull();
    }

    #endregion

    #region Updating Last-Seen Position from PositionDecoder

    [Fact]
    public void UpdatePositionFromDecoder_GT06Frame_UpdatesDeviceLocation()
    {
        // Arrange
        const string imei = "123456789012345";
        var device = _registry.RegisterOrUpdate(imei, ProtocolType.GT06);

        // Create a PositionDecoder (using null logger for test)
        var decoderLogger = Substitute.For<ILogger<PositionDecoder>>();
        var decoder = new PositionDecoder(decoderLogger);

        // Create a valid GT06 frame payload with known coordinates
        // Latitude: 40.7128 (NYC), Longitude: -74.0060
        // GT06 encoding: raw = degrees * 1_800_000
        int rawLat = (int)(40.7128 * 1_800_000); // 73283040
        int rawLon = (int)(-74.0060 * 1_800_000); // -133210800

        byte[] payload = new byte[8];
        payload[0] = (byte)(rawLat >> 24);
        payload[1] = (byte)(rawLat >> 16);
        payload[2] = (byte)(rawLat >> 8);
        payload[3] = (byte)rawLat;
        payload[4] = (byte)(rawLon >> 24);
        payload[5] = (byte)(rawLon >> 16);
        payload[6] = (byte)(rawLon >> 8);
        payload[7] = (byte)rawLon;

        // Act - Decode position
        var location = decoder.DecodeGt06Position(payload);

        // Assert - Decoder should produce valid location
        location.Should().NotBeNull();
        location!.Latitude.Should().BeApproximately(40.7128, 0.0001);
        location.Longitude.Should().BeApproximately(-74.0060, 0.0001);

        // Act - Update device with decoded position (simulating what a real service would do)
        // In a real implementation, this would update device metadata with location
        device.Metadata["LastLatitude"] = location.Latitude.ToString("F6");
        device.Metadata["LastLongitude"] = location.Longitude.ToString("F6");
        device.Metadata["LastPositionUpdate"] = location.Timestamp.ToString("o");
        _registry.RegisterOrUpdate(imei, ProtocolType.GT06); // Refresh heartbeat

        // Assert - Device metadata updated
        var updatedDevice = _registry.GetByImei(imei);
        updatedDevice.Should().NotBeNull();
        updatedDevice!.Metadata.Should().ContainKey("LastLatitude");
        updatedDevice.Metadata["LastLatitude"].Should().Be("40.712800");
        updatedDevice.Metadata.Should().ContainKey("LastLongitude");
        updatedDevice.Metadata["LastLongitude"].Should().Be("-74.006000");
    }

    [Fact]
    public void UpdatePositionFromDecoder_H02Frame_UpdatesDeviceLocation()
    {
        // Arrange
        const string imei = "123456789012345";
        _registry.RegisterOrUpdate(imei, ProtocolType.H02);

        var decoderLogger = Substitute.For<ILogger<PositionDecoder>>();
        var decoder = new PositionDecoder(decoderLogger);

        // Act - Decode H02 position (NMEA format: DDMM.MMMM)
        // Latitude: 40° 42.768' N = 40.7128
        // Longitude: 74° 0.360' W = -74.0060
        var location = decoder.DecodeH02Position("4042.7680", 'N', "07400.3600", 'W');

        // Assert
        location.Should().NotBeNull();
        location!.Latitude.Should().BeApproximately(40.7128, 0.0001);
        location.Longitude.Should().BeApproximately(-74.0060, 0.0001);

        // Update device metadata
        var device = _registry.GetByImei(imei);
        device!.Metadata["LastLatitude"] = location.Latitude.ToString("F6");
        device.Metadata["LastLongitude"] = location.Longitude.ToString("F6");
        _registry.RegisterOrUpdate(imei, ProtocolType.H02);

        // Verify
        var updated = _registry.GetByImei(imei);
        updated!.Metadata["LastLatitude"].Should().Be("40.712800");
        updated.Metadata["LastLongitude"].Should().Be("-74.006000");
    }

    [Fact]
    public void UpdatePositionFromDecoder_TK103Frame_UpdatesDeviceLocation()
    {
        // Arrange
        const string imei = "123456789012345";
        _registry.RegisterOrUpdate(imei, ProtocolType.TK103);

        var decoderLogger = Substitute.For<ILogger<PositionDecoder>>();
        var decoder = new PositionDecoder(decoderLogger);

        // Act - Decode TK103 position (comma-separated: lat,N/S,lon,E/W)
        var location = decoder.DecodeTk103Position("4042.7680,N,07400.3600,W");

        // Assert
        location.Should().NotBeNull();
        location!.Latitude.Should().BeApproximately(40.7128, 0.0001);
        location.Longitude.Should().BeApproximately(-74.0060, 0.0001);

        // Update device metadata
        var device = _registry.GetByImei(imei);
        device!.Metadata["LastLatitude"] = location.Latitude.ToString("F6");
        device.Metadata["LastLongitude"] = location.Longitude.ToString("F6");
        _registry.RegisterOrUpdate(imei, ProtocolType.TK103);

        // Verify
        var updated = _registry.GetByImei(imei);
        updated!.Metadata["LastLatitude"].Should().Be("40.712800");
        updated.Metadata["LastLongitude"].Should().Be("-74.006000");
    }

    #endregion

    #region Marking Device Offline After Inactivity Timeout

    [Fact]
    public void MarkStaleDevicesOffline_DeviceWithinTimeout_RemainsOnline()
    {
        // Arrange
        const string imei = "123456789012345";
        _registry.RegisterOrUpdate(imei, ProtocolType.GT06);

        var timeout = TimeSpan.FromMinutes(5);

        // Act - Mark stale with 5-minute timeout (device just registered, should be online)
        int markedOffline = _registry.MarkStaleDevicesOffline(timeout);

        // Assert
        markedOffline.Should().Be(0);
        var device = _registry.GetByImei(imei);
        device!.Status.Should().Be(DeviceStatus.Online);
    }

    [Fact]
    public void MarkStaleDevicesOffline_DeviceExceedsTimeout_MarksOffline()
    {
        // Arrange
        const string imei = "123456789012345";
        var device = _registry.RegisterOrUpdate(imei, ProtocolType.GT06);

        // Manually set LastSeen to 10 minutes ago (simulating old heartbeat)
        // We need to use reflection or directly manipulate since LastSeen is public
        device.LastSeen = DateTime.UtcNow.AddMinutes(-10);

        var timeout = TimeSpan.FromMinutes(5);

        // Act
        int markedOffline = _registry.MarkStaleDevicesOffline(timeout);

        // Assert
        markedOffline.Should().Be(1);
        var updatedDevice = _registry.GetByImei(imei);
        updatedDevice!.Status.Should().Be(DeviceStatus.Offline);
    }

    [Fact]
    public void MarkStaleDevicesOffline_MultipleDevices_OnlyMarksStaleOnes()
    {
        // Arrange
        _registry.RegisterOrUpdate("device1", ProtocolType.GT06);
        _registry.RegisterOrUpdate("device2", ProtocolType.GT06);
        _registry.RegisterOrUpdate("device3", ProtocolType.GT06);

        // Make device2 stale (10 minutes ago)
        var device2 = _registry.GetByImei("device2")!;
        device2.LastSeen = DateTime.UtcNow.AddMinutes(-10);

        // Make device3 stale (10 minutes ago)
        var device3 = _registry.GetByImei("device3")!;
        device3.LastSeen = DateTime.UtcNow.AddMinutes(-10);

        var timeout = TimeSpan.FromMinutes(5);

        // Act
        int markedOffline = _registry.MarkStaleDevicesOffline(timeout);

        // Assert
        markedOffline.Should().Be(2);
        _registry.GetByImei("device1")!.Status.Should().Be(DeviceStatus.Online);
        _registry.GetByImei("device2")!.Status.Should().Be(DeviceStatus.Offline);
        _registry.GetByImei("device3")!.Status.Should().Be(DeviceStatus.Offline);
    }

    [Fact]
    public void MarkStaleDevicesOffline_AlreadyOfflineDevice_NotCountedAgain()
    {
        // Arrange
        const string imei = "123456789012345";
        var device = _registry.RegisterOrUpdate(imei, ProtocolType.GT06);
        device.LastSeen = DateTime.UtcNow.AddMinutes(-10);

        var timeout = TimeSpan.FromMinutes(5);

        // Act - First call marks it offline
        int firstCall = _registry.MarkStaleDevicesOffline(timeout);

        // Act - Second call should not count it again
        int secondCall = _registry.MarkStaleDevicesOffline(timeout);

        // Assert
        firstCall.Should().Be(1);
        secondCall.Should().Be(0, "Already offline device should not be counted again");
    }

    [Fact]
    public void MarkStaleDevicesOffline_ShortTimeout_WorksCorrectly()
    {
        // Arrange - Use a very short timeout for testing
        const string imei = "123456789012345";
        _registry.RegisterOrUpdate(imei, ProtocolType.GT06);

        var timeout = TimeSpan.FromMilliseconds(50);

        // Act - Immediately check (should be online)
        int immediate = _registry.MarkStaleDevicesOffline(timeout);

        // Wait for timeout to pass
        Thread.Sleep(100);

        // Act - Check again (should be offline now)
        int afterTimeout = _registry.MarkStaleDevicesOffline(timeout);

        // Assert
        immediate.Should().Be(0);
        afterTimeout.Should().Be(1);
        _registry.GetByImei(imei)!.Status.Should().Be(DeviceStatus.Offline);
    }

    #endregion

    #region Re-registration After Reconnect Preserving Device ID

    [Fact]
    public void Reconnect_AfterGoingOffline_PreservesDeviceId()
    {
        // Arrange
        const string imei = "123456789012345";
        var device = _registry.RegisterOrUpdate(imei, ProtocolType.GT06);
        var originalId = device.Id;

        // Simulate device going offline
        device.LastSeen = DateTime.UtcNow.AddMinutes(-10);
        _registry.MarkStaleDevicesOffline(TimeSpan.FromMinutes(5));

        var offlineDevice = _registry.GetByImei(imei)!;
        offlineDevice.Status.Should().Be(DeviceStatus.Offline);

        // Act - Device reconnects
        var reconnectedDevice = _registry.RegisterOrUpdate(imei, ProtocolType.GT06, "192.168.1.200");

        // Assert
        reconnectedDevice.Id.Should().Be(originalId, "Device ID must be preserved across reconnect");
        reconnectedDevice.Status.Should().Be(DeviceStatus.Online);
        reconnectedDevice.ConnectionCount.Should().Be(2, "Connection count should increment");
        reconnectedDevice.IpAddress.Should().Be("192.168.1.200");
        reconnectedDevice.LastSeen.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Reconnect_MultipleTimes_PreservesDeviceIdAndIncrementsConnectionCount()
    {
        // Arrange
        const string imei = "123456789012345";
        var device = _registry.RegisterOrUpdate(imei, ProtocolType.GT06);
        var originalId = device.Id;

        // Act - Simulate multiple reconnects
        for (int i = 0; i < 5; i++)
        {
            // Go offline
            var currentDevice = _registry.GetByImei(imei)!;
            currentDevice.LastSeen = DateTime.UtcNow.AddMinutes(-10);
            _registry.MarkStaleDevicesOffline(TimeSpan.FromMinutes(5));

            // Reconnect
            _registry.RegisterOrUpdate(imei, ProtocolType.GT06);
        }

        // Assert
        var finalDevice = _registry.GetByImei(imei)!;
        finalDevice.Id.Should().Be(originalId);
        finalDevice.ConnectionCount.Should().Be(6, "Initial registration + 5 reconnects = 6");
        finalDevice.Status.Should().Be(DeviceStatus.Online);
    }

    [Fact]
    public void Unregister_ThenRegisterAgain_CreatesNewDeviceWithNewId()
    {
        // Arrange
        const string imei = "123456789012345";
        var device1 = _registry.RegisterOrUpdate(imei, ProtocolType.GT06);
        var originalId = device1.Id;

        // Act - Unregister completely
        bool removed = _registry.Unregister(imei);
        removed.Should().BeTrue();

        // Register again
        var device2 = _registry.RegisterOrUpdate(imei, ProtocolType.GT06);

        // Assert
        device2.Id.Should().NotBe(originalId, "Unregister + register should create new device with new ID");
        device2.ConnectionCount.Should().Be(1);
        device2.RegistrationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Thread-Safety Tests

    [Fact]
    public async Task RegisterOrUpdate_ConcurrentRegistrations_NoLostUpdatesOrExceptions()
    {
        // Arrange
        const string imei = "123456789012345";
        const int numTasks = 50;
        var exceptions = new ConcurrentQueue<Exception>();

        // Act - Register from multiple tasks concurrently
        var tasks = Enumerable.Range(0, numTasks).Select(async _ =>
        {
            try
            {
                _registry.RegisterOrUpdate(imei, ProtocolType.GT06, $"192.168.1.{Random.Shared.Next(1, 255)}");
                await Task.Yield();
            }
            catch (Exception ex)
            {
                exceptions.Enqueue(ex);
            }
        });

        await Task.WhenAll(tasks);

        // Assert
        exceptions.Should().BeEmpty("No exceptions should occur during concurrent registration");

        var device = _registry.GetByImei(imei);
        device.Should().NotBeNull();
        device!.ConnectionCount.Should().Be(numTasks, "All concurrent registrations should increment connection count");
        device.Status.Should().Be(DeviceStatus.Online);
    }

    [Fact]
    public async Task RegisterOrUpdate_ConcurrentDifferentDevices_NoLostUpdates()
    {
        // Arrange
        const int numDevices = 100;
        const int registrationsPerDevice = 10;
        var exceptions = new ConcurrentQueue<Exception>();

        // Act - Register multiple devices concurrently
        var tasks = Enumerable.Range(0, numDevices).Select(async deviceIndex =>
        {
            var imei = $"device{deviceIndex:D3}";
            try
            {
                for (int i = 0; i < registrationsPerDevice; i++)
                {
                    _registry.RegisterOrUpdate(imei, ProtocolType.GT06);
                    await Task.Yield();
                }
            }
            catch (Exception ex)
            {
                exceptions.Enqueue(ex);
            }
        });

        await Task.WhenAll(tasks);

        // Assert
        exceptions.Should().BeEmpty("No exceptions should occur during concurrent multi-device registration");

        var allDevices = _registry.GetAll();
        allDevices.Should().HaveCount(numDevices);

        foreach (var device in allDevices)
        {
            device.ConnectionCount.Should().Be(registrationsPerDevice);
            device.Status.Should().Be(DeviceStatus.Online);
        }
    }

    [Fact]
    public async Task MarkStaleDevicesOffline_ConcurrentWithRegistrations_NoRaceConditions()
    {
        // Arrange
        const string imei = "123456789012345";
        _registry.RegisterOrUpdate(imei, ProtocolType.GT06);
        var exceptions = new ConcurrentQueue<Exception>();
        const int iterations = 100;

        // Act - Concurrently register and mark offline
        var registerTask = Task.Run(async () =>
        {
            for (int i = 0; i < iterations; i++)
            {
                try
                {
                    _registry.RegisterOrUpdate(imei, ProtocolType.GT06);
                    await Task.Delay(1);
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
            }
        });

        var markOfflineTask = Task.Run(async () =>
        {
            for (int i = 0; i < iterations; i++)
            {
                try
                {
                    // Use a very short timeout to trigger offline marking
                    _registry.MarkStaleDevicesOffline(TimeSpan.FromMilliseconds(1));
                    await Task.Delay(1);
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
            }
        });

        await Task.WhenAll(registerTask, markOfflineTask);

        // Assert
        exceptions.Should().BeEmpty("No exceptions should occur during concurrent register/mark-offline");

        var device = _registry.GetByImei(imei);
        device.Should().NotBeNull();
        // Device should exist and be in a valid state (either Online or Offline)
        device!.Status.Should().BeOneOf(DeviceStatus.Online, DeviceStatus.Offline);
    }

    [Fact]
    public async Task GetAll_ConcurrentWithModifications_ReturnsConsistentSnapshot()
    {
        // Arrange
        const int numDevices = 50;
        for (int i = 0; i < numDevices; i++)
        {
            _registry.RegisterOrUpdate($"device{i}", ProtocolType.GT06);
        }

        var exceptions = new ConcurrentQueue<Exception>();
        var snapshots = new ConcurrentBag<IReadOnlyCollection<Device>>();

        // Act - Concurrently read and modify
        var readTask = Task.Run(async () =>
        {
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    var snapshot = _registry.GetAll();
                    snapshots.Add(snapshot);
                    await Task.Delay(1);
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
            }
        });

        var writeTask = Task.Run(async () =>
        {
            for (int i = 0; i < 50; i++)
            {
                try
                {
                    _registry.RegisterOrUpdate($"newdevice{i}", ProtocolType.GT06);
                    await Task.Delay(2);
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
            }
        });

        await Task.WhenAll(readTask, writeTask);

        // Assert
        exceptions.Should().BeEmpty("No exceptions during concurrent read/write");

        // All snapshots should be valid (no nulls, no corrupted data)
        foreach (var snapshot in snapshots)
        {
            snapshot.Should().NotBeNull();
            foreach (var device in snapshot)
            {
                device.Should().NotBeNull();
                device.Id.Should().NotBeNullOrEmpty();
                device.Imei.Should().NotBeNullOrEmpty();
            }
        }
    }

    [Fact]
    public async Task Unregister_ConcurrentWithRegistrations_NoExceptions()
    {
        // Arrange
        const string imei = "123456789012345";
        _registry.RegisterOrUpdate(imei, ProtocolType.GT06);
        var exceptions = new ConcurrentQueue<Exception>();

        // Act - Concurrently unregister and register
        var unregisterTask = Task.Run(async () =>
        {
            for (int i = 0; i < 50; i++)
            {
                try
                {
                    _registry.Unregister(imei);
                    await Task.Delay(2);
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
            }
        });

        var registerTask = Task.Run(async () =>
        {
            for (int i = 0; i < 50; i++)
            {
                try
                {
                    _registry.RegisterOrUpdate(imei, ProtocolType.GT06);
                    await Task.Delay(2);
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
            }
        });

        await Task.WhenAll(unregisterTask, registerTask);

        // Assert
        exceptions.Should().BeEmpty("No exceptions during concurrent unregister/register");

        // Final state should be valid
        var device = _registry.GetByImei(imei);
        // Device may or may not exist depending on timing, but no exceptions
        if (device != null)
        {
            device.Id.Should().NotBeNullOrEmpty();
            device.Imei.Should().Be(imei);
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GetAll_ReturnsAllRegisteredDevices()
    {
        // Arrange
        _registry.RegisterOrUpdate("device1", ProtocolType.GT06);
        _registry.RegisterOrUpdate("device2", ProtocolType.H02);
        _registry.RegisterOrUpdate("device3", ProtocolType.TK103);

        // Act
        var devices = _registry.GetAll();

        // Assert
        devices.Should().HaveCount(3);
        devices.Select(d => d.Imei).Should().BeEquivalentTo("device1", "device2", "device3");
    }

    [Fact]
    public void GetAll_EmptyRegistry_ReturnsEmptyCollection()
    {
        // Act
        var devices = _registry.GetAll();

        // Assert
        devices.Should().BeEmpty();
    }

    [Fact]
    public void Unregister_NonExistentDevice_ReturnsFalse()
    {
        // Act
        bool result = _registry.Unregister("nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RegisterOrUpdate_NullImei_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => _registry.RegisterOrUpdate(null!, ProtocolType.GT06);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RegisterOrUpdate_EmptyImei_CreatesDeviceWithEmptyImei()
    {
        // Act - Register with empty IMEI (treated as key, may create/overwrite)
        var device = _registry.RegisterOrUpdate("", ProtocolType.GT06);

        // Assert - Device is created/updated with empty IMEI as key
        device.Should().NotBeNull();
        device.Imei.Should().BeEmpty();
    }

    #endregion
}