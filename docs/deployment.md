# Deployment Guide

Production deployment strategies and best practices.

## Docker Deployment

### Dockerfile

Build and run in containers:

```bash
# Build image
docker build -t gps-tracker-protocol:latest .

# Run container
docker run -it -p 5000:5000 -p 5001:5001 gps-tracker-protocol:latest

# Run with volume for persistent data
docker run -it -v $(pwd)/data:/app/data gps-tracker-protocol:latest

# Run with environment variables
docker run -it \
  -e GPS_PROTOCOL_DEFAULT=GT06 \
  -e GPS_MAX_DEVICES=10000 \
  -e GPS_LOG_LEVEL=Information \
  gps-tracker-protocol:latest
```

### Docker Compose (Multiple Services)

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f gps-tracker

# Stop services
docker-compose down

# Clean up volumes
docker-compose down -v
```

---

## Kubernetes Deployment

### Deployment Manifest

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: gps-tracker-protocol
  namespace: default
spec:
  replicas: 3
  selector:
    matchLabels:
      app: gps-tracker
  template:
    metadata:
      labels:
        app: gps-tracker
    spec:
      containers:
      - name: gps-tracker
        image: gps-tracker-protocol:1.0.0
        ports:
        - containerPort: 5000
          name: tcp
        - containerPort: 5001
          name: udp
        env:
        - name: GPS_MAX_DEVICES
          value: "10000"
        - name: GPS_LOG_LEVEL
          value: "Information"
        resources:
          requests:
            memory: "256Mi"
            cpu: "100m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
```

### Service Manifest

```yaml
apiVersion: v1
kind: Service
metadata:
  name: gps-tracker-service
spec:
  type: LoadBalancer
  selector:
    app: gps-tracker
  ports:
  - name: tcp
    port: 5000
    targetPort: 5000
    protocol: TCP
  - name: udp
    port: 5001
    targetPort: 5001
    protocol: UDP
```

### Deploy to Kubernetes

```bash
# Create namespace
kubectl create namespace gps-tracker

# Apply manifests
kubectl apply -f deployment.yaml -n gps-tracker
kubectl apply -f service.yaml -n gps-tracker

# Check status
kubectl get pods -n gps-tracker
kubectl get svc -n gps-tracker

# View logs
kubectl logs deployment/gps-tracker-protocol -n gps-tracker

# Scale deployment
kubectl scale deployment gps-tracker-protocol --replicas=5 -n gps-tracker
```

---

## Linux/Windows Service Installation

### Windows Service

Create `install-service.bat`:

```batch
@echo off
sc create GpsTrackerProtocol ^
  binPath= "C:\Apps\GpsTrackerProtocol\bin\Release\net10.0\GpsTrackerProtocol.exe" ^
  start= auto ^
  displayName= "GPS Tracker Protocol Parser"

sc start GpsTrackerProtocol
```

Remove service:
```batch
sc stop GpsTrackerProtocol
sc delete GpsTrackerProtocol
```

### Linux Systemd Service

Create `/etc/systemd/system/gps-tracker.service`:

```ini
[Unit]
Description=GPS Tracker Protocol Parser
After=network.target

[Service]
Type=simple
User=gps-tracker
WorkingDirectory=/opt/gps-tracker
ExecStart=/opt/gps-tracker/GpsTrackerProtocol
Restart=always
RestartSec=10
StandardOutput=append:/var/log/gps-tracker/output.log
StandardError=append:/var/log/gps-tracker/error.log

[Install]
WantedBy=multi-user.target
```

Enable and start:
```bash
sudo systemctl daemon-reload
sudo systemctl enable gps-tracker
sudo systemctl start gps-tracker
sudo systemctl status gps-tracker
```

---

## Performance Tuning

### .NET Runtime Configuration

Create `runtimeconfig.json`:

```json
{
  "runtimeOptions": {
    "tfm": "net10.0",
    "framework": {
      "name": "Microsoft.NETCore.App",
      "version": "10.0.0"
    },
    "configProperties": {
      "System.GC.Server": true,
      "System.GC.Concurrent": true,
      "System.GC.RetainVM": true,
      "System.GC.HeapCount": 4
    }
  }
}
```

### Thread Pool Configuration

In startup code:

```csharp
// Increase thread pool size for high throughput
ThreadPool.GetMinThreads(out int workerThreads, out int ioThreads);
ThreadPool.SetMinThreads(
    Math.Max(workerThreads, Environment.ProcessorCount * 2),
    Math.Max(ioThreads, Environment.ProcessorCount * 2)
);
```

### Memory Configuration

For in-process caching:

```csharp
var cacheOptions = new MemoryCacheOptions
{
    SizeLimit = 1024 * 1024 * 100,  // 100MB
    CompactionPercentage = 0.25,
    ExpirationScanFrequency = TimeSpan.FromMinutes(5)
};
services.AddMemoryCache(options =>
{
    options.SizeLimit = cacheOptions.SizeLimit;
});
```

### Network Tuning

Increase socket buffers for high-throughput TCP:

```csharp
var tcpListener = new TcpListener(IPAddress.Any, 5000);
tcpListener.Server.ReceiveBufferSize = 1024 * 1024;  // 1MB
tcpListener.Server.SendBufferSize = 1024 * 1024;     // 1MB
```

---

## Monitoring and Logging

### Structured Logging Setup

```csharp
services.AddLogging(builder =>
{
    builder
        .AddConsole()
        .AddDebug()
        .SetMinimumLevel(LogLevel.Information)
        .AddFilter("GpsTrackerProtocol", LogLevel.Debug);
});
```

### Health Checks

Implement health check endpoint:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks()
    .AddCheck<GpsTrackerHealthCheck>("gps-tracker");

var app = builder.Build();
app.MapHealthChecks("/health");
app.Run();
```

Health check implementation:

```csharp
public class GpsTrackerHealthCheck : IHealthCheck
{
    private readonly IDeviceService _deviceService;
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var devices = await _deviceService.GetAllDevicesAsync();
            return HealthCheckResult.Healthy($"{devices.Count()} devices active");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Service check failed", ex);
        }
    }
}
```

### Application Insights (Azure)

```csharp
services.AddApplicationInsightsTelemetry();
services.AddSingleton<ITelemetryModule, DependencyTrackingTelemetryModule>();
```

---

## Security Hardening

### TLS/SSL Configuration

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel(options =>
{
    options.Listen(IPAddress.Any, 5000);
    options.Listen(IPAddress.Any, 5001, listenOptions =>
    {
        listenOptions.UseHttps("certificate.pfx", "password");
    });
});
```

### Input Validation

```csharp
// Validate all incoming frames
var validationPipeline = new ValidationPipeline();
validationPipeline.AddValidator(new GpsFrameValidator());
validationPipeline.AddValidator(new LocationDataValidator());
```

### Rate Limiting

```csharp
services.AddRateLimiting(options =>
{
    options.AddFixedWindowLimiter("default", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 1000;
        opt.QueueLimit = 100;
    });
});
```

### CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowTracking",
        policy =>
        {
            policy.WithOrigins("https://yourdomain.com")
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});
```

---

## Backup and Recovery

### Database Backup (SQL Server)

```sql
BACKUP DATABASE GpsTrackerDb 
TO DISK = 'D:\Backups\GpsTrackerDb.bak'
WITH INIT, COPY_ONLY, COMPRESSION;
```

### Data Retention Policy

```csharp
// Cleanup old locations periodically
var timer = new Timer(async _ =>
{
    var cutoffDate = DateTime.UtcNow.AddDays(-90);
    await locationService.PurgeLocationsBeforeAsync(cutoffDate);
}, null, TimeSpan.Zero, TimeSpan.FromHours(6));
```

### Backup Strategy

1. **Daily backups** of location database
2. **Weekly backups** of device configurations
3. **Monthly backups** of complete system state
4. **Offsite storage** for disaster recovery

---

## Load Balancing

### Multiple Instance Setup

```bash
# Start multiple instances on different ports
dotnet run --port 5000 &
dotnet run --port 5001 &
dotnet run --port 5002 &
```

### Nginx Load Balancer Configuration

```nginx
upstream gps_tracker {
    server localhost:5000;
    server localhost:5001;
    server localhost:5002;
}

server {
    listen 80;
    server_name api.tracker.com;
    
    location / {
        proxy_pass http://gps_tracker;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }
}
```

---

## Scaling Strategies

### Horizontal Scaling

Deploy multiple instances behind load balancer:

```bash
# Deploy 5 instances
for i in {1..5}; do
    docker run -d -p 500${i}:5000 gps-tracker-protocol:latest
done
```

### Vertical Scaling

Increase per-instance resources:

```csharp
// Adjust cache size
var memoryCache = new MemoryCache(new MemoryCacheOptions
{
    SizeLimit = 1024 * 1024 * 500  // 500MB instead of 100MB
});
```

### Database Optimization

For large deployments, migrate to SQL Server:

```csharp
// Replace in-memory with EF Core + SQL Server
services.AddDbContext<GpsTrackerDbContext>(options =>
    options.UseSqlServer(connectionString)
);
```

---

## Troubleshooting Deployment

### High Memory Usage

```bash
# Monitor process
ps aux | grep GpsTrackerProtocol

# Check .NET specific stats
dotnet-counters monitor

# Enable GC logging
export DOTNET_GCLogFile=gc.log
```

### Port Already in Use

```bash
# Linux/macOS
lsof -i :5000

# Windows
netstat -ano | findstr :5000

# Kill process
kill <PID>
```

### Slow Performance

1. Enable profiling: `dotnet-trace`
2. Check GC pressure
3. Monitor network throughput
4. Review application logs
5. Scale horizontally if needed

---

## Maintenance

### Regular Tasks

- **Daily**: Check logs for errors
- **Weekly**: Verify backup completion
- **Monthly**: Review performance metrics
- **Quarterly**: Update dependencies
- **Annually**: Security audit

### Update Procedure

```bash
# 1. Pull latest code
git pull origin main

# 2. Build new version
dotnet build -c Release

# 3. Run tests
dotnet test

# 4. Stop current service
systemctl stop gps-tracker

# 5. Backup current version
cp -r /opt/gps-tracker /opt/gps-tracker.bak

# 6. Deploy new version
cp -r bin/Release/net10.0/* /opt/gps-tracker

# 7. Start service
systemctl start gps-tracker

# 8. Verify health
curl http://localhost:5000/health
```

---

## Cost Optimization

1. **Use in-memory storage** for small deployments
2. **Compress old location data** for archival
3. **Implement data retention** policies
4. **Use spot instances** on cloud platforms
5. **Right-size containers** based on load

---

## Compliance and Auditing

### Data Protection

- Encrypt data in transit (TLS)
- Encrypt sensitive data at rest
- Implement access controls
- Maintain audit logs

### GDPR Compliance

```csharp
// Right to be forgotten
public async Task DeleteDeviceDataAsync(string deviceId)
{
    await locationService.DeleteAllLocationsAsync(deviceId);
    await deviceService.UnregisterDeviceAsync(deviceId);
}
```

### Audit Logging

```csharp
logger.LogInformation(
    "Device {DeviceId} location updated by {UserId} from {IpAddress}",
    location.DeviceId,
    userId,
    ipAddress
);
```
