# CLAUDE.md

.NET 10 class library (NuGet `gps-tracker-protocol`) that parses GPS tracker protocols (GT06, H02, TK103) from raw TCP/UDP bytes into structured location data, plus fleet services (devices, geofences, journeys, analytics).

## Build

```bash
dotnet restore
dotnet build GpsTrackerProtocol.csproj -c Release   # library only
dotnet build gps-tracker-protocol.sln -c Release     # library + tests
make build                                           # same as above (Release)
```

SDK pinned in `global.json` (10.0.100, rollForward latestMinor). `Directory.Build.props` sets `Nullable` + `ImplicitUsings` on, warnings not errors.

## Test

```bash
dotnet test gps-tracker-protocol.sln -c Release --logger "console;verbosity=normal"
make test            # same
make test-coverage   # opencover via /p:CollectCoverage=true
dotnet test tests/gps-tracker-protocol.Tests --filter "FullyQualifiedName~ProtocolParserServiceTests"
```

Stack: xUnit 2.9, FluentAssertions 7, NSubstitute 5. Tests live in `tests/gps-tracker-protocol.Tests/` (Nullable disabled there).

## Lint / format

```bash
make format          # dotnet format GpsTrackerProtocol.csproj
make lint            # dotnet format --verify-no-changes
make analyze         # build with EnableNETAnalyzers + EnforceCodeStyleInBuild
```

Rules in `.editorconfig`: 4 spaces, max line 100, `Async` suffix on async methods, PascalCase public, `_camelCase` private, UPPER_CASE constants.

## Layout

Source lives in the repo root (root namespace `GpsTrackerProtocol`); `tests/`, `examples/`, `gps-tracker-protocol.Benchmarks/` are excluded from the main csproj via `DefaultItemExcludes`.

- `Parsers/` - `IProtocolParser`, `IFrameParser`, `Gt06ProtocolParser`, `H02ProtocolParser`, `Tk103ProtocolParser`, `Gt06ResponseBuilder`
- `Services/` - `ProtocolParserService` (main parsing facade), `ProtocolAutoDetector`, `FrameReassembler`, `NmeaSentenceParser`, `LocationSanityFilter`, device/geofence/journey/fuel/analytics/route services
- `Domain/Models/`, `Domain/Enums/` - records/enums (`LocationData`, `GpsFrame`, `ParseResult<T>`, `Device`, `Command`, `ProtocolType`...); `Domain/Exceptions.cs`
- `Configuration/DependencyInjection.cs` - `AddGpsTrackerServices()` DI entry point; `GpsTrackerProtocolOptions.cs` in root
- `Data/` - `IRepository`, `IUnitOfWork`, in-memory implementations
- `Infrastructure/` - error handling middleware, logging/validation pipelines, `RateLimitingService`
- `Events/` - `IDomainEvent`, `IEventPublisher`, geofence events
- `BackgroundWorkers/`, `Caching/`, `Formatting/` (JSON/CSV/GeoJSON/GPX), `Integration/` (geocoding, weather, webhook, notifications), `Utilities/` (byte/string/GPS helpers, Kalman smoother), `CLI/`
- `src/GpsTrackerProtocol/Diagnostics/` - `IParserMetrics`
- `Constants.cs` - protocol markers and limits
- `examples/` - standalone example projects; `gps-tracker-protocol.Benchmarks/Program.cs` - BenchmarkDotNet entry (`make benchmark`)
- `docs/` - per-type markdown docs; `README.md` is the NuGet readme
- CI: `.github/workflows/ci.yml`, `build.yml` (ubuntu/windows/macos matrix), `nuget-publish.yml`, `docker.yml`
- Docker: `Dockerfile`, `docker-compose.yml`, `make docker`

## Conventions

- File-scoped namespaces matching folder (`GpsTrackerProtocol.Parsers`, `.Services`, ...); `using` directives placed after the namespace line; `#nullable enable` at top of each file.
- Every file starts with the author header comment (Vladyslav Zaiets | https://sarmkadan.com).
- XML doc comments on all public members (`GenerateDocumentationFile` is on).
- Interface + implementation pairs (`IXService` / `XService`), registered as singletons in `DependencyInjection.cs`.
- Parsing returns `ParseResult<T>` rather than throwing; parsers accept `ReadOnlySpan<byte>`.
- Extension/validation helpers go in sibling files named `*Extensions.cs`, `*Validation.cs`, `*JsonExtensions.cs`.
- Tests: `MethodName_ShouldExpected_WhenCondition`, `[Fact]`/`[Theory]`, FluentAssertions `.Should()`, NSubstitute for mocks. Test files mirror source names (`XServiceTests.cs`, with `*TestsExtensions.cs` / `*TestsValidation.cs` splits).
- Do not commit `*.backup` / `*.modified` scratch copies (see `Services/`); `.aider*`, `bin/`, `obj/` are gitignored.
