#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GpsTrackerProtocol.Utilities;
using System.Collections.Generic;
using Xunit;

public class DictionaryExtensionsJsonExtensionsTests
{
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
