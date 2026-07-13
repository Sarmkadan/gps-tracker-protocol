#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GpsTrackerProtocol.Utilities;
using System.Text;
using Xunit;

public class ByteExtensionsJsonExtensionsTests
{
    [Fact]
    public void ToJson_ValidInput_ReturnsCorrectOutput()
    {
        // Arrange
        byte[] input = Encoding.UTF8.GetBytes("{\"key\":\"value\"}");

        // Act
        string result = ByteExtensionsJsonExtensions.ToJson(input);

        // Assert
        result.Should().Be("{\"key\":\"value\"}");
    }

    [Fact]
    public void FromJson_ValidInput_ReturnsCorrectOutput()
    {
        // Arrange
        string input = "{\"key\":\"value\"}";

        // Act
        byte[]? result = ByteExtensionsJsonExtensions.FromJson(input);

        // Assert
        result.Should().NotBeNull();
    }
}
