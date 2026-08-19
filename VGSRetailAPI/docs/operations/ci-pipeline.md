# Continuous Integration (CI) Pipeline

## Overview

VGS Retail OS uses GitHub Actions for automated Continuous Integration (CI) validation on code changes.

## Workflow Triggers

The CI pipeline runs automatically on:
- **Pull Requests** targeting the `main` branch.
- **Pushes** directly to the `main` branch.

## CI Validation Checks

### 1. Backend CI Job
- **Environment**: Pinned .NET 10 SDK (configured via `backend/global.json`), PostgreSQL 17, Redis 7.
- **Steps**:
  1. `Backend Restore`: Restores NuGet dependencies.
  2. `Backend Build`: Compiles the solution (`VGS.RetailOS.sln`).
  3. `Backend Tests`: Executes unit, integration, architecture, security, performance, and API tests against live PostgreSQL and Redis service containers.

### 2. Frontend CI Job
- **Environment**: Node.js 24 LTS.
- **Steps**:
  1. `Frontend Install`: Installs npm packages via `npm ci`.
  2. `Frontend Build`: Builds the Angular application (`npm run build`).
  3. `Frontend Tests`: Runs frontend unit/component tests (`npm run test -- --watch=false`).

## Replicating CI Locally

When CI fails, developers should reproduce and debug the failure locally using the following commands:

### Local Backend Verification
Ensure Docker containers for PostgreSQL and Redis are running:
```bash
docker compose --env-file infrastructure/env/.env.example -f infrastructure/compose/docker-compose.dev.yml up -d
```

Run restore, build, and test:
```bash
dotnet restore backend/VGS.RetailOS.sln
dotnet build backend/VGS.RetailOS.sln
dotnet test backend/VGS.RetailOS.sln
```

### Local Frontend Verification
```bash
cd frontend
npm ci
npm run build
npm run test -- --watch=false
```

## Security & Secrets

- CI operates under **least privilege** (`permissions: contents: read`).
- Database and cache credentials used during integration tests are temporary, non-production test defaults. No production secrets or tokens are stored or used in CI.
