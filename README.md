# GpsTracker Protocol

<!-- (rest of README.md remains the same) -->

## IValidator

The `IValidator` interface defines a contract for validating domain models like `GpsFrame`, `LocationData`, and `Device`. It works with the `ValidationPipeline` class to perform type-specific validation and collect errors. The pipeline supports validating GPS frames, location data, and device records, returning structured validation results.

Example usage:

```csharp
using GpsTrackerProtocol.Infrastructure;
using GpsTrackerProtocol.Domain.Models;

public class ValidationExample
{
  public void Demo()
  {
    // Create a validation pipeline with concrete validators
    var pipeline = new ValidationPipeline(
      new FrameValidator(),
      new LocationValidator(),
      new DeviceValidator());

    // Validate a GPS frame
    var frame = new GpsFrame
    {
      FrameId = "frame123",
      RawData = new byte[] { 0x01, 0x02, 0x03 },
      Protocol = ProtocolType.GPRMC,
      ReceivedAt = DateTime.UtcNow
    };

    var frameResult = pipeline.ValidateFrame(frame);
    if (!frameResult.IsValid)
    {
      Console.WriteLine("Frame validation failed: " + frameResult.GetErrorMessage());
    }

    // Validate a location record
    var location = new LocationData
    {
      DeviceId = "device456",
      Latitude = 37.7749,
      Longitude = -122.4194,
      Speed = 50.0,
      Bearing = 90.0,
      SatelliteCount = 8,
      Timestamp = DateTime.UtcNow
    };

    var locationResult = pipeline.ValidateLocation(location);
    if (!locationResult.IsValid)
    {
      Console.WriteLine("Location validation failed: " + locationResult.GetErrorMessage());
    }

    // Validate a device
    var device = new Device
    {
      Id = "device789",
      Imei = "123456789012345",
      Protocol = ProtocolType.GPRMC
    };

    var deviceResult = pipeline.ValidateDevice(device);
    if (!deviceResult.IsValid)
    {
      Console.WriteLine("Device validation failed: " + deviceResult.GetErrorMessage());
    }
  }
}
```

The `ValidationResult` class provides `IsValid` to check validation status and `Errors` to access detailed validation messages. The `GetErrorMessage()` method returns a semicolon-separated string of all validation errors.


## ErrorHandlingMiddleware

The `ErrorHandlingMiddleware` provides centralized exception handling for the application, catching and logging exceptions uniformly while mapping domain-specific exceptions to appropriate error responses. It intercepts exceptions during request processing, logs them with appropriate severity levels, and creates structured error responses containing error codes, messages, correlation IDs, timestamps, and stack traces for debugging.


Example usage:

```csharp
using GpsTrackerProtocol.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

public class ErrorHandlingMiddlewareExample
{
    private readonly IServiceProvider _serviceProvider;
    
    public ErrorHandlingMiddlewareExample(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task HandleRequestAsync(HttpContext context)
    {
        // Create middleware instance with logger
        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        var middleware = new ErrorHandlingMiddleware(loggerFactory.CreateLogger<ErrorHandlingMiddleware>());
        
        // Track correlation ID for error tracking
        var correlationId = Guid.NewGuid().ToString();
        context.Response.Headers["X-Correlation-ID"] = correlationId;
        
        try
        {
            // Process request through middleware
            await middleware.InvokeAsync(async () => 
            {
                // Simulate request processing that might throw
                await ProcessGpsDataAsync(context);
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Create structured error response
            var errorResponse = middleware.CreateErrorResponse(ex, correlationId);
            
            // Log error with context
            var logger = loggerFactory.CreateLogger<ErrorHandlingMiddlewareExample>();
            logger.LogError(ex, "Request processing failed: {ErrorCode}", errorResponse.ErrorCode);
            
            // Return error to client
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
    
    private async Task ProcessGpsDataAsync(HttpContext context)
    {
        // Business logic that might throw ParseException or ValidationException
        await Task.CompletedTask;
    }
}
```
