# ErrorHandlingMiddleware

The `ErrorHandlingMiddleware` is a component within the ASP.NET Core request pipeline designed to intercept unhandled exceptions, transform them into standardized `ErrorResponse` objects, and ensure consistent error logging and reporting across the application. By capturing exceptions at the middleware level, it provides a centralized mechanism to handle errors gracefully, ensuring that clients receive predictable and informative error information while maintaining the security of the internal application state.

## API

*   **`ErrorHandlingMiddleware(...)`**
    Initializes a new instance of the `ErrorHandlingMiddleware` class, typically within the context of the ASP.NET Core dependency injection container.
*   **`async Task InvokeAsync(HttpContext context)`**
    Executes the middleware logic. It invokes the next delegate in the request pipeline and catches any unhandled exceptions, subsequently processing them to generate an appropriate error response.
*   **`ErrorResponse CreateErrorResponse(Exception exception, HttpContext context)`**
    Generates an `ErrorResponse` object based on the provided exception and the current HTTP context, facilitating consistent error structure mapping.
*   **`string ErrorCode`**
    Gets or sets the error code identifier associated with the current or last handled error.
*   **`string Message`**
    Gets or sets the descriptive error message associated with the current or last handled error.
*   **`string CorrelationId`**
    Gets or sets the unique identifier used to trace the current or last handled error across distributed system components.
*   **`DateTime Timestamp`**
    Gets or sets the date and time when the current or last handled error occurred.
*   **`string StackTrace`**
    Gets or sets the stack trace information associated with the current or last handled error, providing debugging details.

## Usage

**Example 1: Registering the middleware in `Program.cs`**

```csharp
using gps_tracker_protocol.Middleware;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Register the error handling middleware early in the pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();
app.Run();
```

**Example 2: Manually invoking the response creation (typically used in custom error handling scenarios)**

```csharp
public async Task HandleCustomError(HttpContext context, Exception ex)
{
    var middleware = new ErrorHandlingMiddleware();
    var errorResponse = middleware.CreateErrorResponse(ex, context);
    
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(errorResponse);
}
```

## Notes

*   **Thread Safety:** Middleware in ASP.NET Core is typically registered as a singleton. Because `ErrorCode`, `Message`, `CorrelationId`, `Timestamp`, and `StackTrace` are public properties of the middleware class itself, they are not inherently thread-safe if modified during the processing of concurrent requests. Avoid relying on these properties to store request-specific state; they should only be used in a manner that does not conflict with concurrent request processing, or the middleware should be designed to handle request-scoped context rather than class-level state.
*   **Sensitive Data:** The `StackTrace` property may contain sensitive information about the application's internal structure. Ensure that this property is only exposed in non-production environments or that sensitive information is filtered out before the error response is serialized and returned to the client.
