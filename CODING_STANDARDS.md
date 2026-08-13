# CODING_STANDARDS.md

## 1) Scope and Intent
These standards govern implementation quality for VGS Retail OS while preserving blueprint-defined architecture and roadmap sequencing.

## 2) General Engineering Standards
- Keep strict modular boundaries (modular monolith, feature-vertical design).
- Prefer explicit, readable business logic over clever abstractions.
- Do not bypass domain rules with direct data updates.
- Keep tenant/store/user context explicit in all operations.
- Avoid hidden side effects; make state changes auditable.

## 3) Domain and Data Standards
- Treat PostgreSQL as source of truth.
- Every stock-changing operation must record cause/context (ledger/audit semantics).
- Use transactional integrity for multi-step operations (sale, purchase, transfer, return, payment).
- Validate command inputs before domain execution.
- Use idempotency keys where repeated submissions are possible (especially future sync/event flows).

## 4) API Standards (ASP.NET Core)
- Versioned REST routes (`/api/v1/...` baseline).
- Consistent request/response shapes and error envelopes.
- Enforce authn/authz + RBAC + tenant isolation per endpoint.
- Support pagination/filtering/sorting for list APIs.
- Include audit/correlation context in sensitive workflows.
- Do not expose internal exception details to clients.

## 5) Backend Code Organization
- Organize by feature module (Auth, Tenant, Store, Product, POS, Inventory, etc.).
- Keep shared cross-cutting concerns in shared libraries (tenancy, auth, validation, auditing).
- Separate command-side mutations from query-side read concerns where beneficial.
- Keep background job handlers deterministic and retry-safe.

## 6) Frontend Standards (Angular/PWA)
- Feature modules mirror backend domain boundaries.
- Use route guards for protected areas and interceptors for auth/error propagation.
- Build reusable UI primitives for forms, tables, notifications, and status states.
- Keep UX responsive and operationally clear for store staff and managers.
- Design PWA behaviors with explicit online/offline states (future offline phases).

## 7) Security Coding Standards
- Validate all external inputs.
- Use parameterized data access and safe ORM/query patterns.
- Secure secrets and environment configs; never hardcode secrets.
- Enforce HTTPS and secure session/token handling practices.
- Restrict privileged actions via role + permission checks.

## 8) Observability and Operations Standards
- Structured logging for business-critical operations.
- Metrics and health checks for API, DB, cache, and workers.
- Trace background workflows and retries.
- Preserve audit trails for operational and sensitive actions.

## 9) Documentation and Change Standards
- Every module implementation must include:
  - purpose and boundaries
  - dependency impacts
  - API/DB contract notes
  - test coverage scope
- Any deviation from blueprint must be explicitly documented and approved.

## 10) Anti-Patterns (Do Not Do)
- Do not start as microservices.
- Do not build all advanced capabilities in MVP.
- Do not implement offline stock-changing flows before conflict policies.
- Do not use AI outputs as direct source-of-truth mutations.
- Do not make tenant isolation optional in any layer.

