# CommandServiceTests

`CommandServiceTests` provides a comprehensive suite of unit tests for verifying the behavioral correctness and robustness of the `CommandService` within the `gps-tracker-protocol` project. It ensures that critical communication workflows—including command dispatch, retrieval for specific devices, and state management during acknowledgment—operate according to specifications, while also validating error handling for scenarios such as non-existent devices or missing commands.

## API

*   **`SendCommandAsync_ShouldAddCommandAndMarkAsSent`**
    Verifies that a command is successfully persisted and marked as "sent" when dispatched to a valid device. It tests the happy-path integration between the service and its underlying data repository.

*   **`SendCommandAsync_ShouldReturnNull_WhenDeviceNotFound`**
    Tests that the service returns a `null` value when an attempt is made to send a command to a device identifier that does not exist in the system, confirming correct handling of invalid device references.

*   **`GetCommandsForDeviceAsync_ShouldReturnCommands`**
    Verifies that the service successfully retrieves a collection of pending commands when queried for a valid device that has commands associated with it.

*   **`GetCommandsForDeviceAsync_ShouldReturnEmptyList_WhenNoCommandsForDevice`**
    Confirms that querying a valid device with no pending commands returns an empty collection rather than `null` or an exception.

*   **`AcknowledgeCommandAsync_ShouldMarkCommandAsAcknowledged`**
    Tests the successful state transition of an existing, pending command to an "acknowledged" status, ensuring the acknowledgment signal is processed correctly.

*   **`AcknowledgeCommandAsync_ShouldDoNothing_WhenCommandNotFound`**
    Ensures that the service handles attempts to acknowledge non-existent commands gracefully without throwing exceptions, confirming the method performs a no-op in this scenario.

## Usage

### Running Tests via Command Line
To execute this test suite using the .NET CLI, run the following command in the project root:

```bash
dotnet test --filter FullyQualifiedName~CommandServiceTests
```

### Typical Test Method Implementation Pattern
These tests are structured to utilize standard mocking frameworks to isolate the `CommandService` from external dependencies (e.g., database context, messaging queues). A typical test case follows the Arrange-Act-Assert pattern:

```csharp
[Fact]
public async Task SendCommandAsync_ShouldAddCommandAndMarkAsSent_Example()
{
    // Arrange
    var mockRepo = new Mock<ICommandRepository>();
    var service = new CommandService(mockRepo.Object);
    var command = new CommandRequest { DeviceId = "dev-123", Payload = "ping" };

    // Act
    var result = await service.SendCommandAsync(command);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(CommandStatus.Sent, result.Status);
    mockRepo.Verify(r => r.SaveAsync(It.IsAny<Command>()), Times.Once);
}
```

## Notes

*   **Test Isolation:** These tests are designed to be executed independently. They should not rely on shared state between test cases to ensure consistent, reproducible results.
*   **Asynchronous Execution:** All methods return `Task`, reflecting the asynchronous nature of the underlying `CommandService` operations. Tests must be awaited to ensure assertions are evaluated after the asynchronous operations have completed.
*   **Dependency Mocking:** The suite relies heavily on mocking infrastructure for repositories and external services. If the interfaces of those dependencies change, the corresponding mocks within these tests must be updated to avoid breaking the suite.
*   **Thread Safety:** While the `CommandService` implementation itself must be thread-safe, the test class methods are typically executed by a test runner which isolates each test execution; therefore, thread-safety is generally not a concern for the test methods themselves, provided that they do not share static mutable state.
