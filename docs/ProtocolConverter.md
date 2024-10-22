# ProtocolConverter

A utility class that provides bidirectional conversion between raw binary frames and structured GPS-tracker protocol messages, as well as batch processing of protocol files.

## API

### `public ProtocolConverter()`

Initializes a new instance of the `ProtocolConverter` class. This constructor does not require any parameters and prepares the converter for subsequent frame or file operations.

### `public async Task<byte[]?> ConvertFrameAsync(byte[] frame)`

Converts a raw binary frame into a structured protocol message.

- **frame**: The raw binary frame received from the GPS tracker device.
- **return value**: A `byte[]` representing the structured protocol message, or `null` if the frame is invalid or cannot be converted.
- **exceptions**: Throws `ArgumentNullException` if `frame` is `null`.

### `public async Task ConvertFileAsync(string inputPath, string outputPath)`

Converts a file containing raw GPS tracker frames into a structured protocol message file.

- **inputPath**: The file system path to the input file containing raw binary frames.
- **outputPath**: The file system path where the converted structured protocol messages will be written.
- **exceptions**:
  - Throws `ArgumentNullException` if `inputPath` or `outputPath` is `null`.
  - Throws `FileNotFoundException` if `inputPath` does not exist.
  - Throws `DirectoryNotFoundException` if the directory for `outputPath` does not exist.
  - Throws `UnauthorizedAccessException` if the caller lacks required permissions.
  - Throws `IOException` on general I/O errors during file operations.

### `public static async Task Main(string[] args)`

Entry point for command-line usage of the protocol converter. Parses command-line arguments to determine whether to convert a single frame or a file, then invokes the appropriate conversion method.

- **args**: Command-line arguments. Supported formats:
  - Single frame: `convert-frame <hex-frame>`
  - File conversion: `convert-file <input-path> <output-path>`
- **return value**: Task representing the asynchronous operation.
- **exceptions**:
  - Throws `ArgumentException` if the command-line arguments are invalid or insufficient.
  - Propagates exceptions from `ConvertFrameAsync` or `ConvertFileAsync` as appropriate.

## Usage

### Converting a single frame from hex string
