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
/// Service for managing device commands.
/// </summary>
public interface ICommandService
{
    Task<Command> CreateCommandAsync(Command command);
    Task<Command?> GetCommandAsync(string commandId);
    Task<IEnumerable<Command>> GetPendingCommandsAsync();
    Task<IEnumerable<Command>> GetCommandHistoryAsync(string deviceId);
    Task<bool> ExecuteCommandAsync(string commandId);
    Task<bool> MarkCommandAsFailedAsync(string commandId);
    Task<int> RetryFailedCommandsAsync();
    Task<int> CleanupOldCommandsAsync(DateTime olderThan);
}

/// <summary>
/// Implementation of command service.
/// </summary>
public class CommandService : ICommandService
{
    private readonly ICommandRepository _repository;
    private readonly IDeviceRepository _deviceRepository;

    public CommandService(IUnitOfWork unitOfWork)
    {
        _repository = unitOfWork.Commands;
        _deviceRepository = unitOfWork.Devices;
    }

    /// <summary>
    /// Creates a new command for a device.
    /// </summary>
    public async Task<Command> CreateCommandAsync(Command command)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (!command.IsValid())
            throw new ValidationException("Command validation failed", nameof(command));

        var device = await _deviceRepository.GetByIdAsync(command.DeviceId);
        if (device is null)
            throw new DeviceException($"Device {command.DeviceId} not found", command.DeviceId);

        if (!device.IsActive)
            throw new DeviceException($"Device {command.DeviceId} is not active", command.DeviceId);

        command.Id ??= Guid.NewGuid().ToString();
        command.CreatedAt = DateTime.UtcNow;
        command.Status = CommandStatus.Pending;

        return await _repository.CreateAsync(command);
    }

    /// <summary>
    /// Gets a command by ID.
    /// </summary>
    public async Task<Command?> GetCommandAsync(string commandId)
    {
        if (string.IsNullOrWhiteSpace(commandId))
            throw new ArgumentException("Command ID cannot be empty", nameof(commandId));

        return await _repository.GetByIdAsync(commandId);
    }

    /// <summary>
    /// Gets all pending commands.
    /// </summary>
    public async Task<IEnumerable<Command>> GetPendingCommandsAsync()
    {
        return await _repository.GetPendingAsync();
    }

    /// <summary>
    /// Gets command history for a device.
    /// </summary>
    public async Task<IEnumerable<Command>> GetCommandHistoryAsync(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID cannot be empty", nameof(deviceId));

        var commands = await _repository.GetByDeviceIdAsync(deviceId);
        return commands.OrderByDescending(c => c.CreatedAt).ToList();
    }

    /// <summary>
    /// Marks a command as executed.
    /// </summary>
    public async Task<bool> ExecuteCommandAsync(string commandId)
    {
        var command = await _repository.GetByIdAsync(commandId);
        if (command is null)
            throw new CommandException($"Command {commandId} not found", commandId);

        command.Execute();
        await _repository.UpdateAsync(command);
        return true;
    }

    /// <summary>
    /// Marks a command as failed and handles retry logic.
    /// </summary>
    public async Task<bool> MarkCommandAsFailedAsync(string commandId)
    {
        var command = await _repository.GetByIdAsync(commandId);
        if (command is null)
            throw new CommandException($"Command {commandId} not found", commandId);

        if (!command.CanRetry())
        {
            await _repository.UpdateAsync(command);
            return false;
        }

        command.Status = CommandStatus.Pending;
        await _repository.UpdateAsync(command);
        return true;
    }

    /// <summary>
    /// Retries expired commands.
    /// </summary>
    public async Task<int> RetryFailedCommandsAsync()
    {
        var expiredCommands = await _repository.GetExpiredAsync(TimeSpan.FromSeconds(ConfigConstants.COMMAND_EXECUTION_TIMEOUT_SECONDS));
        var retryCount = 0;

        foreach (var cmd in expiredCommands)
        {
            if (await MarkCommandAsFailedAsync(cmd.Id))
                retryCount++;
        }

        return retryCount;
    }

    /// <summary>
    /// Cleans up old command records.
    /// </summary>
    public async Task<int> CleanupOldCommandsAsync(DateTime olderThan)
    {
        if (olderThan >= DateTime.UtcNow)
            throw new ArgumentException("Cleanup date must be in the past");

        return await _repository.DeleteOlderThanAsync(olderThan);
    }
}
