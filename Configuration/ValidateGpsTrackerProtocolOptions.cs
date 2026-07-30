using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using GpsTrackerProtocol.Configuration;

namespace GpsTrackerProtocol.Configuration;

/// <summary>
/// Validates <see cref="GpsTrackerProtocolOptions"/> using the same constraints
/// expressed by the data‑annotation attributes on the options class.
/// This validator is registered via <c>services.AddOptions&lt;GpsTrackerProtocolOptions&gt;()
/// .ValidateOptions&lt;GpsTrackerProtocolOptionsValidator&gt;()</c> or similar in the DI
/// configuration (outside of this repository scope).
/// </summary>
public sealed class GpsTrackerProtocolOptionsValidator : IValidateOptions<GpsTrackerProtocolOptions>
{
    public ValidateOptionsResult Validate(string? name, GpsTrackerProtocolOptions? options)
    {
        if (options is null)
        {
            return ValidateOptionsResult.Fail("GpsTrackerProtocolOptions instance is null.");
        }

        var failures = new List<string>();

        // DefaultProtocol – required, non‑empty
        if (string.IsNullOrWhiteSpace(options.DefaultProtocol))
        {
            failures.Add($"{nameof(options.DefaultProtocol)} must be a non‑empty string.");
        }

        // MaxDevices – [Range(1,10000)]
        if (options.MaxDevices < 1 || options.MaxDevices > 10000)
        {
            failures.Add($"{nameof(options.MaxDevices)} must be between 1 and 10000 (actual: {options.MaxDevices}).");
        }

        // LocationHistoryLimit – [Range(1,1000)]
        if (options.LocationHistoryLimit < 1 || options.LocationHistoryLimit > 1000)
        {
            failures.Add($"{nameof(options.LocationHistoryLimit)} must be between 1 and 1000 (actual: {options.LocationHistoryLimit}).");
        }

        // CacheExpirationMinutes – [Range(1,60)]
        if (options.CacheExpirationMinutes < 1 || options.CacheExpirationMinutes > 60)
        {
            failures.Add($"{nameof(options.CacheExpirationMinutes)} must be between 1 and 60 (actual: {options.CacheExpirationMinutes}).");
        }

        // RateLimitPerMinute – [Range(1,1000)]
        if (options.RateLimitPerMinute < 1 || options.RateLimitPerMinute > 1000)
        {
            failures.Add($"{nameof(options.RateLimitPerMinute)} must be between 1 and 1000 (actual: {options.RateLimitPerMinute}).");
        }

        // LoggingLevel – required, non‑empty
        if (string.IsNullOrWhiteSpace(options.LoggingLevel))
        {
            failures.Add($"{nameof(options.LoggingLevel)} must be a non‑empty string.");
        }

        // Protocol settings – each protocol has its own constraints
        var p = options.Protocol;

        // GT06
        if (p.GT06MaxFrameSize <= 0)
        {
            failures.Add($"{nameof(p.GT06MaxFrameSize)} must be greater than 0 (actual: {p.GT06MaxFrameSize}).");
        }
        if (p.GT06Timeout < 0)
        {
            failures.Add($"{nameof(p.GT06Timeout)} cannot be negative (actual: {p.GT06Timeout}).");
        }

        // H02
        if (p.H02MaxFrameSize <= 0)
        {
            failures.Add($"{nameof(p.H02MaxFrameSize)} must be greater than 0 (actual: {p.H02MaxFrameSize}).");
        }
        if (p.H02Timeout < 0)
        {
            failures.Add($"{nameof(p.H02Timeout)} cannot be negative (actual: {p.H02Timeout}).");
        }

        // TK103
        if (p.TK103MaxFrameSize <= 0)
        {
            failures.Add($"{nameof(p.TK103MaxFrameSize)} must be greater than 0 (actual: {p.TK103MaxFrameSize}).");
        }
        if (p.TK103Timeout < 0)
        {
            failures.Add($"{nameof(p.TK103Timeout)} cannot be negative (actual: {p.TK103Timeout}).");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
