#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GpsTrackerProtocol.Utilities;
using System.Collections.Generic;
using Xunit;

/// <summary>
/// Provides unit tests for <see cref="DictionaryExtensionsJsonExtensions"/> methods that convert between dictionaries and JSON strings.
/// </summary>
public class DictionaryExtensionsJsonExtensionsTests
{
	/// <summary>
	/// Tests that <see cref="DictionaryExtensionsJsonExtensions.ToJson"/> correctly converts a dictionary to a JSON string.
	/// </summary>
    [Fact]
    public void ToJson_ValidInput_ReturnsCorrectOutput()
    {
        // Arrange
        var input = new Dictionary<string, object> { { "key", "value" } };

        // Act
        string result = DictionaryExtensionsJsonExtensions.ToJson(input);

        // Assert
        result.Should().Be("{\"key\":\"value\"}");
    }

	/// <summary>
	/// Tests that <see cref="DictionaryExtensionsJsonExtensions.FromJson"/> correctly converts a JSON string back to a dictionary.
	/// </summary>
    [Fact]
    public void FromJson_ValidInput_ReturnsCorrectOutput()
    {
        // Arrange
        string input = "{\"key\":\"value\"}";

        // Act
        Dictionary<string, object>? result = DictionaryExtensionsJsonExtensions.FromJson(input);

        // Assert
        result.Should().NotBeNull();
    }
}
