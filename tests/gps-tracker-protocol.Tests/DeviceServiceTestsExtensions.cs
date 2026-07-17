using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gps_tracker_protocol.Tests
{
    /// <summary>
    /// Extension methods that add utility functionality for <see cref="DeviceServiceTests"/>.
    /// </summary>
    public static class DeviceServiceTestsExtensions
    {
        /// <summary>
        /// Returns a read‑only list of delegates that invoke each public test method on the supplied
        /// <see cref="DeviceServiceTests"/> instance.
        /// </summary>
        /// <param name="tests">The test class instance.</param>
        /// <returns>A read‑only list of <see cref="Func{DeviceServiceTests, Task}"/> delegates.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static IReadOnlyList<Func<DeviceServiceTests, Task>> GetAllTestMethods(this DeviceServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            return new List<Func<DeviceServiceTests, Task>>
            {
                t => t.RegisterDeviceAsync_ShouldAddDevice(),
                t => t.RegisterDeviceAsync_ShouldReturnExistingDevice_IfAlreadyRegistered(),
                t => t.GetDeviceByIdAsync_ShouldReturnDevice(),
                t => t.GetDeviceByIdAsync_ShouldReturnNull_WhenDeviceNotFound(),
                t => t.UpdateDeviceStatusAsync_ShouldUpdateDevice(),
                t => t.UpdateDeviceStatusAsync_ShouldDoNothing_WhenDeviceNotFound(),
                t => t.GetAllDevicesAsync_ShouldReturnAllDevices()
            };
        }

        /// <summary>
        /// Executes all test methods sequentially and propagates any exception that occurs.
        /// </summary>
        /// <param name="tests">The test class instance.</param>
        /// <returns>A task that completes when all tests have run.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static async Task RunAllAsync(this DeviceServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            foreach (var test in tests.GetAllTestMethods())
            {
                await test(tests).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Executes all test methods and returns <c>true</c> if none of them throws an exception;
        /// otherwise returns <c>false</c>.
        /// </summary>
        /// <param name="tests">The test class instance.</param>
        /// <returns>A task that resolves to <c>true</c> when all tests pass, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static async Task<bool> AllTestsPassedAsync(this DeviceServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            try
            {
                await tests.RunAllAsync().ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a simple, culture‑invariant summary of the test methods contained in the
        /// <see cref="DeviceServiceTests"/> instance.
        /// </summary>
        /// <param name="tests">The test class instance.</param>
        /// <returns>A comma‑separated list of test method names.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static string GetTestSummary(this DeviceServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            var names = new List<string>
            {
                nameof(tests.RegisterDeviceAsync_ShouldAddDevice),
                nameof(tests.RegisterDeviceAsync_ShouldReturnExistingDevice_IfAlreadyRegistered),
                nameof(tests.GetDeviceByIdAsync_ShouldReturnDevice),
                nameof(tests.GetDeviceByIdAsync_ShouldReturnNull_WhenDeviceNotFound),
                nameof(tests.UpdateDeviceStatusAsync_ShouldUpdateDevice),
                nameof(tests.UpdateDeviceStatusAsync_ShouldDoNothing_WhenDeviceNotFound),
                nameof(tests.GetAllDevicesAsync_ShouldReturnAllDevices)
            };

            return string.Join(", ", names);
        }
    }
}
