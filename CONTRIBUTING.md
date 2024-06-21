# Contributing to GPS Tracker Protocol

Thank you for your interest in contributing! This document provides guidelines for participating in this project.

## Code of Conduct

Please review our [Code of Conduct](CODE_OF_CONDUCT.md) before contributing. All contributors are expected to uphold this code in all interactions.

## Getting Started

### Prerequisites

- **.NET SDK**: 10.0 or later ([Download](https://dotnet.microsoft.com/download))
- **Git**: Latest version recommended
- **IDE**: Visual Studio 2022+, VS Code with C# extensions, or JetBrains Rider

### Development Setup

1. **Fork the repository**
   ```bash
   # Visit https://github.com/sarmkadan/gps-tracker-protocol and click "Fork"
   ```

2. **Clone your fork**
   ```bash
   git clone https://github.com/YOUR-USERNAME/gps-tracker-protocol.git
   cd gps-tracker-protocol
   ```

3. **Add upstream remote**
   ```bash
   git remote add upstream https://github.com/sarmkadan/gps-tracker-protocol.git
   ```

4. **Verify setup**
   ```bash
   dotnet build
   make build    # or use the Makefile
   ```

## Branching Strategy

- `main` - Production-ready code, protected branch
- `feature/*` - New features (e.g., `feature/protocol-extension`)
- `fix/*` - Bug fixes (e.g., `fix/checksum-validation`)
- `docs/*` - Documentation updates
- `refactor/*` - Code refactoring without behavior changes

### Creating a Branch

```bash
git fetch upstream
git checkout -b feature/your-feature-name upstream/main
```

## Code Style & Conventions

### C# Conventions

- **Naming**: PascalCase for public members, camelCase for private/local
- **Indentation**: 4 spaces (enforced by .editorconfig)
- **Line Length**: Target 120 characters, hard limit 140
- **Braces**: Allman style (opening brace on new line)

### Documentation

- **XML Documentation**: All public APIs require XML doc comments
  ```csharp
  /// <summary>
  /// Validates the GPS frame checksum.
  /// </summary>
  /// <param name="frame">The GPS frame to validate</param>
  /// <returns>True if valid, false otherwise</returns>
  public bool ValidateChecksum(GpsFrame frame)
  {
      // implementation
  }
  ```

- **Author Headers**: All source files must include the author header:
  ```csharp
  // =============================================================================
  // Author: Your Name | your-website.com (optional)
  // Your Role/Title (optional)
  // =============================================================================
  ```

- **Comments**: Use for "why" not "what". Well-named code is self-documenting.

### Testing

- **Unit Tests**: Write tests in `.Tests.cs` files alongside implementations
- **Coverage**: Aim for >80% code coverage on new features
- **Naming**: Test methods follow `Method_Scenario_ExpectedResult` pattern
- **Assertions**: Use clear, descriptive assertions

Example:
```csharp
[Fact]
public void ParseGT06Frame_ValidFrame_ReturnsCorrectLocation()
{
    // Arrange
    var frame = CreateValidGT06Frame();
    
    // Act
    var result = parser.Parse(frame);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(40.7128, result.Latitude);
}
```

## Making Changes

### Before You Start

1. Check for existing [issues](https://github.com/sarmkadan/gps-tracker-protocol/issues)
2. Open an issue to discuss significant changes first
3. Ensure your changes align with project goals

### Code Quality Checklist

- [ ] Code follows style guidelines (.editorconfig enforced)
- [ ] All public APIs have XML documentation
- [ ] Author headers are preserved in modified files
- [ ] New tests added for new functionality
- [ ] Existing tests still pass (`dotnet test`)
- [ ] No performance regressions
- [ ] No breaking changes (or well-justified)
- [ ] Commit messages are clear and descriptive

### Before Committing

```bash
# Build and test locally
dotnet build
dotnet test

# Check formatting
dotnet format --verify-no-changes

# View your changes
git diff
```

## Submitting a Pull Request

### PR Process

1. **Ensure upstream is current**
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Push to your fork**
   ```bash
   git push origin feature/your-feature-name
   ```

3. **Open Pull Request** on GitHub
   - Use a clear, descriptive title
   - Reference related issues (#123)
   - Describe what changed and why
   - Include any breaking changes

### PR Requirements

- ✅ All checks pass (build, tests, formatting)
- ✅ At least one approval from maintainers
- ✅ Code coverage maintained or improved
- ✅ Documentation updated
- ✅ No merge conflicts

### PR Title Format

```
type: brief description

fix: handle null reference in checksum validation
feat: add TK103 protocol support
docs: update installation instructions
refactor: simplify location data aggregation
```

## Reporting Issues

### Bug Reports

Use [GitHub Issues](https://github.com/sarmkadan/gps-tracker-protocol/issues) with:

1. **Title**: Clear, specific description
2. **Environment**: .NET version, OS, reproduce steps
3. **Expected vs. Actual**: What should happen vs. what happens
4. **Minimal Example**: Smallest code that reproduces the issue

### Feature Requests

- Describe the use case and motivation
- Explain how it benefits the project
- Discuss potential implementation approaches

### Security Issues

**Do not** open public issues for security vulnerabilities. See [SECURITY.md](SECURITY.md).

## Testing Your Changes

### Running Tests

```bash
# All tests
dotnet test

# Specific test file
dotnet test --filter ClassName

# With coverage
dotnet test /p:CollectCoverage=true
```

### Integration Testing

```bash
# Build docker image
docker build -t gps-tracker .

# Run with compose
docker-compose up
```

## Documentation

### Updating Docs

- Edit markdown files in `/docs`
- Keep docs in sync with code changes
- Link to relevant source files when helpful
- Include examples for new features

### Building Locally

```bash
# View rendered markdown
# Use VS Code preview or any markdown renderer
```

## Performance Considerations

- Profile before optimizing
- Include benchmarks for performance-critical code
- Use `PerformanceMonitor` utility for measurements
- Document performance trade-offs

## Maintainer Response Times

- **Acknowledgment**: Within 48 hours
- **Initial Assessment**: Within 1 week
- **Resolution**: Depends on complexity
- **Questions**: Feel free to ask in the issue

## Release Process

Releases follow [Semantic Versioning](https://semver.org/):
- **MAJOR**: Breaking changes
- **MINOR**: New features, backward-compatible
- **PATCH**: Bug fixes, backward-compatible

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).

## Questions?

- 📖 Check existing [documentation](docs/)
- 🔍 Search [closed issues](https://github.com/sarmkadan/gps-tracker-protocol/issues?q=is%3Aissue+is%3Aclosed)
- 💬 Open a [discussion](https://github.com/sarmkadan/gps-tracker-protocol/discussions)

---

Thank you for helping make GPS Tracker Protocol better! 🚀
