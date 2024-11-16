using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpsTrackerProtocol.Tests;
using GpsTrackerProtocol.Services;
using GpsTrackerProtocol.Domain.Models;
using GpsTrackerProtocol.Domain;

namespace GpsTrackerProtocol.Tests
{
    /// <summary>
    /// Extension methods for <see cref="FuelTrackingServiceTests"/> that provide convenient test setup and assertions.
    /// </summary>
    public static class FuelTrackingServiceTestsExtensions
    {
        /// <summary>
        /// Creates a new <see cref="FuelTrackingServiceTests"/> instance with default configuration.
        /// </summary>
        /// <param name="_">The test instance (unused, required for extension method syntax).</param>
        /// <returns>A new <see cref="FuelTrackingServiceTests"/> instance.</returns>
        public static FuelTrackingServiceTests CreateDefault(this FuelTrackingServiceTests _) => new();

        /// <summary>
        /// Creates a new <see cref="FuelTrackingServiceTests"/> instance seeded with the specified fuel records.
        /// </summary>
        /// <param name="_">The test instance (unused, required for extension method syntax).</param>
        /// <param name="records">Initial set of fuel records to seed the test service with.</param>
        /// <returns>A new <see cref="FuelTrackingServiceTests"/> instance containing the specified records.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="records"/> is null.</exception>
        public static async Task<FuelTrackingServiceTests> WithRecordsAsync(this FuelTrackingServiceTests _, IEnumerable<FuelRecord> records)
        {
            ArgumentNullException.ThrowIfNull(records);

            var instance = new FuelTrackingServiceTests();
            var service = instance.GetService();

            foreach (var record in records)
            {
                await service.RecordFuelEventAsync(record);
            }

            return instance;
        }

        /// <summary>
        /// Creates a new <see cref="FuelTrackingServiceTests"/> instance with records for the specified vehicle and date range.
        /// </summary>
        /// <param name="_">The test instance (unused, required for extension method syntax).</param>
        /// <param name="vehicleId">The vehicle ID to create records for.</param>
        /// <param name="startDate">Start date for the records.</param>
        /// <param name="endDate">End date for the records.</param>
        /// <param name="recordCount">Number of records to create.</param>
        /// <returns>A new <see cref="FuelTrackingServiceTests"/> instance containing generated records.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="vehicleId"/> is null or whitespace.</exception>
        public static async Task<FuelTrackingServiceTests> WithVehicleRecordsAsync(
            this FuelTrackingServiceTests _,
            string vehicleId,
            DateTime startDate,
            DateTime endDate,
            int recordCount = 5)
        {
            ArgumentException.ThrowIfNullOrEmpty(vehicleId);

            if (endDate <= startDate)
            {
                throw new ArgumentException("End date must be after start date", nameof(endDate));
            }

            var instance = new FuelTrackingServiceTests();
            var service = instance.GetService();

            var random = new Random();
            var dateRange = endDate - startDate;

            for (int i = 0; i < recordCount; i++)
            {
                var recordDate = startDate.AddMinutes(random.Next(0, (int)dateRange.TotalMinutes));
                var fuelAmount = 10.0 + random.NextDouble() * 40.0; // 10-50 liters
                var eventType = random.Next(0, 2) == 0 ? FuelEventType.Refuel : FuelEventType.Consumption;

                await service.RecordFuelEventAsync(new FuelRecord(
                    vehicleId,
                    "testDevice",
                    eventType,
                    fuelAmount,
                    recordDate,
                    OdometerKm: 1000 + i * 100
                ));
            }

            return instance;
        }

        /// <summary>
        /// Asserts that a specific fuel record exists in the test service.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <param name="recordId">The record ID to check for.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="recordId"/> is null or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is null.</exception>
        public static async Task AssertRecordExistsAsync(this FuelTrackingServiceTests test, string recordId)
        {
            ArgumentNullException.ThrowIfNull(test);
            ArgumentException.ThrowIfNullOrEmpty(recordId);

            var service = test.GetService();
            var records = await service.GetRecordsAsync("testVehicle", null);

            if (!records.Any(r => r.Id == recordId))
            {
                throw new Exception($"Expected record with ID {recordId} to exist but it was not found.");
            }
        }

        /// <summary>
        /// Asserts that a specific fuel record does not exist in the test service.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <param name="recordId">The record ID to check for absence.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="recordId"/> is null or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is null.</exception>
        public static async Task AssertRecordDoesNotExistAsync(this FuelTrackingServiceTests test, string recordId)
        {
            ArgumentNullException.ThrowIfNull(test);
            ArgumentException.ThrowIfNullOrEmpty(recordId);

            var service = test.GetService();
            var records = await service.GetRecordsAsync("testVehicle", null);

            if (records.Any(r => r.Id == recordId))
            {
                throw new Exception($"Expected record with ID {recordId} to not exist but it was found.");
            }
        }

        /// <summary>
        /// Gets the <see cref="FuelTrackingService"/> instance from the test.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <returns>The <see cref="FuelTrackingService"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is null.</exception>
        private static FuelTrackingService GetService(this FuelTrackingServiceTests test)
        {
            ArgumentNullException.ThrowIfNull(test);

            var serviceField = typeof(FuelTrackingServiceTests).GetField(
                "_service",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (serviceField == null)
            {
                throw new InvalidOperationException("Could not find _service field in FuelTrackingServiceTests");
            }

            var service = serviceField.GetValue(test) as FuelTrackingService;
            if (service == null)
            {
                throw new InvalidOperationException("Could not cast _service to FuelTrackingService");
            }

            return service;
        }
    }
}