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

<!-- (rest of README.md remains the same) -->
