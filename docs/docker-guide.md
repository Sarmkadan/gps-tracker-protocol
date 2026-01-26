# Docker Guide for GPS Tracker Protocol

## Quick Start with Docker

### Prerequisites
- Docker Engine 20.10+
- Docker Compose v2+

### Running with Docker Compose
```bash
# Clone the repository
git clone https://github.com/your-org/gps-tracker-protocol.git
cd gps-tracker-protocol

# Start the services
docker-compose up -d

# Verify the services are running
docker-compose ps
```

### Running with Docker Directly
```bash
# Build the image
docker build -t gps-tracker-protocol .

# Run the container
docker run -d \
  --name gps-tracker \
  -p 5000:5000 \
  -p 5001:5001 \
  -p 8080:8080 \
  -v $(pwd)/data:/app/data \
  -v $(pwd)/logs:/app/logs \
  -e GPS_PROTOCOL_DEFAULT=GT06 \
  -e GPS_MAX_DEVICES=10000 \
  -e GPS_CACHE_EXPIRATION=60 \
  -e GPS_LOG_LEVEL=Information \
  -e GPS_RATE_LIMIT=1000 \
  gps-tracker-protocol
```

## Docker Compose Usage

### Services Overview
The docker-compose.yml defines the following services:

1. **gps-tracker**: Main application service
2. **redis**: Caching layer (required)
3. **prometheus** (optional): Metrics collection
4. **grafana** (optional): Metrics visualization

### Environment Variables Reference

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| GPS_PROTOCOL_DEFAULT | Default GPS protocol to use | GT06 | No |
| GPS_MAX_DEVICES | Maximum number of tracked devices | 10000 | No |
| GPS_CACHE_EXPIRATION | Cache expiration time in seconds | 60 | No |
| GPS_LOG_LEVEL | Logging level (Trace, Debug, Information, Warning, Error, Critical, None) | Information | No |
| GPS_RATE_LIMIT | Maximum requests per second per IP | 1000 | No |

### Volume Mounts
- `./data:/app/data`: Persistent storage for application data
- `./logs:/app/logs`: Application logs
- Redis, Prometheus, and Grafana data volumes for persistence

### Ports Exposed
- **5000**: TCP listener for GPS devices
- **5001**: UDP listener for GPS devices
- **8080**: Health check endpoint
- **9090**: Prometheus metrics (when enabled)
- **3000**: Grafana dashboard (when enabled)

## Production Deployment Checklist

### Pre-deployment
- [ ] Review and adjust environment variables for production
- [ ] Configure proper logging levels (avoid Debug/Trace in production)
- [ ] Set appropriate rate limits based on expected load
- [ ] Ensure sufficient resources allocated to containers
- [ ] Configure proper restart policies

### Security Considerations
- [ ] Use non-root user in Dockerfile for production
- [ ] Scan images for vulnerabilities regularly
- [ ] Implement network segmentation
- [ ] Use secrets management for sensitive configuration
- [ ] Enable HTTPS termination at reverse proxy level

### Monitoring and Maintenance
- [ ] Configure health checks (already included)
- [ ] Set up log aggregation and rotation
- [ ] Monitor container resource usage
- [ ] Regularly update base images
- [ ] Backup persistent volumes regularly

### Scaling Considerations
- [ ] For high availability, consider running multiple instances behind a load balancer
- [ ] Redis can be scaled using Redis Cluster for larger deployments
- [ ] Monitor database connection pool sizes if using persistent storage
- [ ] Consider using managed Redis services for production

## Troubleshooting

### Common Issues
1. **Container fails to start**
   - Check logs: `docker-compose logs gps-tracker`
   - Verify port availability (5000, 5001, 8080)
   - Ensure required environment variables are set

2. **GPS devices cannot connect**
   - Verify firewall rules allow traffic on ports 5000/5001
   - Check that the service is listening: `docker-compose exec gps-tracker netstat -tlnp`
   - Verify protocol configuration matches device expectations

3. **Performance issues**
   - Monitor Redis memory usage
   - Check CPU and memory limits
   - Review rate limiting settings
   - Consider increasing worker threads if applicable

### Logs and Debugging
```bash
# View application logs
docker-compose logs -f gps-tracker

# View Redis logs
docker-compose logs -f redis

# Execute commands in running container
docker-compose exec gps-tracker /bin/bash

# Inspect container resources
docker stats gps-tracker-protocol
```

## Development Workflow

### Local Development with Docker
```bash
# Start services for development
docker-compose up -d

# Rebuild after code changes
docker-compose build gps-tracker
docker-compose up -d gps-tracker

# Run tests inside container
docker-compose exec gps-tracker dotnet test
```

### Multi-stage Dockerfile Explanation
The Dockerfile uses multi-stage builds:
1. **Builder stage**: Uses .NET SDK to compile and publish the application
2. **Runtime stage**: Uses .NET runtime only, resulting in smaller final image
3. **Dependencies**: Installs curl for healthcheck functionality
4. **Configuration**: Sets environment variables for production readiness
5. **Entrypoint**: Runs the published application

## Customization

### Modifying the Dockerfile
To add additional dependencies or modify the build process:
1. Edit the Dockerfile in the repository root
2. Rebuild the image: `docker-compose build`
3. Restart services: `docker-compose up -d`

### Extending docker-compose.yml
To add additional services:
1. Add new service definition to docker-compose.yml
2. Configure necessary networks, volumes, and dependencies
3. Update environment variables as needed
4. Apply changes: `docker-compose up -d`

## References
- Docker Documentation: https://docs.docker.com/
- Docker Compose Reference: https://docs.docker.com/compose/
- .NET Docker Samples: https://github.com/dotnet/dotnet-docker