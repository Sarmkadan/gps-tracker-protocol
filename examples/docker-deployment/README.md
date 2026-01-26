# GPS Tracker Protocol Docker Deployment Example

This example demonstrates how to deploy the GPS Tracker Protocol v2.0 using Docker and Docker Compose with the new fleet analytics features.

## Prerequisites
- Docker Engine 20.10+
- Docker Compose v2+

## Running the Example

1. Navigate to the example directory:
```bash
cd examples/docker-deployment
```

2. Start the services:
```bash
docker-compose up -d
```

3. Verify the services are running:
```bash
docker-compose ps
```

## Configuration

The docker-compose.yml file includes:

1. **gps-tracker service**:
   - Builds from the main Dockerfile
   - Exposes TCP (5000), UDP (5001), and health check (8080) ports
   - Configures environment variables for v2.0 features
   - Mounts data and logs volumes
   - Depends on Redis service

2. **redis service**:
   - Uses Redis 7 Alpine image
   - Exposes port 6379
   - Persists data in a named volume
   - Uses restart policy for high availability

## Environment Variables

The example uses the following environment variables for v2.0 features:

- `FLEET_DASHBOARD_ROUTE_OPTIMIZATION=true` - Enables route optimization
- `FLEET_DASHBOARD_FUEL_TRACKING=true` - Enables fuel tracking
- `FLEET_DASHBOARD_UPDATE_INTERVAL=30` - Sets analytics update interval to 30 seconds

## Volumes

The example uses Docker named volumes for persistence:
- Application data in `./data` directory
- Application logs in `./logs` directory
- Redis data in `redis-data` volume

## Networking

Services are connected through a custom bridge network named `gps-network`.

## Scaling

For production deployments, you can scale the gps-tracker service:
```bash
docker-compose up -d --scale gps-tracker=3
```

## Monitoring

You can monitor the services using:
```bash
docker-compose logs -f
```

## Cleanup

To stop and remove containers:
```bash
docker-compose down
```

To remove volumes as well:
```bash
docker-compose down -v
```