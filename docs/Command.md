# Command
The `Command` type represents a single command in the gps-tracker-protocol, encapsulating its properties, execution status, and behavior. It provides a structured way to handle commands, including their creation, execution, and tracking of their lifecycle.

## API
### Properties
* `Id`: A unique identifier for the command.
* `DeviceId`: The identifier of the device associated with the command.
* `Type`: The type of command, represented by the `CommandType` enum.
* `Parameters`: A dictionary of parameters for the command, where keys are strings and values are objects.
* `CreatedAt`: The date and time when the command was created.
* `ExecutedAt`: The date and time when the command was executed, or null if not executed.
* `Status`: The current status of the command, represented by the `CommandStatus` enum.
* `Priority`: The priority of the command, represented as an integer.
* `RetryCount`: The number of times the command has been retried.
* `CommandType`: A string representation of the command type.
* `Payload`: The payload of the command, represented as a string.
* `SentTime`: The date and time when the command was sent, or null if not sent.
* `IsSent`: A boolean indicating whether the command has been sent.
* `IsAcknowledged`: A boolean indicating whether the command has been acknowledged.
* `AcknowledgedTime`: The date and time when the command was acknowledged, or null if not acknowledged.
* `MaxRetries`: The maximum number of times the command can be retried.
* `IsValid`: A boolean indicating whether the command is valid.

### Methods
* `Execute()`: Executes the command. This method does not take any parameters and does not return a value.
* `CanRetry()`: Returns a boolean indicating whether the command can be retried.
* `ToFormattedCommand()`: Returns a string representation of the command in a formatted manner.

## Usage
The following examples demonstrate how to use the `Command` type:
```csharp
// Create a new command
var command = new Command
{
    Id = "cmd-123",
    DeviceId = "dev-123",
    Type = CommandType.StartTracking,
    Parameters = new Dictionary<string, object> { { "interval", 10 } },
    Priority = 1,
    MaxRetries = 3
};

// Execute the command
command.Execute();

// Check if the command can be retried
if (command.CanRetry())
{
    // Retry the command
    command.RetryCount++;
    command.Execute();
}
```

```csharp
// Create a new command with a payload
var command = new Command
{
    Id = "cmd-456",
    DeviceId = "dev-456",
    Type = CommandType.SendLocation,
    Payload = "lat:45.123,lon:10.456",
    SentTime = DateTime.Now,
    IsSent = true
};

// Format the command as a string
var formattedCommand = command.ToFormattedCommand();
Console.WriteLine(formattedCommand);
```

## Notes
When working with the `Command` type, consider the following:
* The `Execute` method does not throw any exceptions, but its behavior may depend on the command's status and type.
* The `CanRetry` method returns a boolean value based on the command's retry count and maximum retries.
* The `ToFormattedCommand` method returns a string representation of the command, which can be useful for logging or debugging purposes.
* The `Command` type is not thread-safe by default, so consider using synchronization mechanisms when accessing or modifying its properties and methods concurrently.
* The `IsValid` property can be used to validate the command's state before executing or retrying it.
