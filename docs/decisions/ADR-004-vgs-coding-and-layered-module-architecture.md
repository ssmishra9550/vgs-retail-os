# ADR-004: VGS Coding & Layered Module Architecture

## Status
Accepted

## Date
2026-08-15

## Context
VGS Retail OS is a business operating system designed for independent retail enterprises. As the project transitions from foundational infrastructure into core business module development, a clear, predictable, and robust backend and frontend architectural model is required. 

Without explicit architectural boundaries, applications risk developing global mega-layers, leaky abstractions, circular dependencies, mixed persistence/presentation concerns, or fragmented data-access mechanisms. This ADR defines the mandatory coding, layering, naming, and dependency standards for all human developers and AI development agents working on VGS Retail OS.

---

## Decision

### 1. Module-First Backend Architecture
VGS Retail OS follows a **module-first (vertical slice/domain-owned)** architecture. Business capabilities are isolated into self-contained business modules such as:
- Auth & Identity
- Tenant Management
- Organization & Store
- User & RBAC
- Audit
- Product Master (Categories, Brands, Units, Taxes)
- Inventory Management
- Purchase & Supplier Operations
- Sales & POS Operations
- Customer Management
- Payments & Financials

**Forbidden:** Global mega-layers (such as a root `BL/`, `BO/`, or `DAC/` directory containing all application domain code). Instead, each module owns its own internal layers and domain logic.

---

### 2. Module Layering Standard
Each applicable business module follows a layered structure with a canonical linear dependency direction:

```
[ API Layer ]         Controllers, HTTP DTOs, Routing, Versioning
      ↓
[ IBL Layer ]         Business Layer Interfaces
      ↓
[ BL Layer ]          Business Logic & Workflows
      ↓
[ IDAC Layer ]        Data Access Component Interfaces
      ↓
[ DAC Layer ]         Persistence Implementation (EF Core 10)
      ↓
[ Database / Infra ]  PostgreSQL 17 / Redis
```

*Note on BO (Business Objects):* **BO** represents framework-independent business/domain objects used across appropriate application and domain boundaries (`IBL`, `BL`, `IDAC`, `DAC`). `BO` is a domain model boundary and is **NOT** part of the linear execution dependency chain.

Canonical dependency direction:
`API → IBL → BL → IDAC → DAC → EF Core 10 → PostgreSQL 17`

---

### 3. API Layer
- **Responsibilities:** HTTP endpoint hosting, route mapping, request validation, transport DTO serialization/deserialization, HTTP status code translation, and API versioning (`/api/v1/...`).
- **Abstractions:** Controllers MUST depend on `IBL` interfaces.
- **Forbidden:** Controllers MUST NOT directly access `DAC`, `DbContext`, or `EF Core`.

---

### 4. IBL (Interface Business Layer) Layer
- **Responsibilities:** Defines the business contract for the module (e.g., `IProductBL`, `IInventoryBL`, `ISalesBL`).
- **Usage:** Decouples the presentation/API layer from concrete business logic implementations, enabling dependency injection, testability, and mockability.

---

### 5. BL (Business Logic) Layer
- **Responsibilities:** Core business rules execution, use-case orchestration, business validation, domain event dispatching (when applicable), and transaction orchestration.
- **Examples:** `ProductBL`, `InventoryBL`, `SalesBL`.
- **Forbidden:** `BL` MUST NOT directly depend on ASP.NET Core controllers/HTTP contexts, `DbContext`, `EF Core`, raw SQL, PostgreSQL drivers, or Redis clients. `BL` depends on `IDAC` abstractions for persistence operations.

---

### 6. BO (Business Object) Layer
- **Responsibilities:** Domain and business entities representing real business concepts with behavior and state (e.g., `Product`, `ProductPrice`, `ProductVariant`, `ProductTaxProfile`, `Customer`, `CustomerAddress`, `SalesOrder`, `SalesOrderLine`, `InventoryItem`, `StockLedgerEntry`, `Payment`, `Invoice`).
- **Naming Rule:** Use direct, natural business names (`Product`, `Customer`, `SalesOrder`). Avoid artificial suffixes like `ProductBO`, `CustomerBO`, or `SalesBO` unless there is a genuine business domain reason.
- **Forbidden:** `BO` objects must remain completely POCO (Plain Old C# Objects) and independent of ASP.NET Core, EF Core, PostgreSQL, Redis, or HTTP packages.

---

### 7. IDAC (Interface Data Access Component) Layer
- **Responsibilities:** Module-specific data-access interfaces defined from the perspective of application business needs (e.g., `IProductDAC`, `IInventoryDAC`, `ISalesDAC`).
- **Ownership:** Belongs conceptually to the application domain contract. `BL` depends on `IDAC`, while `DAC` implements `IDAC`.

---

### 8. DAC (Data Access Component) Layer
- **Responsibilities:** Persistence operations executing against PostgreSQL via EF Core 10 (e.g., `ProductDAC`, `InventoryDAC`, `SalesDAC`).
- **Forbidden:** `DAC` MUST NOT contain business rule validation, workflow logic, or HTTP/controller concerns.

---

### 9. Database Access Technology
- **Standard:** **EF Core 10 + Npgsql + PostgreSQL 17** is the sole approved persistence technology stack for VGS Retail OS.
- **Forbidden:** ADO.NET and Dapper are **NOT** part of the baseline architecture and must not be introduced without an explicit, approved Architecture Decision Record (ADR).

---

### 10. BO vs. EF Entity Separation
- Business Objects (`BO`) represent domain state and logic. EF Core persistence entities represent database schema layout and relationships.
- Persistence entities must not be leaked as API contracts or directly mutated outside the persistence boundary. Mapping occurs at the `DAC` / `BL` boundary as appropriate.

---

### 11. DTO vs. BO Separation
- Request/Response DTOs are transport contracts tailored to external client endpoints.
- Business Objects (`BO`) are domain entities. DTOs and BOs must remain distinct objects to allow independent API evolution without breaking domain models.

---

### 12. Async/Await Standard
- All I/O-bound database and external system operations must use asynchronous programming (`async`/`await`).
- EF Core queries and commands must use async methods (e.g., `ToListAsync`, `FirstOrDefaultAsync`, `SingleOrDefaultAsync`, `AnyAsync`, `SaveChangesAsync`).
- **Forbidden:** Creating synchronous wrappers around async methods (`.Result`, `.Wait()`) or creating fake async methods over synchronous CPU operations (`Task.FromResult` wrappers over non-I/O calls).

---

### 13. CancellationToken Standard
- All I/O-bound asynchronous methods must accept a `CancellationToken`.
- Cancellation tokens must flow cleanly through all architectural layers:  
  `API Controller → IBL → BL → IDAC → DAC → EF Core`

---

### 14. Exclusion of Generic Repository
- `IGenericRepository<T>` and `GenericRepository<T>` are **EXPLICITLY FORBIDDEN**.
- Data access interfaces must be domain-tailored and module-specific (`IProductDAC`, `IInventoryDAC`, `ISalesDAC`) to avoid leaky abstractions and anti-patterns.

---

### 15. Cross-Module Boundaries
- Modules must respect strict encapsulation boundaries.
- **Forbidden:** A module MUST NOT directly access another module's `DAC`, `DbContext`, or persistence layer (e.g., `SalesBL` directly calling `InventoryDAC` is prohibited).
- Cross-module operations must occur through public module contracts or application services.

---

### 16. Transaction Boundaries
- Database transactions must align with business use-case boundaries.
- Individual `DAC` methods must not open arbitrary independent database transactions. Explicit unit-of-work / transaction orchestration belongs at the `BL` layer for multi-operation workflows.

---

### 17. Shared Code Rules
- `VGS.RetailOS.Shared` / `BuildingBlocks` is reserved for genuinely cross-cutting, domain-agnostic technical infrastructure (e.g., base exception types, logging middleware, tenant context resolution, health check handlers).
- Business-specific domain logic, DTOs, or models must remain inside their respective business modules.

---

### 18. Frontend Architecture Standard
- The Angular frontend follows a **feature-first** architecture matching backend domain boundaries.
- A feature module layout contains:
  - `pages/`
  - `components/`
  - `services/`
  - `models/`
  - `state/`
  - `validators/`
  - `routes.ts`
  - `constants/`
- Feature-specific logic remains inside the feature directory. `Core` contains app-wide infrastructure (guards, interceptors, auth tokens), and `Shared` contains only reusable UI primitives (buttons, dialogs, form controls).

---

### 19. Naming Conventions
- Names must communicate both module identity and architectural responsibility:
  - `ProductController`
  - `IProductBL` / `ProductBL`
  - `IProductDAC` / `ProductDAC`
- Avoid generic, ambiguous names like `Manager`, `Service`, `Helper`, or `Repository` when they obscure exact responsibility.

---

### 20. Strict Dependency Matrix

| Layer | Allowed Dependencies | Forbidden Dependencies |
| :--- | :--- | :--- |
| **API / Controller** | `IBL`, Transport DTOs, ASP.NET Core | `BL` (concrete), `IDAC`, `DAC`, `DbContext`, EF Core |
| **IBL** | `BO`, Application Contracts | ASP.NET Core, `DAC`, `DbContext`, EF Core |
| **BL** | `IBL`, `IDAC`, `BO`, Shared Building Blocks | `DbContext`, EF Core, PostgreSQL, Redis Client, Controllers, HTTP Context |
| **BO** | None (Pure C# POCOs) | ASP.NET Core, EF Core, PostgreSQL, Redis, HTTP |
| **IDAC** | `BO`, Domain Types | `DAC` (concrete), `DbContext`, EF Core |
| **DAC** | `IDAC`, `BO`, EF Core `DbContext`, Npgsql | Controllers, HTTP Contexts, Business Rule Validation |

---

### 21. AI Agent Development Directives
Any AI coding agent working in this repository MUST strictly follow these rules:
1. Read relevant architecture documentation before writing code.
2. Enforce module boundaries strictly.
3. Follow the `API → IBL → BL → IDAC → DAC` dependency direction without exception.
4. Never bypass `IBL` or `IDAC` interfaces.
5. Never inject or reference `DbContext` or `EF Core` in `BL` or `Controllers`.
6. Never reference `DAC` implementations directly in `API` controllers.
7. Never introduce ADO.NET or Dapper without an explicit approved ADR.
8. Never introduce `GenericRepository<T>` or `IGenericRepository<T>`.
9. Never introduce premature abstractions or unneeded complexity.
10. Preserve existing codebase patterns and architecture standards.
11. Run `dotnet build` and relevant tests after implementation to verify changes.
12. Report architectural conflicts immediately instead of silently altering rules.
13. Do not invent custom architectural patterns outside this ADR.

---

### 22. Future Extensibility (Phased Capabilities)
The architecture is designed to support future evolution into:
- CQRS & Read-Optimized Projections
- Domain Events & Outbox Pattern
- Integration Events & Async Messaging
- Background Processing & Worker Services
- Offline Synchronization (Phased POS sync)
- AI & Automation Insights

*Rule:* **DO NOT** implement these advanced patterns prematurely. They must be introduced only when explicit roadmap tasks specify them.

---

## Architectural Principles Summary
- **Module-first & domain-encapsulated**
- **Layered with explicit dependency inversion**
- **Async-first & cancellation-aware**
- **Multi-tenant safe by design**
- **EF Core 10 standard persistence**
- **AI-agent compliant and deterministic**

---

## Consequences

### Benefits
- **Strict Boundary Integrity:** Prevents spaghetti code and tight coupling between HTTP endpoints and persistence.
- **High Testability:** Every layer can be independently unit-tested or mock-tested via interfaces (`IBL`, `IDAC`).
- **AI-Agent Safety:** Explicit, predictable patterns prevent AI agents from generating invalid architecture or leaking data.
- **Maintainability & Evolution:** Modules can evolve, refactor, or transition persistence strategies independently.

### Trade-offs
- **Boilerplate & Files:** Requires more interfaces and files per module slice compared to monolithic scripts.
- **Mapping Overhead:** Requires mapping between DTOs, BOs, and EF Entities at application boundaries.

---

## Rejected Alternatives

1. **Global BL/BO/DAC Mega-Layers:** Rejected because global technical layers create cross-domain coupling and violate modular monolith boundaries.
2. **Generic Repository (`IGenericRepository<T>`):** Rejected due to leaky query abstractions, poor domain expressiveness, and impedance mismatch with EF Core.
3. **Direct Controller → DAC or BL → DbContext:** Rejected because bypassing layer abstractions violates separation of concerns and ruins unit testability.
4. **ADO.NET / Dapper as Baseline:** Rejected to prevent manual SQL string fragmentation and maintain centralized EF Core multi-tenant query filtering and security.
5. **Premature CQRS / Event Sourcing:** Rejected for MVP baseline to avoid unnecessary initial complexity before core operational stability.

---

## Related Documentation
- `docs/decisions/ADR-001-dotnet-version.md`
- `docs/decisions/ADR-002-angular-frontend-stack.md`
- `docs/decisions/ADR-003-database-data-access-strategy.md`
- `ARCHITECTURE.md`
- `PROJECT_RULES.md`
- `IMPLEMENTATION_PLAN.md`
