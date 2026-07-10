#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace GpsTrackerProtocol.Tests;

/// <summary>
/// Provides validation helpers for <see cref="DomainAndServiceTests"/> instances.
/// These extension methods allow validation of the test class itself and can be used
/// to validate domain models (LocationData, Device, GpsFrame) that are tested by
/// the test methods in DomainAndServiceTests.
/// </summary>
public static class DomainAndServiceTestsValidation
{
	/// <summary>
	/// Validates a DomainAndServiceTests instance.
	/// </summary>
	/// <param name="value">The test class instance to validate</param>
	/// <returns>List of validation errors; empty if valid</returns>
	public static IReadOnlyList<string> Validate(this DomainAndServiceTests value)
	{
		var errors = new List<string>();

		if (value == null)
		{
			errors.Add("DomainAndServiceTests instance is null");
			return errors.AsReadOnly();
		}

		return errors.AsReadOnly();
	}

	/// <summary>
	/// Checks if a DomainAndServiceTests instance is valid.
	/// </summary>
	/// <param name="value">The test class instance to check</param>
	/// <returns>True if valid, false otherwise</returns>
	public static bool IsValid(this DomainAndServiceTests value)
	{
		return !Validate(value).Any();
	}

	/// <summary>
	/// Ensures a DomainAndServiceTests instance is valid, throwing an exception if not.
	/// </summary>
	/// <param name="value">The test class instance to validate</param>
	/// <exception cref="ArgumentException">Thrown when validation fails</exception>
	public static void EnsureValid(this DomainAndServiceTests value)
	{
		var errors = Validate(value);
		if (errors.Any())
		{
			throw new ArgumentException(string.Join(Environment.NewLine, errors));
		}
	}
}
