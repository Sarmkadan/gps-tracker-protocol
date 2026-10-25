#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GpsTrackerProtocol.Data;
using GpsTrackerProtocol.Domain;
using GpsTrackerProtocol.Domain.Models;

namespace GpsTrackerProtocol.Services;

/// <summary>
/// Service for managing device commands, including creation, execution, retry, and cleanup.
/// </summary>
public interface ICommandService
{
    /// <summary>
    /// Creates a new command for a device after validating the command and verifying the target device exists and is active.
    /// </summary>
    /// <param name="command">The command to create.</param>
    /// <returns>The created command with assigned ID, timestamp, and pending status.</returns>
    Task<Command> CreateCommandAsync(Command command);

    /// <summary>
    /// Retrieves a command by its unique identifier.
    /// </summary>
    /// <param name="commandId">The unique identifier of the command.</param>
    /// <returns>The command if found; otherwise, <c>null</c>.</returns>
    Task<Command?> GetCommandAsync(string commandId);

    /// <summary>
    /// Retrieves all commands that are currently in a pending state.
    /// </summary>
    /// <returns>A collection of pending commands.</returns>
    Task<IEnumerable<Command>> GetPendingCommandsAsync();

    /// <summary>
    /// Retrieves the command history for a specific device, ordered by creation time descending.
    /// </summary>
    /// <param name="deviceId">The unique identifier of the device.</param>
    /// <returns>A collection of commands associated with the device.</returns>
    Task<IEnumerable<Command>> GetCommandHistoryAsync(string deviceId);

    /// <summary>
    /// Marks a command as successfully executed.
    /// </summary>
    /// <param name="commandId">The unique identifier of the command to execute.</param>
    /// <returns><c>true</c> if the command was successfully marked as executed.</returns>
    Task<bool> ExecuteCommandAsync(string commandId);

    /// <summary>
    /// Marks a command as failed and re-queues it if retries are available.
    /// </summary>
    /// <param name="commandId">The unique identifier of the command.</param>
    /// <returns><c>true</c> if the command was re-queued for retry; <c>false</c> if retry limit was reached.</returns>
    Task<bool> MarkCommandAsFailedAsync(string commandId);

    /// <summary>
    /// Finds expired commands and retries those that have remaining retry attempts.
    /// </summary>
    /// <returns>The number of commands that were successfully re-queued.</returns>
    Task<int> RetryFailedCommandsAsync();

    /// <summary>
    /// Deletes command records older than the specified date.
    /// </summary>
    /// <param name="olderThan">The cutoff date; commands created before this are removed.</param>
    /// <returns>The number of commands deleted.</returns>
    Task<int> CleanupOldCommandsAsync(DateTime olderThan);
}

/// <summary>
/// Manages the lifecycle of device commands - creation, execution, failure handling,
/// retry scheduling, and cleanup of stale records. Supports both unit-of-work and
/// legacy generic repository construction paths.
/// </summary>
public class CommandService : ICommandService
{
    private const string GenericRepositoriesRequiredMessage = "Service was not constructed with generic repositories.";

    private readonly ICommandRepository _repository = null!;
    private readonly IDeviceRepository _deviceRepository = null!;
    private readonly IRepository<Command>? _legacyCommandRepository;
    private readonly IRepository<Device>? _legacyDeviceRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandService"/> class using a unit of work.
    /// </summary>
    /// <param name="unitOfWork">The unit of work providing access to command and device repositories.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="unitOfWork"/> is <c>null</c>.</exception>
    public CommandService(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _repository = unitOfWork.Commands;
        _deviceRepository = unitOfWork.Devices;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandService"/> class using legacy generic repositories.
    /// </summary>
    /// <param name="commandRepository">The generic repository for command entities.</param>
    /// <param name="deviceRepository">The generic repository for device entities.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandRepository"/> or <paramref name="deviceRepository"/> is <c>null</c>.</exception>
    public CommandService(IRepository<Command> commandRepository, IRepository<Device> deviceRepository)
    {
        ArgumentNullException.ThrowIfNull(commandRepository);
        ArgumentNullException.ThrowIfNull(deviceRepository);

        _legacyCommandRepository = commandRepository;
        _legacyDeviceRepository = deviceRepository;
    }

    /// <summary>
    /// Sends a command to a device using the legacy generic repositories.
    /// Returns <c>null</c> if the target device does not exist.
    /// </summary>
    /// <param name="deviceId">The unique identifier of the target device.</param>
    /// <param name="commandType">The type of command to send.</param>
    /// <param name="payload">The command payload data.</param>
    /// <returns>The created command, or <c>null</c> if the device was not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="deviceId"/>, <paramref name="commandType"/>, or <paramref name="payload"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the service was not constructed with generic repositories.</exception>
    public async Task<Command?> SendCommandAsync(string deviceId, string commandType, string payload)
    {
        ArgumentNullException.ThrowIfNull(deviceId);
        ArgumentNullException.ThrowIfNull(commandType);
        ArgumentNullException.ThrowIfNull(payload);

        if (_legacyCommandRepository is null || _legacyDeviceRepository is null)
            throw new InvalidOperationException(GenericRepositoriesRequiredMessage);

        var device = await _legacyDeviceRepository.GetByIdAsync(deviceId).ConfigureAwait(false);
        if (device is null)
            return null;

        var command = new Command
        {
            DeviceId = deviceId,
            CommandType = commandType,
            Payload = payload,
            SentTime = DateTime.UtcNow,
            IsSent = true
        };

        await _legacyCommandRepository.AddAsync(command).ConfigureAwait(false);
        return command;
    }

    /// <summary>
    /// Retrieves all commands sent to a specific device using the legacy generic repository.
    /// </summary>
    /// <param name="deviceId">The unique identifier of the device.</param>
    /// <returns>A collection of commands associated with the device.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="deviceId"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the service was not constructed with generic repositories.</exception>
    public async Task<IEnumerable<Command>> GetCommandsForDeviceAsync(string deviceId)
    {
        ArgumentNullException.ThrowIfNull(deviceId);

        if (_legacyCommandRepository is null)
            throw new InvalidOperationException(GenericRepositoriesRequiredMessage);

        return await _legacyCommandRepository.FindManyAsync(c => c.DeviceId == deviceId).ConfigureAwait(false);
    }

    /// <summary>
    /// Marks a command as acknowledged by the device using the legacy generic repository.
    /// If the command is not found, the method returns without error.
    /// </summary>
    /// <param name="commandId">The unique identifier of the command to acknowledge.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandId"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the service was not constructed with generic repositories.</exception>
    public async Task AcknowledgeCommandAsync(string commandId)
    {
        ArgumentNullException.ThrowIfNull(commandId);

        if (_legacyCommandRepository is null)
            throw new InvalidOperationException(GenericRepositoriesRequiredMessage);

        var command = await _legacyCommandRepository.GetByIdAsync(commandId).ConfigureAwait(false);
        if (command is null)
            return;

        command.IsAcknowledged = true;
        command.AcknowledgedTime = DateTime.UtcNow;
        await _legacyCommandRepository.UpdateAsync(command).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new command for a device after validating the command and verifying the target device exists and is active.
    /// Assigns a new ID, timestamp, and pending status to the command.
    /// </summary>
    /// <param name="command">The command to create.</param>
    /// <returns>The persisted command with assigned ID and metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is <c>null</c>.</exception>
    /// <exception cref="ValidationException">Thrown when the command fails validation.</exception>
    /// <exception cref="DeviceException">Thrown when the target device is not found or is inactive.</exception>
    public async Task<Command> CreateCommandAsync(Command command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!command.IsValid())
            throw new ValidationException("Command validation failed", nameof(command));

        var device = await _deviceRepository.GetByIdAsync(command.DeviceId).ConfigureAwait(false);
        if (device is null)
            throw new DeviceException($"Device {command.DeviceId} not found", command.DeviceId);

        if (!device.IsActive)
            throw new DeviceException($"Device {command.DeviceId} is not active", command.DeviceId);

        command.Id ??= Guid.NewGuid().ToString();
        command.CreatedAt = DateTime.UtcNow;
        command.Status = CommandStatus.Pending;

        return await _repository.CreateAsync(command).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves a command by its unique identifier.
    /// </summary>
    /// <param name="commandId">The unique identifier of the command.</param>
    /// <returns>The command if found; otherwise, <c>null</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandId"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="commandId"/> is whitespace.</exception>
    public async Task<Command?> GetCommandAsync(string commandId)
    {
        ArgumentNullException.ThrowIfNull(commandId);

        if (string.IsNullOrWhiteSpace(commandId))
            throw new ArgumentException("Command ID cannot be empty", nameof(commandId));

        return await _repository.GetByIdAsync(commandId).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves all commands that are currently in a pending state.
    /// </summary>
    /// <returns>A collection of pending commands.</returns>
    public async Task<IEnumerable<Command>> GetPendingCommandsAsync()
    {
        return await _repository.GetPendingAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves the full command history for a specific device, ordered by creation time descending.
    /// </summary>
    /// <param name="deviceId">The unique identifier of the device.</param>
    /// <returns>A collection of commands ordered from newest to oldest.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="deviceId"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="deviceId"/> is whitespace.</exception>
    public async Task<IEnumerable<Command>> GetCommandHistoryAsync(string deviceId)
    {
        ArgumentNullException.ThrowIfNull(deviceId);

        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        var commands = await _repository.GetByDeviceIdAsync(deviceId).ConfigureAwait(false);
        return commands.OrderByDescending(c => c.CreatedAt).ToList();
    }

    /// <summary>
    /// Marks a command as successfully executed by invoking the command's execution logic and persisting the update.
    /// </summary>
    /// <param name="commandId">The unique identifier of the command to execute.</param>
    /// <returns><c>true</c> if the command was successfully marked as executed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandId"/> is <c>null</c>.</exception>
    /// <exception cref="CommandException">Thrown when the command is not found.</exception>
    public async Task<bool> ExecuteCommandAsync(string commandId)
    {
        ArgumentNullException.ThrowIfNull(commandId);

        var command = await _repository.GetByIdAsync(commandId).ConfigureAwait(false);
        if (command is null)
            throw new CommandException($"Command {commandId} not found", commandId);

        command.Execute();
        await _repository.UpdateAsync(command).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Marks a command as failed and re-queues it for retry if the retry limit has not been reached.
    /// </summary>
    /// <param name="commandId">The unique identifier of the command.</param>
    /// <returns><c>true</c> if the command was re-queued for retry; <c>false</c> if no retries remain.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandId"/> is <c>null</c>.</exception>
    /// <exception cref="CommandException">Thrown when the command is not found.</exception>
    public async Task<bool> MarkCommandAsFailedAsync(string commandId)
    {
        ArgumentNullException.ThrowIfNull(commandId);

        var command = await _repository.GetByIdAsync(commandId).ConfigureAwait(false);
        if (command is null)
            throw new CommandException($"Command {commandId} not found", commandId);

        if (!command.CanRetry())
        {
            await _repository.UpdateAsync(command).ConfigureAwait(false);
            return false;
        }

        command.Status = CommandStatus.Pending;
        await _repository.UpdateAsync(command).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Finds commands that have exceeded the execution timeout and re-queues those with remaining retry attempts.
    /// </summary>
    /// <returns>The number of commands that were successfully re-queued for retry.</returns>
    public async Task<int> RetryFailedCommandsAsync()
    {
        var expiredCommands = await _repository.GetExpiredAsync(TimeSpan.FromSeconds(ConfigConstants.COMMAND_EXECUTION_TIMEOUT_SECONDS)).ConfigureAwait(false);
        var retryCount = 0;

        foreach (var cmd in expiredCommands)
        {
            if (await MarkCommandAsFailedAsync(cmd.Id))
                retryCount++;
        }

        return retryCount;
    }

    /// <summary>
    /// Deletes command records that were created before the specified cutoff date.
    /// </summary>
    /// <param name="olderThan">The cutoff date; commands created before this are deleted.</param>
    /// <returns>The number of commands deleted.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="olderThan"/> is not in the past.</exception>
    public async Task<int> CleanupOldCommandsAsync(DateTime olderThan)
    {
        if (olderThan >= DateTime.UtcNow)
            throw new ArgumentException("Cleanup date must be in the past");

        return await _repository.DeleteOlderThanAsync(olderThan).ConfigureAwait(false);
    }
}
