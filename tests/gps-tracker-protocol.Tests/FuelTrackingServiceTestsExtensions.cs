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
    public static class FuelTrackingServiceTestsExtensions
    {
        /// <summary>
        /// Creates a new FuelTrackingServiceTests instance with default configuration.
        /// </summary>
        public static FuelTrackingServiceTests CreateDefault(this FuelTrackingServiceTests _) => new FuelTrackingServiceTests();

        /// <summary>
        /// Creates a new FuelTrackingServiceTests instance with a custom test data setup.
        /// </summary>
        /// <param name="records">Initial set of fuel records to seed the test service with.</param>
        public static FuelTrackingServiceTests WithRecords(this FuelTrackingServiceTests _, IEnumerable<FuelRecord> records)
        {
            var instance = new FuelTrackingServiceTests();
            // Use reflection to set up the internal service state with initial records
            var serviceField = typeof(FuelTrackingServiceTests).GetField(
                "_fuelTrackingService",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (serviceField != null)
            {
                var service = serviceField.GetValue(instance);
                var addMethod = service?.GetType().GetMethod("AddRecordsAsync");

                if (addMethod != null)
                {
                    addMethod.Invoke(service, new object[] { records });
                }
            }

            return instance;
        }

        /// <summary>
        /// Creates a new FuelTrackingServiceTests instance with a specific date range filter.
        /// </summary>
        /// <param name="startDate">Start date for filtering records.</param>
        /// <param name="endDate">End date for filtering records.</param>
        public static FuelTrackingServiceTests WithDateRange(this FuelTrackingServiceTests _, DateTime startDate, DateTime endDate)
        {
            var instance = new FuelTrackingServiceTests();
            // Use reflection to set up the internal service state with date range
            var serviceField = typeof(FuelTrackingServiceTests).GetField(
                "_fuelTrackingService",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (serviceField != null)
            {
                var service = serviceField.GetValue(instance);
                var filterMethod = service?.GetType().GetMethod("SetDateRangeAsync");

                if (filterMethod != null)
                {
                    filterMethod.Invoke(service, new object[] { startDate, endDate });
                }
            }

            return instance;
        }

        /// <summary>
        /// Asserts that a specific fuel record exists in the test service.
        /// </summary>
        /// <param name="test">The test instance.</param>
        /// <param name="recordId">The record ID to check for.</param>
        public static async Task AssertRecordExistsAsync(this FuelTrackingServiceTests test, string recordId)
        {
            // Use reflection to access the private _service field
            var serviceField = typeof(FuelTrackingServiceTests).GetField(
                "_service",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (serviceField == null)
            {
                throw new Exception("Could not find _service field in FuelTrackingServiceTests");
            }

            var service = serviceField.GetValue(test) as FuelTrackingService;
            if (service == null)
            {
                throw new Exception("Could not cast _service to FuelTrackingService");
            }

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
        public static async Task AssertRecordDoesNotExistAsync(this FuelTrackingServiceTests test, string recordId)
        {
            // Use reflection to access the private _service field
            var serviceField = typeof(FuelTrackingServiceTests).GetField(
                "_service",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (serviceField == null)
            {
                throw new Exception("Could not find _service field in FuelTrackingServiceTests");
            }

            var service = serviceField.GetValue(test) as FuelTrackingService;
            if (service == null)
            {
                throw new Exception("Could not cast _service to FuelTrackingService");
            }

            var records = await service.GetRecordsAsync("testVehicle", null);
            if (records.Any(r => r.Id == recordId))
            {
                throw new Exception($"Expected record with ID {recordId} to not exist but it was found.");
            }
        }
    }
}
