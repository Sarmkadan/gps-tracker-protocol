# GPS Tracker Protocol v2.0 Basic Usage Example

This example demonstrates how to use the GPS Tracker Protocol v2.0 with the new fleet analytics dashboard features.

## Prerequisites
- .NET 8.0 SDK
- GPS Tracker Protocol library

## Running the Example

1. Navigate to the example directory:
```bash
cd examples/v2-basic-usage
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build and run:
```bash
dotnet run
```

## Code Overview

The example shows how to:

1. Configure the GPS Tracker Protocol with the new v2.0 features
2. Set up the fleet dashboard with route optimization and fuel tracking
3. Integrate with the hosted services pattern for background processing
4. Use the new v2.0 services for fleet analytics

## Key Features Demonstrated

- Basic GPS tracker protocol setup
- Fleet dashboard configuration
- Route optimization activation
- Fuel tracking activation
- Analytics service integration

## Customization

You can modify the configuration options in the Program.cs file to adjust:
- Default protocol type
- Maximum number of devices
- Cache expiration
- Analytics update intervals