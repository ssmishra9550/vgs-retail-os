# ADR-003: Database Data Access Strategy

## Status
Accepted

## Date
2025-02-18

## Context
VGS Retail OS is a multi-tenant SaaS application that requires robust, auditable, and secure data access. As a .NET 10 application using PostgreSQL 17, we need to select the primary data-access and schema migration mechanism. We evaluated options such as Entity Framework Core (EF Core), Dapper, and a hybrid approach. The priority is ensuring strict tenant isolation, minimizing data leakage risks, supporting complex domain modeling, and providing a maintainable migration strategy.

## Requirements
- Support PostgreSQL 17 features.
- Provide strong multi-tenant isolation mechanisms.
- Support auditability for critical entity changes.
- Offer a robust and trackable schema migration system.
- Align with AI-assisted development (strong typing, predictable patterns).
- Support modular monolith boundaries.

## Options Considered

### Option 1: Entity Framework Core 10 (EF Core) + Npgsql
- **Pros:** Native .NET ORM, supports Global Query Filters (critical for multi-tenancy), strong typing for compile-time safety and AI context, built-in migration tooling (`dotnet ef`), excellent LINQ support for complex queries, supports shadow properties for auditing.
- **Cons:** Can be slower than micro-ORMs for extremely high-throughput read operations if not optimized (e.g., using `AsNoTracking` or compiled queries).
- **Evaluation:** Best fit for enforcing multi-tenancy and audit rules centrally.

### Option 2: Dapper
- **Pros:** High performance, raw SQL control.
- **Cons:** Requires manual implementation of tenant isolation in every query (high risk of human error), no built-in schema migration tool, less context for AI assistants due to string-based SQL.
- **Evaluation:** High risk for multi-tenant data leakage. Not suitable as the primary ORM.

### Option 3: Hybrid (EF Core for Writes + Dapper for Reads)
- **Pros:** Combines EF Core's structured writes with Dapper's fast reads.
- **Cons:** Increases complexity, fragments data access patterns, requires duplicating tenant isolation logic in Dapper queries, higher maintenance overhead.
- **Evaluation:** Unnecessary complexity for the baseline architecture. EF Core's read performance (with `.AsNoTracking()`) is sufficient for the majority of use cases.

## Decision
VGS Retail OS will use **Entity Framework Core 10** with the **Npgsql.EntityFrameworkCore.PostgreSQL** provider as the sole primary data-access and schema migration framework.

- EF Core Global Query Filters will be used to enforce tenant isolation at the database level.
- EF Core Interceptors or overridden `SaveChanges` will be used for audit trailing.
- EF Core Migrations will be used to manage schema changes, generating idempotent SQL scripts for production deployment.
- Dapper and hybrid approaches are explicitly excluded from the baseline architecture.

## Consequences
- **Positive:** Centralized and reliable enforcement of tenant isolation and auditing. Strong typing improves developer productivity and AI assistance. Unified schema management.
- **Negative:** Developers must be mindful of EF Core performance pitfalls (N+1 queries, tracking overhead) and use techniques like `.AsNoTracking()` appropriately.
- **Constraint:** AI agents and developers must not introduce Dapper or bypass EF Core without an explicit architectural review and a new ADR.
