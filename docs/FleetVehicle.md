# FleetVehicle
The `FleetVehicle` type represents a vehicle in a fleet, providing essential information about the vehicle, its fuel consumption, and registration details. It is designed to be used in the context of a GPS tracking system, allowing for efficient management and monitoring of fleet vehicles.

## API
The `FleetVehicle` type has the following public members:
* `Id`: A required string identifier for the vehicle.
* `DeviceId`: A required string identifier for the device associated with the vehicle.
* `RegistrationNumber`: A required string representing the vehicle's registration number.
* `Make`: A required string representing the vehicle's make.
* `Model`: A required string representing the vehicle's model.
* `Year`: An integer representing the vehicle's year of manufacture.
* `FuelType`: An enumeration of type `FuelType` representing the vehicle's fuel type.
* `TankCapacityLiters`: A double representing the vehicle's tank capacity in liters.
* `BaseConsumptionLper100km`: A double representing the vehicle's base fuel consumption per 100 kilometers.
* `RegisteredAt`: A `DateTime` object representing the date and time the vehicle was registered.
* `Metadata`: A dictionary of strings to objects providing additional metadata about the vehicle.
* `FuelRecord`: Two instances of `FuelRecord` are present, but their purpose is unclear without further context.

## Usage
Here are two examples of using the `FleetVehicle` type in C#:
```csharp
// Example 1: Creating a new FleetVehicle instance
FleetVehicle vehicle = new FleetVehicle
{
    Id = "VEH-001",
    DeviceId = "DEV-001",
    RegistrationNumber = "ABC123",
    Make = "Toyota",
    Model = "Camry",
    Year = 2020,
    FuelType = FuelType.Gasoline,
    TankCapacityLiters = 60.0,
    BaseConsumptionLper100km = 8.5,
    RegisteredAt = DateTime.Now,
    Metadata = new Dictionary<string, object>
    {
        { "Color", "Silver" },
        { "Transmission", "Automatic" }
    }
};

// Example 2: Updating a FleetVehicle instance
vehicle.Metadata["Mileage"] = 50000;
vehicle.BaseConsumptionLper100km = 9.0;
```

## Notes
When working with the `FleetVehicle` type, consider the following edge cases and thread-safety remarks:
* The `Id`, `DeviceId`, `RegistrationNumber`, `Make`, and `Model` properties are required, so attempting to create a `FleetVehicle` instance without these properties will result in an error.
* The `FuelRecord` instances are not clearly defined, so their usage and behavior may vary depending on the specific implementation.
* The `Metadata` dictionary can store arbitrary key-value pairs, but it is not thread-safe by default. If multiple threads need to access or modify the `Metadata` dictionary, consider using a thread-safe implementation, such as `ConcurrentDictionary<string, object>`.
* The `RegisteredAt` property is a `DateTime` object, which can be affected by the system's clock and timezone settings. When working with dates and times, consider using a consistent timezone and clock source to avoid inconsistencies.
