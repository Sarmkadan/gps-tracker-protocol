# CommandLineInterface

`CommandLineInterface` (`CLI/CommandLineInterface.cs`) is a command router for operating the library from a terminal or a host process. It implements `ICommandLineInterface`, which exposes a single method, `ExecuteAsync(string[] args)`, that returns a process exit code. The class is registered as a singleton by `AddGpsTrackerServices()`.

The library does not ship an executable. A host application builds the service provider, passes its command-line arguments to `ExecuteAsync`, and returns the result as its process exit code. The usage text printed by `help` uses `GpsTrackerProtocol` as the program name; substitute your host's executable name.

## Usage

```
GpsTrackerProtocol <command> [options]
```

Command names are case-insensitive. Running the host with no arguments prints the help text and exits with `0`.

## Commands

| Command | Description |
|---------|-------------|
| `parse <protocol> <hex-data>` | Detects the protocol and reports whether the frame is valid. For a valid frame, also prints the parsed location as indented JSON. |
| `devices` | Lists all registered devices with their ID, name, IMEI, and status. |
| `location <device-id> [count]` | Prints the device's location history as CSV, up to `count` entries (default `10`). |
| `journey <device-id>` | Prints the active journeys for a device with waypoint count and total distance in km. |
| `export <device-id> <format> [output-file]` | Writes location history to a file. `format` is `json` or `csv`. |
| `alerts list <device-id>` | Lists active geofence alerts for a device. |
| `alerts add <device-id> <geofence-id> <enter\|exit>` | Creates a geofence alert rule that fires on enter or exit. |
| `alerts ack <alert-id> [notes]` | Acknowledges an alert. Trailing words are joined with spaces as notes. |
| `replay <journey-id> [speed-multiplier]` | Prints the route frames of a journey with coordinates, speed, replay timestamp, and cumulative distance. |
| `diagnostics <device-id> [--selftest]` | Shows device status, last contact, packet and location counts, battery, and signal. With `--selftest`, runs the device self-test and prints `PASS` or `WARN` with each warning. |
| `help [command]` | Prints the usage text. The optional `command` argument is accepted but ignored; the full text is always printed. |

### parse

```
parse <protocol> <hex-data>
```

- `protocol` is parsed as a `ProtocolType` name after conversion to upper case: `GT06`, `H02`, or `TK103`. Any other value is reported as an error (exit `1`).
- `hex-data` may be split across several arguments and may contain spaces or dashes; they are removed before decoding. Odd-length or non-hex input is reported as `Invalid hex data` (exit `1`).
- Output lines: `Detected protocol: <type>`, `Valid frame: <True|False>`, and, only for a valid frame, `Parsed location:` followed by the JSON.

### devices

```
devices
```

Prints `No devices found`, or `Total devices: <n>` followed by one indented line per device: `<id>: <name> (<imei>) - <status>`.

### location

```
location <device-id> [count]
```

- `count` defaults to `10`. A value that is not an integer is silently replaced by the default.
- Prints the history as CSV, produced by the CSV formatter. Prints `No locations found for device <id>` when there is no data.

### journey

```
journey <device-id>
```

Prints one line per journey whose `Status` equals the CLI's active-journey constant (`1`): `Journey <id>: <waypoints> waypoints, <distance>km`. The distance is formatted with two decimals.

> **Note:** the `Status` comment in `Domain/Models/Journey.cs` lists `1` as *completed*. The CLI treats `1` as active. Check this when relying on the output.

### export

```
export <device-id> <format> [output-file]
```

- `format` is case-insensitive. `json` writes the first location serialized with the JSON formatter; `csv` writes the full location history as CSV.
- `geojson` is listed in the usage text but is **not implemented**. It fails with `Error: Unsupported format: geojson` (exit `1`). No file is written.
- When `output-file` is omitted, the file is `export-<device-id>-<yyyyMMdd-HHmmss>.<format>` (UTC timestamp) in the current working directory. An existing file at the target path is overwritten.
- On success: `Exported <n> locations to <file>`. When the device has no locations, the command writes an error to standard error and exits with `1`.

### alerts

```
alerts list <device-id>
alerts add <device-id> <geofence-id> <enter|exit>
alerts ack <alert-id> [notes]
```

- `list` prints one line per active alert: `[<first 8 characters of id>] <type> geofence=<id> at <UTC time>`.
- `add` creates the rule and prints `Alert rule created: <rule-id-prefix> (<type> on <geofence> for <device>)`. The direction is compared to `exit` (case-insensitive); any other value creates an `Enter` rule.
- `ack` prints `Alert <id> acknowledged.` (exit `0`) or `Alert <id> not found.` (exit `1`).
- An unknown sub-command is reported on standard error with exit `1`. Running `alerts` without a sub-command prints its usage and exits with `1`.

### replay

```
replay <journey-id> [speed-multiplier]
```

- `speed-multiplier` defaults to `1.0`. It must parse as a floating-point number using invariant culture (for example `10` or `2.5`); otherwise the command reports `Invalid speed multiplier; must be a positive number.` and exits with `1`.
- The handler does not reject zero or negative numbers, despite the message wording.
- Output is a summary (frame count, distance, real duration, replay duration at the chosen speed) followed by a table with columns `#`, `Lat`, `Lon`, `Speed`, `Replay Time`, and `Km`.
- A replay failure reported as `InvalidOperationException` prints `Replay failed: <message>` on standard error and exits with `1`.

### diagnostics

```
diagnostics <device-id> [--selftest]
```

- Without the flag, prints a report: status and online state, last seen time and time since last contact, packets received, battery (`unknown` when below zero), signal quality and strength, location point count, total distance, journey counts, and the last position when known.
- With `--selftest`, prints `Self-test for <id>: PASS` or `WARN`, followed by each warning prefixed with `⚠`. Exit code is `0` for `PASS` and `2` for `WARN`.
- Unknown device IDs print `Device <id> not found.` on standard error and exit with `1`.
- The flag is detected anywhere in the arguments, but `device-id` is always the first argument after the command.

### help

```
help
```

Prints the usage text and exits with `0`. Running the host with no arguments does the same.

## Exit codes

| Code | Meaning |
|------|---------|
| `0` | Success. |
| `1` | Error, missing arguments, unknown command, unsupported export format, or a device / alert that does not exist. Unexpected exceptions are logged and reported as `Error: <message>` on standard error. |
| `2` | `diagnostics --selftest` completed with warnings. |

Output written with `Console.WriteLine` goes to standard output. Usage problems that the code reports with `Console.Error` (unknown commands, invalid input, unknown alert sub-commands) go to standard error.

## Example

Host program that wires up the CLI:

```csharp
using GpsTrackerProtocol.CLI;
using GpsTrackerProtocol.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddGpsTrackerServices();

        await using var provider = services.BuildServiceProvider();
        var cli = provider.GetRequiredService<ICommandLineInterface>();

        return await cli.ExecuteAsync(args);
    }
}
```

Example invocations:

```bash
# Show usage
GpsTrackerProtocol help

# Parse a GT06 login frame (spaces in the hex string are allowed)
GpsTrackerProtocol parse GT06 78781F120119110B162334

# List registered devices
GpsTrackerProtocol devices

# Show the last 50 locations for a device as CSV
GpsTrackerProtocol location device-001 50

# Show active journeys for a device
GpsTrackerProtocol journey device-001

# Export all locations as CSV to a file
GpsTrackerProtocol export device-001 csv output.csv

# Export as JSON using the default file name (export-device-001-<timestamp>.json)
GpsTrackerProtocol export device-001 json

# Create an alert rule that fires when the device enters a geofence
GpsTrackerProtocol alerts add device-001 zone-hq enter

# List active alerts for a device
GpsTrackerProtocol alerts list device-001

# Acknowledge an alert with notes
GpsTrackerProtocol alerts ack 3f9c1a2b arrived at depot

# Replay a journey at 10x speed
GpsTrackerProtocol replay journey-abc 10

# Show diagnostics, then run the self-test (exit code 2 on warnings)
GpsTrackerProtocol diagnostics device-001
GpsTrackerProtocol diagnostics device-001 --selftest
```
