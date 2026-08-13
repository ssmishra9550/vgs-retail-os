# ARCHITECTURE.md

## 1) Architecture Baseline
- **Style:** Modular Monolith (initial and mandatory).
- **Goal:** Reliable retail core for VGS pilot with clean boundaries for future SaaS evolution.
- **Reasoning:** Lower complexity, easier deployment/debugging, faster MVP delivery, clearer domain ownership.

## 2) Technology Stack (Blueprint-Defined)
- **Frontend:** Angular (PWA-first).
- **Backend:** ASP.NET Core Web API.
- **Database:** PostgreSQL.
- **Cache:** Redis.
- **Background Processing:** .NET Worker Services / Hangfire-style jobs.
- **Containerization:** Docker.
- **Source Control:** Git + GitHub.
- **AI Development Assist:** Claude Code + GitHub Copilot.
- **Cloud Direction:** Cloud-ready, cloud-vendor neutral.

## 3) High-Level Runtime Flow
1. User interacts with Angular/PWA.
2. Requests pass through API gateway/app layer in ASP.NET Core.
3. Domain modules process business logic with strict module boundaries.
4. Persistent state in PostgreSQL; hot/shared transient data in Redis.
5. Asynchronous tasks handled by background workers.
6. Notifications and integrations executed via controlled adapters.

## 4) Core Domain Modules (Initial Product Map)
- Authentication
- Tenant Management
- Organization
- Store Management
- User Management
- Roles & Permissions
- Product/Category/Brand/Unit/Tax
- Customer Management
- Supplier Management
- Purchase & Purchase Return
- Inventory, Stock Ledger, Stock Transfer, Stock Adjustment
- POS, Sales, Billing, Invoice, Sales Return
- Payment, Receivables, Payables, Expenses
- Notifications, Dashboard, Reports, Audit Logs, Settings
- Subscription/SaaS, Integrations, Automation, AI Insights

## 5) Multi-Tenant Model
- One platform serves multiple independent businesses (tenants).
- Hierarchy: `Tenant -> Organization -> Store -> User`.
- Access control: `User -> Role -> Permission`.
- Tenant data isolation is mandatory in API, data, jobs, reports, and caching.

## 6) Data Architecture Principles
- PostgreSQL is the system-of-record.
- Inventory truth is transaction/ledger-driven; every stock change must have a reason/event trail.
- Use CRUD for low-risk/master data (e.g., categories/brands/config).
- Introduce Event Sourcing/CQRS selectively in later phases for critical domains (inventory/sales/payments/transfers/returns) per blueprint.

## 7) API Architecture Principles
- Base style: REST with versioning (`/api/v1/...`).
- Core endpoint domains include auth, tenants, stores, products, customers, suppliers, purchases, inventory, transfers, sales, POS, payments, expenses, reports, notifications, subscriptions.
- Advanced future endpoint domains include events/sync/risk/workflows/b2b/channels/orders/reservations.
- All endpoints must enforce authn/authz, validation, tenant isolation, auditability; advanced flows also require idempotency and correlation IDs.

## 8) Frontend Architecture Principles
- Angular app shell with core/shared + feature modules.
- Feature routing with guards and interceptors.
- Reusable forms/tables/notifications components.
- Responsive UX across desktop/tablet/mobile.
- PWA foundation for later offline-first rollout.

## 9) Backend Architecture Principles
- Vertical feature modules (not layered by technical type only).
- Shared cross-cutting concerns in common libraries (auth, validation, errors, auditing, tenancy context).
- Command paths for writes; read paths optimized for operational dashboards/reports.
- Background jobs for non-blocking tasks (notifications, projections, reporting tasks).

## 10) Security Baseline
- Security from version 1: authentication, RBAC, tenant isolation, secure transport, secure secret handling, input validation, audit logging, rate limiting, backup/recovery readiness.
- Advanced phases add stricter controls for offline devices, sync signing, event immutability, sensitive action approvals, risk data access controls.

## 11) Offline Strategy (Phased)
- Not blanket-enabled in MVP.
- Start online-first.
- Introduce controlled offline for selected POS operations after conflict rules, idempotency, reconciliation, and device policies are fully defined.

## 12) Evolution Principle
> Build a simple and reliable core now; preserve boundaries so Event Sourcing, CQRS, Offline Sync, B2B, Omnichannel, Risk, Automation, and AI can be added without product rewrite.

