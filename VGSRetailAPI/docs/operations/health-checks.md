# Health Checks

The backend API exposes operational health-check endpoints using the standard ASP.NET Core Health Checks framework. These endpoints allow platforms (like Docker, Kubernetes, CI/CD, or uptime monitors) to determine the state of the API and its dependencies.

## Endpoints

### `/health/live` (Liveness)
Indicates whether the API process is running and able to accept incoming HTTP requests.
- **Dependencies Checked:** None (Checks itself).
- **Usage:** Docker `healthcheck` or Kubernetes `livenessProbe`. If this endpoint fails, the container/process should be restarted.

### `/health/ready` (Readiness)
Indicates whether the API is ready to serve business traffic by verifying critical external dependencies.
- **Dependencies Checked:**
  - PostgreSQL Database
  - Redis Cache
- **Usage:** Load balancer routing or Kubernetes `readinessProbe`. If this endpoint fails, the API should be temporarily removed from the load balancer rotation until dependencies recover.

## Response Format
Responses are returned in `application/json` format.

```json
{
  "status": "Healthy",
  "totalDuration": 15.2,
  "entries": {
    "postgresql": {
      "status": "Healthy",
      "description": null
    },
    "redis": {
      "status": "Healthy",
      "description": "Redis is reachable. Ping time: 2.1ms."
    }
  }
}
```

*Note: In the event of a failure, exception details, passwords, and connection strings are strictly omitted to prevent information disclosure.*
