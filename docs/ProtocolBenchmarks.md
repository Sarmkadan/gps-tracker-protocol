# ProtocolBenchmarks

`ProtocolBenchmarks` is a benchmarking utility for GPS tracker protocol parsers, designed to measure and compare the performance of parsing and processing frames from various GPS tracker devices. It provides methods to validate, parse, store, and analyze location data across different protocols (e.g., GT06, H02, TK103) and includes batch operations for high-throughput scenarios.

## API

### `public void Setup()`
Initializes the benchmarking environment, including test data, mock storage, and protocol parsers. Call this before any benchmarking methods to ensure a clean state. Throws `InvalidOperationException` if called after `Cleanup()`.

### `public void Cleanup()`
Releases resources allocated during `Setup()`, such as clearing test data and disposing of protocol parsers. Call this after all benchmarking methods to avoid memory leaks. Throws `InvalidOperationException` if called before `Setup()`.

### `public async Task Parse_GT06_Frame(byte[] frameData)`
Parses a single GT06 protocol frame. The input `frameData` must contain a valid GT06 frame. Returns a parsed location object if successful. Throws `ArgumentException` if the frame is malformed or unsupported.

### `public async Task Parse_H02_Frame(byte[] frameData)`
Parses a single H02 protocol frame. The input `frameData` must contain a valid H02 frame. Returns a parsed location object if successful. Throws `ArgumentException` if the frame is malformed or unsupported.

### `public async Task Parse_TK103_Frame(byte[] frameData)`
Parses a single TK103 protocol frame. The input `frameData` must contain a valid TK103 frame. Returns a parsed location object if successful. Throws `ArgumentException` if the frame is malformed or unsupported.

### `public async Task Detect_Protocol(byte[] frameData)`
Detects the protocol type of the given frame. The input `frameData` must contain a valid frame header. Returns the detected protocol name (e.g., "GT06", "H02"). Throws `ArgumentException` if the frame is too short or unrecognized.

### `public async Task Validate_GT06_Frame(byte[] frameData)`
Validates a GT06 frame without parsing it. The input `frameData` must contain a GT06 frame. Returns `true` if the frame is valid; otherwise, `false`. Throws `ArgumentException` if the frame is malformed.

### `public async Task Store_Location_Data(LocationData location)`
Stores parsed location data in the benchmarking storage backend. The input `location` must be non-null. Returns the stored location ID. Throws `ArgumentNullException` if `location` is null.

### `public async Task Get_Latest_Location(string deviceId)`
Retrieves the most recent location data for a given device. The input `deviceId` must be non-null or empty. Returns the latest `LocationData` or `null` if no data exists. Throws `ArgumentException` if `deviceId` is invalid.

### `public async Task Batch_Parse_100_Frames(byte[] frameData)`
Parses 100 identical frames in sequence. The input `frameData` must contain a valid frame. Returns a list of parsed location objects. Throws `ArgumentException` if the frame is malformed or unsupported.

### `public async Task Batch_Parse_100_Frames_List(IEnumerable<byte[]> frameDataList)`
Parses 100 frames from a list of byte arrays. Each array in `frameDataList` must contain a valid frame. Returns a list of parsed location objects. Throws `ArgumentException` if any frame is malformed or unsupported.

### `public async Task Generate_Device_Analytics(string deviceId)`
Generates analytics for a single device, including distance traveled, average speed, and idle time. The input `deviceId` must be non-null or empty. Returns a `DeviceAnalytics` object. Throws `ArgumentException` if `deviceId` is invalid.

### `public async Task Generate_Fleet_Analytics(IEnumerable<string> deviceIds)`
Generates aggregated analytics for a fleet of devices. The input `deviceIds` must be non-null and contain valid IDs. Returns a `FleetAnalytics` object. Throws `ArgumentNullException` if `deviceIds` is null.

### `public async Task Register_Device(string deviceId, string protocol)`
Registers a new device with the benchmarking system. The input `deviceId` must be non-null or empty, and `protocol` must specify a supported protocol. Returns `true` if registration succeeds. Throws `ArgumentException` if `deviceId` or `protocol` is invalid.

### `public async Task Get_Location_History(string deviceId, DateTime from, DateTime to)`
Retrieves location history for a device within a time range. The input `deviceId` must be non-null or empty, and `from` must not be later than `to`. Returns a list of `LocationData` objects. Throws `ArgumentException` if `deviceId` is invalid or the time range is invalid.

## Usage

### Example 1: Benchmarking a Single Frame
