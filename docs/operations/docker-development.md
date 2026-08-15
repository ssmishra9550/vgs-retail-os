# Docker Development Guide

## 1. Overview
VGS Retail OS provides containerized local development infrastructure for PostgreSQL and Redis to guarantee reproducibility across development workstations and CI environments.

## 2. Infrastructure Services
- **PostgreSQL 17 (`postgres:17.4-alpine`):** Pinned multi-arch (arm64/amd64) database server.
- **Redis 7 (`redis:7.4.2-alpine`):** Pinned multi-arch (arm64/amd64) in-memory cache with AOF persistence.

## 3. Usage Guide

### Starting Services
```bash
docker compose --env-file infrastructure/env/.env.example -f infrastructure/compose/docker-compose.dev.yml up -d
```

### Checking Health
```bash
docker compose --env-file infrastructure/env/.env.example -f infrastructure/compose/docker-compose.dev.yml ps
```

### Stopping Services
```bash
docker compose --env-file infrastructure/env/.env.example -f infrastructure/compose/docker-compose.dev.yml down
```

## 4. Data Persistence & Reset Rules
- **Normal Workflow:** Using `docker compose down` removes containers while preserving the named volume `vgs-postgres-dev-data`.
- **Database Reset:** Running `docker compose down -v` permanently removes the volume, allowing fresh database initialization when needed.
