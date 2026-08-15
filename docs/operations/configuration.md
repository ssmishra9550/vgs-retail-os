# Configuration and Environment Strategy

## 1. Overview
VGS Retail OS follows standard Twelve-Factor app configuration principles, keeping environment-specific settings and sensitive credentials decoupled from application code and source control.

## 2. Environment Names
The platform recognizes standard environment stages:
- **`Development`**: Local developer machines and dev containers.
- **`Testing`**: Automated test runs, CI pipelines, and unit/integration test suites.
- **`Staging`**: Pre-production staging environment matching production topology.
- **`Production`**: Live multi-tenant SaaS environment for VGS stores and SaaS tenants.

## 3. Backend Configuration Hierarchy (ASP.NET Core)
Configuration values are resolved in the following order of precedence (later sources override earlier ones):

1. `appsettings.json` (Base defaults, schema structure, safe placeholders)
2. `appsettings.{Environment}.json` (Environment-specific overrides e.g. `appsettings.Development.json`, `appsettings.Testing.json`)
3. `appsettings.local.json` / `appsettings.{Environment}.local.json` (Local developer overrides, ignored by Git)
4. Environment Variables (`ConnectionStrings__DefaultConnection`, `ASPNETCORE_*`, etc.)
5. Secret Managers / Cloud Key Vaults (for staging and production deployments)

## 4. Frontend Configuration (Angular)
Angular environment files under `frontend/src/environments/` provide client-side constants:
- `environment.ts`: Default/production configuration.
- `environment.development.ts`: Local development configuration.

> **Crucial Rule:** Frontend environment files are packaged into public client-side JavaScript bundles. **Never store passwords, private API keys, or secret tokens in Angular environment files.**

## 5. Secret Handling and Safe Storage
- **Source Control Policy:** No real passwords, database credentials, API keys, or JWT private keys may be committed to Git.
- **Local Development:** Use `.env.example` as a template to generate a local `.env` or use .NET User Secrets (`dotnet user-secrets`).
- **Production & Staging:** Injected securely via container environment variables or managed secret stores (e.g., HashiCorp Vault, AWS Secrets Manager, Azure Key Vault, GCP Secret Manager).
