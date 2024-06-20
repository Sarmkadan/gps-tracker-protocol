# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

.PHONY: help build clean run test format analyze docker docker-up docker-down

help:
	@echo "GPS Tracker Protocol Parser - Build Tasks"
	@echo ""
	@echo "Usage: make [target]"
	@echo ""
	@echo "Targets:"
	@echo "  build              Build the project in Release mode"
	@echo "  debug              Build the project in Debug mode"
	@echo "  clean              Clean build artifacts"
	@echo "  run                Run the demo application"
	@echo "  test               Run unit tests"
	@echo "  format             Format code using dotnet format"
	@echo "  analyze            Run static code analysis"
	@echo "  restore            Restore NuGet packages"
	@echo "  publish            Publish Release build"
	@echo "  docker             Build Docker image"
	@echo "  docker-up          Start Docker containers"
	@echo "  docker-down        Stop Docker containers"
	@echo "  docker-logs        View Docker logs"
	@echo "  benchmark          Run performance benchmarks"
	@echo "  import-csv         Run CSV importer example"
	@echo "  import-json        Run JSON importer example"
	@echo "  export-json        Export to JSON format"
	@echo "  export-csv         Export to CSV format"
	@echo "  export-geojson     Export to GeoJSON format"
	@echo "  server             Start real-time GPS server"
	@echo "  interactive        Start interactive command center"
	@echo "  analyze-journey    Analyze device journey"
	@echo "  convert            Convert between protocols"
	@echo "  info               Display project information"
	@echo ""

# Project info
PROJECT_NAME := GpsTrackerProtocol
PROJECT_FILE := GpsTrackerProtocol.csproj
SOLUTION_FILE := GpsTrackerProtocol.sln
BUILD_CONFIG := Release
DEBUG_CONFIG := Debug
OUTPUT_DIR := bin
PUBLISH_DIR := publish

# Docker info
DOCKER_IMAGE := gps-tracker-protocol
DOCKER_TAG := latest
DOCKER_REGISTRY := docker.io

# Build targets
build: restore
	@echo "Building $(PROJECT_NAME) in $(BUILD_CONFIG) mode..."
	dotnet build $(PROJECT_FILE) -c $(BUILD_CONFIG)
	@echo "Build complete!"

debug: restore
	@echo "Building $(PROJECT_NAME) in $(DEBUG_CONFIG) mode..."
	dotnet build $(PROJECT_FILE) -c $(DEBUG_CONFIG)
	@echo "Debug build complete!"

clean:
	@echo "Cleaning build artifacts..."
	dotnet clean $(PROJECT_FILE)
	rm -rf $(OUTPUT_DIR) $(PUBLISH_DIR) .vs .vscode
	find . -type d -name obj -exec rm -rf {} + 2>/dev/null || true
	find . -type d -name bin -exec rm -rf {} + 2>/dev/null || true
	@echo "Clean complete!"

restore:
	@echo "Restoring NuGet packages..."
	dotnet restore $(PROJECT_FILE)
	@echo "Restore complete!"

# Run targets
run: build
	@echo "Running $(PROJECT_NAME)..."
	dotnet run --project $(PROJECT_FILE) --configuration $(BUILD_CONFIG)

run-debug: debug
	@echo "Running $(PROJECT_NAME) in debug mode..."
	dotnet run --project $(PROJECT_FILE) --configuration $(DEBUG_CONFIG)

# Test targets
test:
	@echo "Running tests..."
	dotnet test $(SOLUTION_FILE) -c $(BUILD_CONFIG) --logger "console;verbosity=normal"
	@echo "Tests complete!"

test-verbose:
	@echo "Running tests with verbose output..."
	dotnet test $(SOLUTION_FILE) -c $(BUILD_CONFIG) --logger "console;verbosity=detailed"

test-coverage:
	@echo "Running tests with coverage..."
	dotnet test $(SOLUTION_FILE) -c $(BUILD_CONFIG) /p:CollectCoverage=true /p:CoverageFormat=opencover
	@echo "Coverage report generated!"

# Code quality targets
format:
	@echo "Formatting code..."
	dotnet format $(PROJECT_FILE)
	@echo "Code formatted!"

format-verify:
	@echo "Verifying code format..."
	dotnet format $(PROJECT_FILE) --verify-no-changes
	@echo "Format verification complete!"

analyze:
	@echo "Running static code analysis..."
	dotnet build $(PROJECT_FILE) -c $(BUILD_CONFIG) /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
	@echo "Analysis complete!"

lint:
	@echo "Running code linter..."
	dotnet format $(PROJECT_FILE) --verify-no-changes --verbosity diagnostic
	@echo "Lint check complete!"

# Publish targets
publish: clean
	@echo "Publishing $(PROJECT_NAME) in $(BUILD_CONFIG) mode..."
	dotnet publish $(PROJECT_FILE) -c $(BUILD_CONFIG) -o $(PUBLISH_DIR)
	@echo "Publish complete!"

publish-self-contained: clean
	@echo "Publishing $(PROJECT_NAME) as self-contained..."
	dotnet publish $(PROJECT_FILE) -c $(BUILD_CONFIG) -o $(PUBLISH_DIR) --self-contained
	@echo "Self-contained publish complete!"

# Docker targets
docker: clean
	@echo "Building Docker image: $(DOCKER_IMAGE):$(DOCKER_TAG)..."
	docker build -t $(DOCKER_IMAGE):$(DOCKER_TAG) .
	@echo "Docker image built successfully!"

docker-push: docker
	@echo "Pushing Docker image to registry..."
	docker tag $(DOCKER_IMAGE):$(DOCKER_TAG) $(DOCKER_REGISTRY)/$(DOCKER_IMAGE):$(DOCKER_TAG)
	docker push $(DOCKER_REGISTRY)/$(DOCKER_IMAGE):$(DOCKER_TAG)
	@echo "Docker image pushed!"

docker-up:
	@echo "Starting Docker containers..."
	docker-compose up -d
	@echo "Containers started!"

docker-down:
	@echo "Stopping Docker containers..."
	docker-compose down
	@echo "Containers stopped!"

docker-logs:
	@echo "Showing Docker logs..."
	docker-compose logs -f

docker-build-no-cache:
	@echo "Building Docker image without cache..."
	docker build --no-cache -t $(DOCKER_IMAGE):$(DOCKER_TAG) .
	@echo "Docker image built!"

docker-clean:
	@echo "Cleaning Docker resources..."
	docker-compose down -v
	docker rmi $(DOCKER_IMAGE):$(DOCKER_TAG) 2>/dev/null || true
	@echo "Docker resources cleaned!"

# Example targets
benchmark:
	@echo "Running performance benchmarks..."
	dotnet run --project examples/PerformanceBenchmark.cs -- all

benchmark-validation:
	@echo "Running frame validation benchmark..."
	dotnet run --project examples/PerformanceBenchmark.cs -- validation 10000

benchmark-storage:
	@echo "Running location storage benchmark..."
	dotnet run --project examples/PerformanceBenchmark.cs -- storage 10000

benchmark-stress:
	@echo "Running stress test (100 devices, 100 locations each)..."
	dotnet run --project examples/PerformanceBenchmark.cs -- stress 100 100

import-csv:
	@echo "Running CSV importer..."
	dotnet run --project examples/BatchDataImporter.cs -- csv devices.csv

import-json:
	@echo "Running JSON importer..."
	dotnet run --project examples/BatchDataImporter.cs -- json locations.json

export-json:
	@echo "Exporting to JSON..."
	dotnet run --project examples/DataExporter.cs -- json locations.json device-001

export-csv:
	@echo "Exporting to CSV..."
	dotnet run --project examples/DataExporter.cs -- csv locations.csv device-001

export-geojson:
	@echo "Exporting to GeoJSON..."
	dotnet run --project examples/DataExporter.cs -- geojson map.json device-001

server:
	@echo "Starting real-time GPS server on TCP:5000 UDP:5001..."
	dotnet run --project examples/RealTimeGpsServer.cs

server-custom:
	@echo "Starting GPS server (usage: make server-custom PORT=9000)"
	dotnet run --project examples/RealTimeGpsServer.cs $(PORT)

interactive:
	@echo "Starting interactive command center..."
	dotnet run --project examples/DeviceCommandCenter.cs

analyze-journey:
	@echo "Analyzing device journey (usage: make analyze-journey DEVICE=device-001)"
	dotnet run --project examples/JourneyAnalyzer.cs -- analyze $(DEVICE)

simulate-journey:
	@echo "Simulating journey (usage: make simulate-journey DEVICE=device-001 POINTS=10)"
	dotnet run --project examples/JourneyAnalyzer.cs -- simulate $(DEVICE) $(POINTS)

fleet-report:
	@echo "Generating fleet report..."
	dotnet run --project examples/JourneyAnalyzer.cs -- fleet

convert:
	@echo "Converting GPS frames (usage: make convert FROM=GT06 TO=H02 INPUT=data.bin)"
	dotnet run --project examples/ProtocolConverter.cs $(FROM) $(TO) $(INPUT)

# Utility targets
info:
	@echo "GPS Tracker Protocol Parser - Project Information"
	@echo "=================================================="
	@echo "Project Name:   $(PROJECT_NAME)"
	@echo "Project File:   $(PROJECT_FILE)"
	@echo "Build Config:   $(BUILD_CONFIG)"
	@echo "Output Dir:     $(OUTPUT_DIR)"
	@echo "Publish Dir:    $(PUBLISH_DIR)"
	@echo "Docker Image:   $(DOCKER_IMAGE):$(DOCKER_TAG)"
	@echo ""
	@echo "Installed .NET:"
	@dotnet --version
	@echo ""
	@echo "SDK Info:"
	@dotnet --info | grep "Version" | head -5

deps:
	@echo "Listing project dependencies..."
	dotnet list $(PROJECT_FILE) package

update-deps:
	@echo "Checking for NuGet package updates..."
	dotnet list $(PROJECT_FILE) package --outdated

# Documentation targets
docs:
	@echo "Documentation is available in docs/ directory"
	@echo "  - docs/getting-started.md    - Setup and quick start guide"
	@echo "  - docs/architecture.md        - Architecture and design patterns"
	@echo "  - docs/api-reference.md       - Complete API documentation"
	@echo "  - docs/deployment.md          - Production deployment guide"
	@echo "  - docs/faq.md                 - Frequently asked questions"
	@echo "  - README.md                   - Main project README"

# CI/CD targets (for automation)
ci-build: clean build test format-verify analyze
	@echo "CI build complete!"

ci-publish: ci-build publish
	@echo "CI publish complete!"

ci-docker: ci-build docker
	@echo "CI Docker build complete!"

# Development targets
dev-setup: restore
	@echo "Setting up development environment..."
	@echo "  - Restoring packages"
	@echo "  - Setting up git hooks (future)"
	@echo "Development environment ready!"

watch:
	@echo "Watching for file changes and running tests..."
	dotnet watch --project $(PROJECT_FILE) run

watch-test:
	@echo "Watching for file changes and running tests..."
	dotnet watch --project $(PROJECT_FILE) test

# Cleanup targets
distclean: clean docker-clean
	@echo "Performing complete cleanup..."
	rm -rf $(PUBLISH_DIR)
	rm -rf .coverage
	find . -type f -name "*.log" -delete
	@echo "Complete cleanup done!"

# Default target
.DEFAULT_GOAL := help

# Silent by default, use 'make V=1' for verbose
ifeq ($(V),)
.SILENT: help build clean run test format analyze info
endif
