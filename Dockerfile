# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:10 AS builder

WORKDIR /build

COPY . .

RUN dotnet restore
RUN dotnet build -c Release
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:10

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=builder /app .

EXPOSE 5000 5001 8080

ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["./GpsTrackerProtocol"]
