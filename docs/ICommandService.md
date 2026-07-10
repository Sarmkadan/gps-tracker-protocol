# ICommandService

`ICommandService` defines the contract for managing command dispatching, acknowledgment, execution, and lifecycle within the GPS tracker protocol. It serves as the central interface for sending commands to remote devices, retrieving pending and historical commands, and handling retry and cleanup operations.

## API

### Constructors

- **`CommandService`**  
  Instantiates a new instance of the command service implementation. The concrete constructor is expected to accept dependencies required for command persistence, transport, and execution.

### Methods

- **`async Task<Command?> SendCommandAsync`**  
  Sends a command to a target device and returns the resulting `Command` object if the send operation succeeds, or `null` if the command could not be dispatched.  
  *Parameters:* Accepts a command definition and a device identifier.  
  *Returns:* A `Command` instance representing the sent command, or `null`.  
  *Throws:* May throw if the underlying transport layer encounters an unrecoverable error.

- **`async Task<IEnumerable<Command>> GetCommandsForDeviceAsync`**  
  Retrieves all commands associated with a specific device, regardless of their status.  
  *Parameters:* A device identifier.  
  *Returns:* An enumerable collection of `Command` objects.  
  *Throws:* May throw if the persistence layer is unavailable.

- **`async Task AcknowledgeCommandAsync`**  
  Marks a command as acknowledged by the target device, confirming receipt.  
  *Parameters:* The command identifier to acknowledge.  
  *Returns:* A completed task.  
  *Throws:* May throw if the command does not exist or is in a state that cannot be acknowledged.

- **`async Task<Command> CreateCommandAsync`**  
  Creates a new command record in the system without immediately sending it.  
  *Parameters:* A command definition containing type, payload, and target device.  
  *Returns:* The newly created `Command` object.  
  *Throws:* May throw if validation fails or persistence is unavailable.

- **`async Task<Command?> GetCommandAsync`**  
  Retrieves a single command by its unique identifier.  
  *Parameters:* A command identifier.  
  *Returns:* The matching `Command` object, or `null` if not found.  
  *Throws:* May throw if the persistence layer is unavailable.

- **`async Task<IEnumerable<Command>> GetPendingCommandsAsync`**  
  Retrieves all commands that have been created or sent but not yet acknowledged or executed.  
  *Parameters:* None, or optionally a device filter.  
  *Returns:* An enumerable collection of pending `Command` objects.  
  *Throws:* May throw if the persistence layer is unavailable.

- **`async Task<IEnumerable<Command>> GetCommandHistoryAsync`**  
  Retrieves historical commands, typically those that have reached a terminal state (executed, failed, or expired).  
  *Parameters:* Optional filtering criteria such as device identifier or time range.  
  *Returns:* An enumerable collection of historical `Command` objects.  
  *Throws:* May throw if the persistence layer is unavailable.

- **`async Task<bool> ExecuteCommandAsync`**  
  Attempts to execute a command that has been acknowledged, applying its intended effect.  
  *Parameters:* The command identifier to execute.  
  *Returns:* `true` if execution succeeded; `false` if the command could not be executed.  
  *Throws:* May throw if the command is not in an executable state.

- **`async Task<bool> MarkCommandAsFailedAsync`**  
  Marks a command as failed, typically due to a delivery or execution error.  
  *Parameters:* The command identifier and optionally a failure reason.  
  *Returns:* `true` if the status was updated successfully; `false` otherwise.  
  *Throws:* May throw if the command does not exist.

- **`async Task<int> RetryFailedCommandsAsync`**  
  Re-enqueues commands that are in a failed state for another delivery attempt.  
  *Parameters:* Optional batch size or age threshold.  
  *Returns:* The number of commands that were successfully re-queued for retry.  
  *Throws:* May throw if the persistence or transport layer is unavailable.

- **`async Task<int> CleanupOldCommandsAsync`**  
  Removes or archives commands that have exceeded a retention threshold.  
  *Parameters:* A retention period or cutoff timestamp.  
  *Returns:* The number of commands cleaned up.  
  *Throws:* May throw if the persistence layer is unavailable.

## Usage

### Example 1: Sending and Tracking a Command

```csharp
// Create and send a command to a device
var command = await commandService.CreateCommandAsync(new CommandDefinition
{
    DeviceId = "DEV-1234",
    Type = CommandType.LocationRequest,
    Payload = Encoding.UTF8.GetBytes("{\"interval\": 60}")
});

var sentCommand = await commandService.SendCommandAsync(command.Id, "DEV-1234");
if (sentCommand != null)
{
    // Wait for acknowledgment from the device
    await commandService.AcknowledgeCommandAsync(sentCommand.Id);
    
    // Execute the acknowledged command
    bool executed = await commandService.ExecuteCommandAsync(sentCommand.Id);
    Console.WriteLine($"Command executed: {executed}");
}
```

### Example 2: Retry and Cleanup Workflow

```csharp
// Retry all failed commands that are less than 24 hours old
int retriedCount = await commandService.RetryFailedCommandsAsync(
    maxAge: TimeSpan.FromHours(24),
    batchSize: 50
);
Console.WriteLine($"Retried {retriedCount} commands");

// Clean up commands older than 30 days
int cleanedCount = await commandService.CleanupOldCommandsAsync(
    retentionPeriod: TimeSpan.FromDays(30)
);
Console.WriteLine($"Cleaned up {cleanedCount} old commands");

// Verify pending queue after maintenance
var pendingCommands = await commandService.GetPendingCommandsAsync();
Console.WriteLine($"Remaining pending commands: {pendingCommands.Count()}");
```

## Notes

- **State transitions:** Commands follow a defined lifecycle. `AcknowledgeCommandAsync` should only be called for commands that have been sent. `ExecuteCommandAsync` should only be called for commands that have been acknowledged. Calling these methods on commands in invalid states will result in exceptions.
- **Null returns:** `SendCommandAsync` and `GetCommandAsync` return `null` when the target command does not exist or cannot be dispatched. Callers must handle null results to avoid null-reference exceptions.
- **Batch operations:** `RetryFailedCommandsAsync` and `CleanupOldCommandsAsync` operate on multiple records. Their return values indicate the count of affected items, which may be zero if no commands match the criteria.
- **Thread safety:** Implementations of this interface are expected to be thread-safe. Concurrent calls to methods affecting the same command (e.g., simultaneous `AcknowledgeCommandAsync` and `MarkCommandAsFailedAsync`) must be serialized by the underlying implementation to prevent race conditions.
- **Persistence dependency:** Most methods depend on an underlying persistence store. Transient failures in the store may cause exceptions that callers should be prepared to retry with appropriate back-off strategies.
