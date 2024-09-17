FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["GpsTrackerProtocol.csproj", "./"]
RUN dotnet restore "GpsTrackerProtocol.csproj"
COPY . .
RUN dotnet build "GpsTrackerProtocol.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GpsTrackerProtocol.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GpsTrackerProtocol.dll"]
