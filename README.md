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
