using System;
using System.Collections.Generic;
using System.Linq;

namespace gps_tracker_protocol.Tests
{
    public static class DeviceServiceTestsValidation
    {
        public static IReadOnlyList<string> Validate(this DeviceServiceTests value)
        {
            var errors = new List<string>();

            if (value == null)
            {
                errors.Add("DeviceServiceTests instance is null");
                return errors.AsReadOnly();
            }

            return errors.AsReadOnly();
        }

        public static bool IsValid(this DeviceServiceTests value)
        {
            return !Validate(value).Any();
        }

        public static void EnsureValid(this DeviceServiceTests value)
        {
            var errors = Validate(value);
            if (errors.Any())
            {
                throw new ArgumentException(string.Join(Environment.NewLine, errors));
            }
        }
    }
}