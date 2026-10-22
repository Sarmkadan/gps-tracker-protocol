#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;

namespace GpsTrackerProtocol.Configuration;

/// <summary>
/// Validates <see cref="GpsListenerOptions"/>. Every violated rule is reported,
/// not just the first, so a misconfigured deployment can be fixed in one pass.
/// </summary>
public sealed class GpsListenerOptionsValidator : IValidateOptions<GpsListenerOptions>
{
    private const int MIN_PORT = 1;
    private const int MAX_PORT = 65535;
    private const int MIN_RECEIVE_BUFFER_SIZE = 64;
    private const int MAX_RECEIVE_BUFFER_SIZE = 65536;

    // TimeSpan.CancelAfter rejects anything above int.MaxValue milliseconds.
    private static readonly TimeSpan MAX_IDLE_TIMEOUT = TimeSpan.FromMilliseconds(int.MaxValue);

    /// <summary>
    /// Validates the specified listener options.
    /// </summary>
    /// <param name="name">The options instance name, or null for the default instance.</param>
    /// <param name="options">The options to validate.</param>
    /// <returns>
    /// <see cref="ValidateOptionsResult.Success"/> when every rule passes; otherwise a failure
    /// listing each violated rule.
    /// </returns>
    public ValidateOptionsResult Validate(string? name, GpsListenerOptions? options)
    {
        if (options is null)
        {
            return ValidateOptionsResult.Fail("GpsListenerOptions instance is null.");
        }

        var failures = new List<string>();

        if (options.TcpPort < MIN_PORT || options.TcpPort > MAX_PORT)
        {
            failures.Add($"{nameof(options.TcpPort)} must be between {MIN_PORT} and {MAX_PORT} (actual: {options.TcpPort}).");
        }

        if (options.UdpPort < MIN_PORT || options.UdpPort > MAX_PORT)
        {
            failures.Add($"{nameof(options.UdpPort)} must be between {MIN_PORT} and {MAX_PORT} (actual: {options.UdpPort}).");
        }

        if (options.MaxConnections <= 0)
        {
            failures.Add($"{nameof(options.MaxConnections)} must be greater than 0 (actual: {options.MaxConnections}).");
        }

        if (options.ReceiveBufferSize < MIN_RECEIVE_BUFFER_SIZE || options.ReceiveBufferSize > MAX_RECEIVE_BUFFER_SIZE)
        {
            failures.Add($"{nameof(options.ReceiveBufferSize)} must be between {MIN_RECEIVE_BUFFER_SIZE} and {MAX_RECEIVE_BUFFER_SIZE} bytes (actual: {options.ReceiveBufferSize}).");
        }

        if (options.IdleTimeout <= TimeSpan.Zero || options.IdleTimeout > MAX_IDLE_TIMEOUT)
        {
            failures.Add($"{nameof(options.IdleTimeout)} must be greater than zero and at most {MAX_IDLE_TIMEOUT} (actual: {options.IdleTimeout}).");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
