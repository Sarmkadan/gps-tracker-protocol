# GpsTrackerProtocolOptions

Configuration class for managing protocol-specific settings and operational parameters in the GPS tracker protocol system. This class centralizes configuration options for multiple supported protocols (GT06, H02, TK103) and general system constraints such as device limits, caching, and logging.

## API

### DefaultProtocol
Gets or sets the default protocol identifier used when no specific protocol is specified.  
**Type:** `string`  
**Exceptions:** None  

### MaxDevices
Gets or sets the maximum number of concurrent devices supported by the tracker.  
**Type:** `int`  
**Exceptions:** None  

### LocationHistoryLimit
Gets or sets the maximum number of location records retained per device.  
**Type:** `int`  
**Exceptions:** None  

### CacheExpirationMinutes
Gets or sets the duration (in minutes) after which cached data expires.  
**Type:** `int`  
**Exceptions:** None  

### RateLimitPerMinute
Gets or sets the maximum number of requests allowed per minute per device.  
**Type:** `int`  
**Exceptions:** None  

### LoggingLevel
Gets or sets the verbosity level for protocol logging (e.g., "Debug", "Info", "Error").  
**Type:** `string`  
**Exceptions:** None  

### Protocol
Gets or sets the nested `ProtocolSettings` instance containing protocol-specific configurations.  
**Type:** `ProtocolSettings`  
**Exceptions:** None  

### GT06Enabled
Gets or sets whether the GT06 protocol is enabled.  
**Type:** `bool`  
**Exceptions:** None  

### GT06Timeout
Gets or sets the timeout (in milliseconds) for GT06 protocol operations.  
**Type:** `int`  
**Exceptions:** None  

### GT06MaxFrameSize
Gets or sets the maximum frame size (in bytes) for GT06 protocol messages.  
**Type:** `int`  
**Exceptions:** None  

### H02Enabled
Gets or sets whether the H02 protocol is enabled.  
**Type:** `bool`  
**Exceptions:** None  

### H02Timeout
Gets or sets the timeout (in milliseconds) for H02 protocol operations.  
**Type:** `int`  
**Exceptions:** None  

### H02MaxFrameSize
Gets or sets the maximum frame size (in bytes) for H02 protocol messages.  
**Type:** `int`  
**Exceptions:** None  

### TK103Enabled
Gets or sets whether the TK103 protocol is enabled.  
**Type:** `bool`  
**Exceptions:** None  

### TK103Timeout
Gets or sets the timeout (in milliseconds) for TK103 protocol operations.  
**Type:** `int`  
**Exceptions:** None  

### TK103MaxFrameSize
Gets or sets the maximum frame size (in bytes) for TK103 protocol messages.  
**Type:** `int`  
**Exceptions:** None  

## Usage

```csharp
// Example 1: Basic configuration with default settings
var options = new GpsTrackerProtocolOptions
{
    DefaultProtocol = "GT06",
    MaxDevices = 100,
    LocationHistoryLimit = 50,
    CacheExpirationMinutes = 30,
    RateLimitPerMinute = 60,
    LoggingLevel = "Info",
    GT06Enabled = true,
    GT06Timeout = 5000,
    GT06MaxFrameSize = 1024
};

var tracker = new GpsTracker(options);
```

```csharp
// Example 2: Multi-protocol configuration
var options = new GpsTrackerProtocolOptions();
options.GT06Enabled = true;
options.GT06Timeout = 3000;
options.H02Enabled = true;
options.H02MaxFrameSize = 2048;
options.TK103Enabled = false;

options.Protocol = new ProtocolSettings
{
    // Additional protocol-specific settings
};

var service = new ProtocolService(options);
```

## Notes

- Thread safety is not guaranteed for runtime modifications. Properties should be configured during initialization and treated as immutable thereafter.
- Negative values for numeric properties (e.g., `MaxDevices`, `Timeout`) may result in undefined behavior and should be validated by the consuming application.
- Disabling all protocols (`GT06Enabled`, `H02Enabled`, `TK103Enabled` set to `false`) will prevent the tracker from processing any incoming data.
- The `Protocol` property may require additional configuration depending on the implementation of `ProtocolSettings`.
