# Redis Caching in VGS Retail OS

## Purpose
Redis is used strictly as an **infrastructure cache** in VGS Retail OS. It is intended for ephemeral performance optimizations and must **never** be used as a system of record.

## Rules for Business Modules
- **ALLOWED**: 
  - Using `IRedisCache` (located in `VGS.RetailOS.Shared.BuildingBlocks.Caching`) to get, set, delete, and check existence of temporary cache data.
- **NOT ALLOWED**: 
  - Referencing `StackExchange.Redis` or the Redis implementation directly.
  - Relying on Redis for distributed locking, pub/sub, queues, or critical business workflows.
  - Using Redis as a persistent database (e.g. session storage, event sourcing).

## Connecting to Redis
The application obtains the connection string from `ConnectionStrings:Redis` in `appsettings.json` (or `.env` environment variables). 
The `Infrastructure` module handles dependency injection, establishing a single multiplexed connection.

## Local Development
Start the Redis container using the provided Docker Compose setup:
```bash
docker compose --env-file infrastructure/env/.env.example -f infrastructure/compose/docker-compose.dev.yml up -d redis
```
Local configuration defaults to `localhost:6379`.
