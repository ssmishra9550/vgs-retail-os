# Docker Compose Development Infrastructure

This directory contains Docker Compose definitions for local infrastructure services (PostgreSQL 17 and Redis 7).

## Prerequisites
- Docker Engine / Docker Desktop (version 24+ or newer, arm64 / amd64 compatible)
- Docker Compose v2+ / v5+

## Configuration
Services use environment variables configured in `infrastructure/env/.env.example`.
Copy `infrastructure/env/.env.example` to `.env` or pass the env file:

```bash
# Start local infrastructure
docker compose --env-file infrastructure/env/.env.example -f infrastructure/compose/docker-compose.dev.yml up -d
```

## Services & Ports
| Service | Image | Default Host Port | Container Port | Volume |
|---|---|---|---|---|
| PostgreSQL | `postgres:17.4-alpine` | `5432` (configurable via `POSTGRES_PORT`) | `5432` | `vgs-postgres-dev-data` |
| Redis | `redis:7.4.2-alpine` | `6379` (configurable via `REDIS_PORT`) | `6379` | `vgs-redis-dev-data` |

*Note: If your host machine has a native PostgreSQL service running on port 5432, set `POSTGRES_PORT=5435` (or any free port) in your `.env`.*

## Common Commands

### Start Services
```bash
docker compose --env-file infrastructure/env/.env.example -f infrastructure/compose/docker-compose.dev.yml up -d
```

### Check Container Status & Health
```bash
docker compose --env-file infrastructure/env/.env.example -f infrastructure/compose/docker-compose.dev.yml ps
```

### View Logs
```bash
docker compose --env-file infrastructure/env/.env.example -f infrastructure/compose/docker-compose.dev.yml logs -f
```

### Stop Services (Preserving Database Data)
```bash
docker compose --env-file infrastructure/env/.env.example -f infrastructure/compose/docker-compose.dev.yml down
```

### Reset / Clear Database (Destructive)
To intentionally purge local database data and re-initialize from scratch:
```bash
docker compose --env-file infrastructure/env/.env.example -f infrastructure/compose/docker-compose.dev.yml down -v
```
