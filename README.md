# GpsTracker Protocol

<!-- (rest of README.md remains the same) -->

## CommandServiceTests

The `CommandServiceTests` class provides unit tests for the `CommandService` functionality, covering command sending, retrieval, and acknowledgment operations. It uses mock repositories with NSubstitute to test various scenarios including successful command transmission, device not found cases, and command acknowledgment workflows.

Example usage in a test project:

```csharp
using Xunit;
using NSubstitute;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Data;
using System.Threading.Tasks;

public class CommandServiceIntegrationTests
{
    private readonly IRepository<Command> _commandRepository = Substitute.For<IRepository<Command>>();
    private readonly IRepository<Device> _deviceRepository = Substitute.For<IRepository<Device>>();
    private readonly CommandService _commandService;

    public CommandServiceIntegrationTests()
    {
        _commandService = new CommandService(_commandRepository, _deviceRepository);
    }

    [Fact]
    public async Task SendCommandAsync_ShouldAddCommandAndMarkAsSent()
    {
        // Arrange
        var deviceId = "device123";
        var commandType = "REBOOT_DEVICE";
        var payload = "immediate";
        
        _deviceRepository.GetByIdAsync(deviceId).Returns(new Device { Id = deviceId, IsActive = true });

        // Act
        var command = await _commandService.SendCommandAsync(deviceId, commandType, payload);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(deviceId, command.DeviceId);
        Assert.Equal(commandType, command.CommandType);
        Assert.True(command.IsSent);
        Assert.NotNull(command.SentTime);
    }

    [Fact]
    public async Task GetCommandsForDeviceAsync_ShouldReturnCommands()
    {
        // Arrange
        var deviceId = "device123";
        var commands = new List<Command>
        {
            new Command { Id = "cmd1", DeviceId = deviceId, CommandType = "REBOOT_DEVICE", IsSent = true },
            new Command { Id = "cmd2", DeviceId = deviceId, CommandType = "UPDATE_FIRMWARE", IsSent = false }
        };
        
        _commandRepository.FindManyAsync(Arg.Any<Func<Command, bool>>())
            .Returns(ci => commands.FindAll(c => ci.Arg<Func<Command, bool>>().Invoke(c)));

        // Act
        var result = await _commandService.GetCommandsForDeviceAsync(deviceId);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, c => c.CommandType == "REBOOT_DEVICE");
    }

    [Fact]
    public async Task AcknowledgeCommandAsync_ShouldMarkCommandAsAcknowledged()
    {
        // Arrange
        var commandId = "cmd123";
        var command = new Command { Id = commandId, IsSent = true, IsAcknowledged = false };
        
        _commandRepository.GetByIdAsync(commandId).Returns(command);

        // Act
        await _commandService.AcknowledgeCommandAsync(commandId);

        // Assert
        Assert.True(command.IsAcknowledged);
        Assert.NotNull(command.AcknowledgedTime);
    }
}
```

<!-- (rest of README.md remains the same) -->

## IDomainEvent

The `IDomainEvent` interface represents a domain event in the system. It provides metadata such as the event ID, timestamp, and aggregate ID, which can be used to track and manage events in the system.

Example usage:

```csharp
using GpsTrackerProtocol.Events;

public class EventPublisherExample
{
    public async Task PublishEventAsync()
    {
        // Create a new event
        var locationUpdatedEvent = new LocationUpdatedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            AggregateId = "device123",
            DeviceId = "device123",
            Location = new LocationData
            {
                Latitude = 37.7749,
                Longitude = -122.4194
            }
        };

        // Publish the event
        var eventPublisher = new EventPublisher();
        await eventPublisher.PublishAsync(locationUpdatedEvent);
    }
}
```

<!-- (rest of README.md remains the same) -->

## CommandServiceTests

The `CommandServiceTests` class provides unit tests for the `CommandService` functionality, covering command sending, retrieval, and acknowledgment operations. It uses mock repositories with NSubstitute to test various scenarios including successful command transmission, device not found cases, and command acknowledgment workflows.

Example usage in a test project:

```csharp
using Xunit;
using NSubstitute;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Data;
using System.Threading.Tasks;

public class CommandServiceIntegrationTests
{
    private readonly IRepository<Command> _commandRepository = Substitute.For<IRepository<Command>>();
    private readonly IRepository<Device> _deviceRepository = Substitute.For<IRepository<Device>>();
    private readonly CommandService _commandService;

    public CommandServiceIntegrationTests()
    {
        _commandService = new CommandService(_commandRepository, _deviceRepository);
    }

    [Fact]
    public async Task SendCommandAsync_ShouldAddCommandAndMarkAsSent()
    {
        // Arrange
        var deviceId = "device123";
        var commandType = "REBOOT_DEVICE";
        var payload = "immediate";
        
        _deviceRepository.GetByIdAsync(deviceId).Returns(new Device { Id = deviceId, IsActive = true });

        // Act
        var command = await _commandService.SendCommandAsync(deviceId, commandType, payload);

        // Assert
        Assert.NotNull(command);
        Assert.Equal(deviceId, command.DeviceId);
        Assert.Equal(commandType, command.CommandType);
        Assert.True(command.IsSent);
        Assert.NotNull(command.SentTime);
    }

    [Fact]
    public async Task GetCommandsForDeviceAsync_ShouldReturnCommands()
    {
        // Arrange
        var deviceId = "device123";
        var commands = new List<Command>
        {
            new Command { Id = "cmd1", DeviceId = deviceId, CommandType = "REBOOT_DEVICE", IsSent = true },
            new Command { Id = "cmd2", DeviceId = deviceId, CommandType = "UPDATE_FIRMWARE", IsSent = false }
        };
        
        _commandRepository.FindManyAsync(Arg.Any<Func<Command, bool>>())
            .Returns(ci => commands.FindAll(c => ci.Arg<Func<Command, bool>>().Invoke(c)));

        // Act
        var result = await _commandService.GetCommandsForDeviceAsync(deviceId);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, c => c.CommandType == "REBOOT_DEVICE");
    }

    [Fact]
    public async Task AcknowledgeCommandAsync_ShouldMarkCommandAsAcknowledged()
    {
        // Arrange
        var commandId = "cmd123";
        var command = new Command { Id = commandId, IsSent = true, IsAcknowledged = false };
        
        _commandRepository.GetByIdAsync(commandId).Returns(command);

        // Act
        await _commandService.AcknowledgeCommandAsync(commandId);

        // Assert
        Assert.True(command.IsAcknowledged);
        Assert.NotNull(command.AcknowledgedTime);
    }
}
```

<!-- (rest of README.md remains the same) -->

## GeofenceEnteredEvent

The `GeofenceEnteredEvent` represents a device crossing into a geofence zone. It contains metadata such as the event ID, timestamp, device and geofence identifiers, and the device's location and speed at the moment of entry.

Example usage:

```csharp
using System;
using GpsTrackerProtocol.Events;

var geofenceEvent = new GeofenceEnteredEvent
{
    EventId = Guid.NewGuid().ToString(),
    Timestamp = DateTime.UtcNow,
    AggregateId = "device123",
    DeviceId = "device123",
    GeofenceId = "zoneA",
    Latitude = 37.7749,
    Longitude = -122.4194,
    Speed = 45.0
};

Console.WriteLine($"Event {geofenceEvent.EventId} for device {geofenceEvent.DeviceId} entered geofence {geofenceEvent.GeofenceId} at {geofenceEvent.Timestamp}.");
```

<!-- (rest of README.md remains the same) -->

## GeofenceAlertingServiceTests

The `GeofenceAlertingServiceTests` class provides unit tests for the `GeofenceAlertingService` functionality, covering alert rule creation, geofence event processing, alert acknowledgment, and rule deletion. It uses mock event publishers with NSubstitute to test various scenarios including alert triggering, cooldown periods, and alert suppression.

Example usage in a test project:

```csharp
using Xunit;
using NSubstitute;
using FluentAssertions;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Events;
using Microsoft.Extensions.Logging;

public class GeofenceAlertingServiceTestsDemo
{
    private readonly IEventPublisher _publisher;
    private readonly GeofenceAlertingService _alertingService;

    public GeofenceAlertingServiceTestsDemo()
    {
        _publisher = Substitute.For<IEventPublisher>();
        _publisher
            .Subscribe(Arg.Any<Func<GeofenceEnteredEvent, Task>>())
            .Returns(Substitute.For<IDisposable>());
        _publisher
            .Subscribe(Arg.Any<Func<GeofenceExitedEvent, Task>>())
            .Returns(Substitute.For<IDisposable>());

        _alertingService = new GeofenceAlertingService(
            _publisher,
            Substitute.For<ILogger<GeofenceAlertingService>>());
    }

    [Fact]
    public void CreateAlertRule_ShouldAddRuleForDevice()
    {
        // Arrange & Act
        var rule = _alertingService.CreateAlertRule("device-1", "zone-hq", GeofenceAlertType.Enter);

        // Assert
        Assert.NotNull(rule);
        Assert.Equal("device-1", rule.DeviceId);
        Assert.Equal("zone-hq", rule.GeofenceId);
        Assert.Equal(GeofenceAlertType.Enter, rule.AlertType);
        Assert.True(rule.IsEnabled);
        
        var rules = _alertingService.GetRulesForDevice("device-1");
        Assert.Single(rules);
    }

    [Fact]
    public void ProcessGeofenceEntered_ShouldFireAlertWhenMatchingRuleExists()
    {
        // Arrange
        _alertingService.CreateAlertRule("device-2", "zone-a", GeofenceAlertType.Enter, cooldown: TimeSpan.Zero);

        var @event = new GeofenceEnteredEvent
        {
            DeviceId = "device-2",
            GeofenceId = "zone-a",
            Latitude = 51.5,
            Longitude = -0.1,
            Speed = 30
        };

        // Act
        _alertingService.ProcessGeofenceEntered(@event);

        // Assert
        var alerts = _alertingService.GetActiveAlerts("device-2");
        Assert.Single(alerts);
        Assert.Equal(GeofenceAlertType.Enter, alerts[0].AlertType);
        Assert.Equal(GeofenceAlertStatus.Active, alerts[0].Status);
    }

    [Fact]
    public void ProcessGeofenceEntered_ShouldSuppressAlertWithinCooldown()
    {
        // Arrange
        _alertingService.CreateAlertRule("device-3", "zone-b", GeofenceAlertType.Enter, 
            cooldown: TimeSpan.FromHours(1));

        var @event = new GeofenceEnteredEvent
        {
            DeviceId = "device-3",
            GeofenceId = "zone-b",
            Latitude = 51.5,
            Longitude = -0.1
        };

        // Act
        _alertingService.ProcessGeofenceEntered(@event);
        _alertingService.ProcessGeofenceEntered(@event);

        // Assert
        var history = _alertingService.GetAlertHistory("device-3");
        Assert.Equal(2, history.Count);
        Assert.Single(history.Where(a => a.Status == GeofenceAlertStatus.Active));
        Assert.Single(history.Where(a => a.Status == GeofenceAlertStatus.Suppressed));
    }

    [Fact]
    public void AcknowledgeAlert_ShouldMarkAlertAsAcknowledged()
    {
        // Arrange
        _alertingService.CreateAlertRule("device-4", "zone-c", GeofenceAlertType.Exit, cooldown: TimeSpan.Zero);
        _alertingService.ProcessGeofenceExited(new GeofenceExitedEvent
        {
            DeviceId = "device-4",
            GeofenceId = "zone-c"
        });

        var alert = _alertingService.GetActiveAlerts("device-4").First();

        // Act
        var result = _alertingService.AcknowledgeAlert(alert.Id, "reviewed by operator");

        // Assert
        Assert.True(result);
        Assert.Empty(_alertingService.GetActiveAlerts("device-4"));
    }

    [Fact]
    public void DeleteAlertRule_ShouldRemoveRule()
    {
        // Arrange
        var rule = _alertingService.CreateAlertRule("device-5", "zone-d", GeofenceAlertType.Enter);

        // Act
        _alertingService.DeleteAlertRule(rule.Id);

        // Assert
        Assert.Empty(_alertingService.GetRulesForDevice("device-5"));
    }

    [Fact]
    public void ProcessGeofenceEntered_ShouldNotFireAlert_WhenNoMatchingRule()
    {
        // Arrange & Act
        _alertingService.ProcessGeofenceEntered(new GeofenceEnteredEvent
        {
            DeviceId = "device-no-rule",
            GeofenceId = "zone-x"
        });

        // Assert
        Assert.Empty(_alertingService.GetActiveAlerts("device-no-rule"));
    }
}
```

<!-- (rest of README.md remains the same) -->

## DeviceServiceTests

`DeviceServiceTests` provides a comprehensive test suite for the `DeviceService` class, verifying device registration, lookup, status updates, and bulk retrieval. The tests confirm correct handling of new devices, already‑registered devices, missing devices, and status changes.

```csharp
using System;
using System.Threading.Tasks;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Data;
using NSubstitute;

public class DeviceServiceTestsDemo
{
    public async Task RunAllTestsAsync()
    {
        // Arrange shared dependencies
        var deviceRepository = Substitute.For<IRepository<Device>>();
        var deviceService = new DeviceService(deviceRepository);
        var tests = new DeviceServiceTests();

        // Register a new device (adds it)
        await tests.RegisterDeviceAsync_ShouldAddDevice();

        // Register an existing device (returns the existing instance)
        await tests.RegisterDeviceAsync_ShouldReturnExistingDevice_IfAlreadyRegistered();

        // Retrieve an existing device by its identifier
        await tests.GetDeviceByIdAsync_ShouldReturnDevice();

        // Attempt to retrieve a non‑existent device (expects null)
        await tests.GetDeviceByIdAsync_ShouldReturnNull_WhenDeviceNotFound();

        // Update the status of a known device
        await tests.UpdateDeviceStatusAsync_ShouldUpdateDevice();

        // Attempt to update a device that does not exist (no action)
        await tests.UpdateDeviceStatusAsync_ShouldDoNothing_WhenDeviceNotFound();

        // Retrieve all devices from the repository
        await tests.GetAllDevicesAsync_ShouldReturnAllDevices();
    }
}
```

<!-- (rest of README.md remains the same) -->

## DeviceDiagnosticsServiceTests

The `DeviceDiagnosticsServiceTests` class provides unit tests for the `DeviceDiagnosticsService` class, covering device diagnostics retrieval, self-test execution, and connectivity analysis. It uses mock repositories with NSubstitute to test various scenarios including healthy devices, low battery conditions, weak signal situations, and missing devices.

Example usage in a diagnostics dashboard or monitoring service:

```csharp
using System;
using System.Threading.Tasks;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Data;
using Microsoft.Extensions.Logging;
using NSubstitute;

public class DeviceDiagnosticsDashboard
{
private readonly IDeviceDiagnosticsService _diagnosticsService;

public DeviceDiagnosticsDashboard(IDeviceDiagnosticsService diagnosticsService)
{
_diagnosticsService = diagnosticsService;
}

public async Task MonitorDeviceHealthAsync(string deviceId)
{
// Get comprehensive diagnostics for a specific device
var report = await _diagnosticsService.GetDiagnosticsAsync(deviceId);

if (report is null)
{
Console.WriteLine($"Device {deviceId} not found.");
return;
}

Console.WriteLine($"Device: {report.DeviceName} ({report.Imei})");
Console.WriteLine($"Status: {(report.IsOnline ? "Online" : "Offline")} - Last seen: {report.LastSeen:HH:mm:ss}");
Console.WriteLine($"Battery: {report.BatteryLevel}% - Signal: {report.SignalStrength} dBm ({report.SignalQuality})");
Console.WriteLine($"Location points: {report.TotalLocationPoints} - Journeys: {report.TotalJourneys}");
Console.WriteLine($"Total distance: {report.TotalDistanceKm:F1} km");

// Run a self-test and cache the result
var selfTest = await _diagnosticsService.RunSelfTestAsync(deviceId);

if (selfTest is not null)
{
Console.WriteLine($"\nSelf-test results:");
Console.WriteLine($"  Connectivity: {(selfTest.ConnectivityOk ? "OK" : "FAIL")}");
Console.WriteLine($"  Battery: {(selfTest.BatteryOk ? "OK" : "FAIL")}");
Console.WriteLine($"  Signal: {(selfTest.SignalOk ? "OK" : "FAIL")}");
Console.WriteLine($"  Location data: {(selfTest.LocationDataOk ? "OK" : "FAIL")}");

if (selfTest.Warnings.Any())
{
Console.WriteLine("  Warnings:");
foreach (var warning in selfTest.Warnings)
Console.WriteLine($"    - {warning}");
}
}
}

public async Task GenerateFleetHealthReportAsync()
{
// Get fleet-wide health metrics
var fleetReport = await _diagnosticsService.GetFleetHealthReportAsync();

Console.WriteLine($"\n=== Fleet Health Report ===");
Console.WriteLine($"Total devices: {fleetReport.TotalDevices}");
Console.WriteLine($"Online: {fleetReport.OnlineDevices} | Offline: {fleetReport.OfflineDevices}");
Console.WriteLine($"Low battery: {fleetReport.LowBatteryDevices} | Weak signal: {fleetReport.WeakSignalDevices}");
Console.WriteLine($"Generated at: {fleetReport.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
}
}

// Example usage:
var unitOfWork = Substitute.For<IUnitOfWork>();
var logger = Substitute.For<ILogger<DeviceDiagnosticsService>>();
var diagnosticsService = new DeviceDiagnosticsService(unitOfWork, logger);

var dashboard = new DeviceDiagnosticsDashboard(diagnosticsService);
await dashboard.MonitorDeviceHealthAsync("device-123");
await dashboard.GenerateFleetHealthReportAsync();
```

<!-- (rest of README.md remains the same) -->

## AnalyticsServiceTests

The `AnalyticsServiceTests` class provides unit tests for the `AnalyticsService` functionality, covering journey analytics including total journey counts, average journey durations, and device activity tracking. It uses mock repositories with NSubstitute to test various scenarios including journeys with different durations, empty repositories, and device activity calculations.

Example usage in a test project:

```csharp
using Xunit;
using NSubstitute;
using FluentAssertions;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Data;
using System.Threading.Tasks;
using System;

public class AnalyticsServiceTestsDemo
{
    private readonly IRepository<Journey> _journeyRepository = Substitute.For<IRepository<Journey>>();
    private readonly IRepository<LocationData> _locationDataRepository = Substitute.For<IRepository<LocationData>>();
    private readonly AnalyticsService _analyticsService;

    public AnalyticsServiceTestsDemo()
    {
        _analyticsService = new AnalyticsService(_journeyRepository, _locationDataRepository);
    }

    [Fact]
    public async Task GetTotalJourneys_ShouldReturnCorrectCount()
    {
        // Arrange
        _journeyRepository.GetAllAsync().Returns(new List<Journey>
        {
            new Journey { Id = "journey1", DeviceId = "device1", StartTime = DateTime.UtcNow.AddHours(-2), EndTime = DateTime.UtcNow.AddHours(-1) },
            new Journey { Id = "journey2", DeviceId = "device1", StartTime = DateTime.UtcNow.AddHours(-4), EndTime = DateTime.UtcNow.AddHours(-3) }
        });

        // Act
        var totalJourneys = await _analyticsService.GetTotalJourneysAsync().ConfigureAwait(false);

        // Assert
        Assert.Equal(2, totalJourneys);
    }

    [Fact]
    public async Task GetAverageJourneyDuration_ShouldReturnCorrectAverage()
    {
        // Arrange
        _journeyRepository.GetAllAsync().Returns(new List<Journey>
        {
            new Journey { Id = "journey1", DeviceId = "device1", StartTime = DateTime.UtcNow.AddHours(-2), EndTime = DateTime.UtcNow.AddHours(-1) },
            new Journey { Id = "journey2", DeviceId = "device1", StartTime = DateTime.UtcNow.AddHours(-4), EndTime = DateTime.UtcNow.AddHours(-2) }
        });

        // Act
        var averageDuration = await _analyticsService.GetAverageJourneyDurationAsync().ConfigureAwait(false);

        // Assert
        Assert.Equal(TimeSpan.FromHours(1.5), averageDuration);
    }

    [Fact]
    public async Task GetMostActiveDevice_ShouldReturnCorrectDeviceId()
    {
        // Arrange
        _journeyRepository.GetAllAsync().Returns(new List<Journey>
        {
            new Journey { Id = "j1", DeviceId = "deviceA", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddMinutes(30) },
            new Journey { Id = "j2", DeviceId = "deviceB", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddMinutes(30) },
            new Journey { Id = "j3", DeviceId = "deviceA", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddMinutes(30) }
        });

        // Act
        var mostActiveDevice = await _analyticsService.GetMostActiveDeviceAsync().ConfigureAwait(false);

        // Assert
        Assert.Equal("deviceA", mostActiveDevice);
    }
}
```

## DomainAndServiceTests

The `DomainAndServiceTests` class provides comprehensive unit tests for core domain models (`LocationData`, `Device`, `GpsFrame`) and the `GeofenceService`, covering validation rules, geometric calculations, and geofence containment checks. Tests validate edge cases such as invalid coordinates, malformed device identifiers, checksum validation, and service behavior with missing or invalid inputs.

Example usage in a test project:

```csharp
using Xunit;
using FluentAssertions;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;

public class DomainValidationTests
{
    [Fact]
    public void ValidateLocationData_WithValidCoordinatesAndDeviceId_ReturnsTrue()
    {
        // Arrange
        var location = new LocationData
        {
            DeviceId = "device-001",
            Latitude = 51.5074,
            Longitude = -0.1278,
            Speed = 60,
            Bearing = 270,
            SatelliteCount = 9
        };

        // Act
        var isValid = location.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateDevice_WithValidImei_ReturnsTrue()
    {
        // Arrange
        var device = new Device
        {
            Id = "device-001",
            Imei = "123456789012345"
        };

        // Act
        var isValid = device.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateGpsFrame_WithValidGT06Frame_ReturnsTrue()
    {
        // Arrange
        var frame = new GpsFrame
        {
            Protocol = ProtocolType.GT06,
            RawData = new byte[20],
            IsValidChecksum = true
        };

        // Act
        var isValid = frame.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void GeofenceService_CheckPointInsideGeofence_ReturnsTrue()
    {
        // Arrange
        var service = new GeofenceService();
        service.AddGeofence("london-zone", 51.5074, -0.1278, 1.0);

        // Act
        var isInside = service.IsInsideGeofence("london-zone", 51.5074, -0.1278);

        // Assert
        isInside.Should().BeTrue();
    }
}
```

<!-- (rest of README.md remains the same) -->
