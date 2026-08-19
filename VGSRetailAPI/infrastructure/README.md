# Infrastructure Structure

This directory contains infrastructure configurations, Docker compose templates, and environment definitions for VGS Retail OS.

## Structure
- `compose/`: Docker Compose definitions (`docker-compose.dev.yml`) for local development infrastructure.
- `env/`: Environment variable templates (`.env.example`) and environment documentation.
- `docker/`: Dockerfiles for build stages (`dev/`, `test/`, `staging/`, `prod/`).
- `monitoring/`: Monitoring dashboard definitions, log pipelines, and alert configs.
