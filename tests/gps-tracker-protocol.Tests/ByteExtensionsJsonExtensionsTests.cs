#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GpsTrackerProtocol.Utilities;
using System.Text;
using Xunit;

/// <summary>
/// Provides unit tests for the <see cref="ByteExtensionsJsonExtensions"/> class.
/// </summary>
public class ByteExtensionsJsonExtensionsTests
{
	/// <summary>
	/// Tests that the <see cref="ByteExtensionsJsonExtensions.ToJson(byte[])"/> method correctly converts a UTF-8 encoded JSON byte array to its string representation.
	/// </summary>
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

	/// <summary>
	/// Tests that the <see cref="ByteExtensionsJsonExtensions.FromJson(string)"/> method correctly converts a JSON string to a UTF-8 encoded byte array.
	/// </summary>
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