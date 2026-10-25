# CommandService

`CommandService` is the concrete implementation of `ICommandService` (see `Services/CommandService.cs`). It manages the lifecycle of commands sent to tracking devices: validating and creating them against an active device, querying pending and historical commands, marking them executed or failed, re-queuing timed-out commands for retry, and deleting old records.

It is registered as a singleton through `AddGpsTrackerServices()` in `Configuration/DependencyInjection.cs`, which exposes it as `ICommandService`.

## Responsibilities

- **Creation:** Validates a `Command` with `Command.IsValid()`, confirms the target `Device` exists and is active, then assigns a timestamp and `Pending` status before persisting it.
- **Querying:** Looks up commands by ID, lists pending commands, and returns a device's command history sorted newest first.
- **Lifecycle transitions:** Marks commands as executed, or as failed with a retry decision based on `Command.RetryCount` and `Command.MaxRetries`.
- **Retry:** Finds commands that exceeded `ConfigConstants.COMMAND_EXECUTION_TIMEOUT_SECONDS` (120 seconds) and routes each through the failure logic.
- **Cleanup:** Deletes command records created before a cutoff date.
- **Legacy support:** A second constructor accepts generic `IRepository<Command>` and `IRepository<Device>` instances and adds three methods (`SendCommandAsync`, `GetCommandsForDeviceAsync`, `AcknowledgeCommandAsync`) that are not part of `ICommandService`.

## API

### Constructors

- **`CommandService(IUnitOfWork unitOfWork)`**  
  Uses `unitOfWork.Commands` and `unitOfWork.Devices`. This is the path used by the `ICommandService` methods.

- **`CommandService(IRepository<Command> commandRepository, IRepository<Device> deviceRepository)`**  
  Uses the generic repositories. Only the legacy methods work on this instance (see [Notes](#notes)).

### Methods from `ICommandService`

- **`Task<Command> CreateCommandAsync(Command command)`**  
  Validates the command, checks that the target device exists and is active, sets `CreatedAt` to the current UTC time and `Status` to `Pending`, assigns an `Id` if none is set, and persists the command.  
  *Returns:* The persisted command.  
  *Throws:* `ArgumentNullException` if `command` is `null`; `ValidationException` if `command.IsValid()` is `false`; `DeviceException` if the device is not found or `IsActive` is `false`.

- **`Task<Command?> GetCommandAsync(string commandId)`**  
  Retrieves a command by ID.  
  *Returns:* The command, or `null` if no command has that ID.  
  *Throws:* `ArgumentException` if `commandId` is null, empty, or whitespace.

- **`Task<IEnumerable<Command>> GetPendingCommandsAsync()`**  
  Retrieves all commands whose status is pending.  
  *Returns:* The pending commands.

- **`Task<IEnumerable<Command>> GetCommandHistoryAsync(string deviceId)`**  
  Retrieves every command for a device, sorted by `CreatedAt` descending.  
  *Returns:* A list ordered newest to oldest; empty if the device has no commands.  
  *Throws:* `ArgumentException` if `deviceId` is null, empty, or whitespace.

- **`Task<bool> ExecuteCommandAsync(string commandId)`**  
  Calls `Command.Execute()`, which sets `ExecutedAt` to the current UTC time and `Status` to `Executed`, then persists the change. The current status is not checked, so a command in any state can be executed.  
  *Returns:* Always `true` on success.  
  *Throws:* `CommandException` if the command is not found.

- **`Task<bool> MarkCommandAsFailedAsync(string commandId)`**  
  Calls `Command.CanRetry()`. If retries remain, it increments `RetryCount`, sets `Status` to `Pending`, and persists the command. If not, `CanRetry()` sets `Status` to `Failed`, and the command is persisted in that state.  
  *Returns:* `true` if the command was re-queued; `false` if no retries remain.  
  *Throws:* `CommandException` if the command is not found.

- **`Task<int> RetryFailedCommandsAsync()`**  
  Loads commands still in `Pending` status whose `CreatedAt` is more than 120 seconds in the past, then calls `MarkCommandAsFailedAsync` on each.  
  *Returns:* The number of commands re-queued. Commands with no retries left are still marked `Failed` but are not counted.

- **`Task<int> CleanupOldCommandsAsync(DateTime olderThan)`**  
  Deletes commands created before `olderThan`.  
  *Returns:* The number of commands deleted.  
  *Throws:* `ArgumentException` if `olderThan` is not earlier than the current UTC time.

### Legacy methods (not on `ICommandService`)

These require the generic-repository constructor. On a unit-of-work instance they throw `InvalidOperationException`.

- **`Task<Command?> SendCommandAsync(string deviceId, string commandType, string payload)`**  
  Returns `null` if the device does not exist. Otherwise creates a command with `IsSent = true`, `SentTime` set to the current UTC time, and the given `CommandType` label and `Payload`, then adds it to the command repository.  
  *Throws:* `InvalidOperationException` if the service was not built with generic repositories.

- **`Task<IEnumerable<Command>> GetCommandsForDeviceAsync(string deviceId)`**  
  Returns all commands whose `DeviceId` matches.  
  *Throws:* `InvalidOperationException` if the service was not built with generic repositories.

- **`Task AcknowledgeCommandAsync(string commandId)`**  
  Sets `IsAcknowledged = true` and `AcknowledgedTime` to the current UTC time, then updates the command. Does nothing if the command is not found.  
  *Throws:* `InvalidOperationException` if the service was not built with generic repositories.

## Usage

### Example 1: Creating a Command and Recording Its Execution

The example uses `InMemoryUnitOfWork` so it runs without a database.

```csharp
using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Services;

var unitOfWork = new InMemoryUnitOfWork();
var commandService = new CommandService(unitOfWork);

// The target device must exist and be active
await unitOfWork.Devices.AddAsync(new Device { Id = "DEV-1234", IsActive = true });

// SetGpsInterval requires an "interval" parameter (see Command.IsValid)
var command = await commandService.CreateCommandAsync(new Command
{
    DeviceId = "DEV-1234",
    Type = CommandType.SetGpsInterval,
    Parameters = new Dictionary<string, object> { ["interval"] = 60 }
});

Console.WriteLine($"Created {command.Id} with status {command.Status}");

// Mark the command as executed once the device confirms it
bool executed = await commandService.ExecuteCommandAsync(command.Id);
Console.WriteLine($"Executed: {executed}");
```

### Example 2: Handling Failures and Retries

```csharp
// A fresh command that has not been executed yet
var pendingCommand = await commandService.CreateCommandAsync(new Command
{
    DeviceId = "DEV-1234",
    Type = CommandType.RequestLocation
});

// Report a delivery failure. Returns true while retries remain.
bool requeued = await commandService.MarkCommandAsFailedAsync(pendingCommand.Id);
if (!requeued)
{
    Console.WriteLine("Retry limit reached; command is now Failed.");
}

// Periodically re-queue commands that have timed out
int retried = await commandService.RetryFailedCommandsAsync();
Console.WriteLine($"Re-queued {retried} timed-out commands");

var pending = await commandService.GetPendingCommandsAsync();
Console.WriteLine($"Pending commands: {pending.Count()}");
```

### Example 3: Device History and Maintenance

```csharp
// Newest first
var history = await commandService.GetCommandHistoryAsync("DEV-1234");
foreach (var item in history)
{
    Console.WriteLine($"{item.CreatedAt:u} {item.Type} {item.Status}");
}

// Delete records older than 30 days
int deleted = await commandService.CleanupOldCommandsAsync(DateTime.UtcNow.AddDays(-30));
Console.WriteLine($"Deleted {deleted} old commands");
```

### Example 4: Error Handling

```csharp
try
{
    await commandService.CreateCommandAsync(new Command
    {
        DeviceId = "UNKNOWN-DEVICE",
        Type = CommandType.RequestLocation
    });
}
catch (DeviceException ex)
{
    Console.WriteLine($"Device problem: {ex.Message}");
}
catch (ValidationException ex)
{
    Console.WriteLine($"Invalid command: {ex.Message}");
}
```

## Notes

- **Constructor choice:** The `ICommandService` methods use `_repository` and `_deviceRepository`, which are only set by the unit-of-work constructor. A service built with the generic-repository constructor leaves them `null`, so calling `CreateCommandAsync`, `GetCommandAsync`, `ExecuteCommandAsync`, or any other `ICommandService` method on it throws `NullReferenceException`. Use the unit-of-work constructor for these methods, and use the generic-repository constructor only for the legacy methods.
- **Status is not checked on execution:** `ExecuteCommandAsync` does not verify that the command was sent or acknowledged. Callers are responsible for calling it in the right order.
- **Status is not checked on failure:** `MarkCommandAsFailedAsync` does not check the current status either. Calling it on an executed command sets that command back to `Pending` if retries remain, so only call it for commands that actually failed.
- **Retry accounting:** Each call to `MarkCommandAsFailedAsync` that re-queues a command increments `RetryCount`. The command is re-queued only while `RetryCount < MaxRetries`, which is 3 by default.
- **Batch failures:** `RetryFailedCommandsAsync` processes commands one at a time. If a repository call throws partway through, the commands already processed stay updated and the exception propagates.
- **Persistence:** `CommandService` does not call `IUnitOfWork.SaveChangesAsync` or any transaction method. It only calls the command and device repositories, so any commit or transaction handling has to happen outside the service.
- **Thread safety:** The class has no internal locking. Concurrent calls that touch the same command can interleave, so the underlying repositories or the caller must serialize them.
