# IMPLEMENTATION_PLAN.md

## Scope and guardrails
- This document is an implementation **plan only**.
- No application source code, Angular components, ASP.NET services/controllers, DB tables/migrations, or Docker runtime implementation is created here.
- Source of truth used: `VGS_Retail_OS_Master_Blueprint_Updated.md`, `AGENTS.md`, and all foundation documents.

---

## PART 1 — CURRENT REPOSITORY ANALYSIS

### 1) Current folder structure
- Repository root currently contains only documentation/planning artifacts (flat root, no app folders yet).

### 2) Existing files
- `VGS_Retail_OS_Master_Blueprint_Updated.md`
- `VGS_Retail_OS_Master_Blueprint_Updated.html`
- `AGENTS.md`
- `PROJECT_RULES.md`
- `ARCHITECTURE.md`
- `DEVELOPMENT_ROADMAP.md`
- `MODULE_DEPENDENCIES.md`
- `CODING_STANDARDS.md`
- `TESTING_STRATEGY.md`
- `AI_DEVELOPMENT_GUIDELINES.md`

### 3) Existing source code
- No Angular or ASP.NET application source code is present.

### 4) Existing configuration
- No project runtime/build configuration files detected yet (no package/solution/build/test configs in repo root).

### 5) Existing Git configuration
- Current workspace is **not a Git repository**.

### 6) Existing package/dependency configuration
- No `package.json`, lock files, `.csproj`, or `.sln` found.

### 7) Existing Docker configuration
- No `Dockerfile` or `docker-compose*.yml` found.

### 8) Existing documentation
- Master blueprint and foundation governance docs are present and consistent.

### 9) What is already complete
- Product/business/architecture blueprint.
- Agent operating rules.
- Foundation architecture, roadmap, dependency, coding, testing, and AI guardrail documents.

### 10) What is missing
- Repository scaffolding for frontend/backend/infrastructure/tests/CI/CD.
- Runtime implementations.
- Build/test/deployment pipelines.
- Operational observability stack.
- Production-grade security implementation.

---

## PART 2 — TARGET REPOSITORY STRUCTURE

> Proposed target structure (planning only; do not create yet):

```text
VGS/
  AGENTS.md
  IMPLEMENTATION_PLAN.md
  VGS_Retail_OS_Master_Blueprint_Updated.md
  VGS_Retail_OS_Master_Blueprint_Updated.html
  PROJECT_RULES.md
  ARCHITECTURE.md
  DEVELOPMENT_ROADMAP.md
  MODULE_DEPENDENCIES.md
  CODING_STANDARDS.md
  TESTING_STRATEGY.md
  AI_DEVELOPMENT_GUIDELINES.md

  docs/
    architecture/
    decisions/
    api/
    operations/

  frontend/
    package.json
    angular.json
    tsconfig*.json
    src/
      app/
        core/
          config/
          auth/
          http/
          guards/
          interceptors/
          tenancy-context/
          error-handling/
        shared/
          ui/
          forms/
          tables/
          pipes/
          directives/
          models/
          utilities/
        layout/
          shell/
          navigation/
          dashboard-layout/
        features/
          health/
          auth/
          tenant/
          organization/
          store/
          users/
          roles-permissions/
          products/
          categories/
          brands/
          units/
          tax/
          customers/
          suppliers/
          purchases/
          inventory/
          transfers/
          sales/
          pos/
          billing/
          payments/
          returns/
          expenses/
          receivables/
          payables/
          reports/
          notifications/
          settings/
          subscription/
          audit/
          automation/
          ai-insights/
        state/
          app-state/
          feature-state/
      assets/
      environments/
    tests/
      unit/
      integration/
      e2e/

  backend/
    VgsRetailOs.sln
    src/
      ApiHost/
      Shared/
        BuildingBlocks/
        Security/
        Tenancy/
        Validation/
        ErrorHandling/
        Observability/
        Contracts/
      Modules/
        Health/
        Auth/
        Tenant/
        Organization/
        Store/
        User/
        RolePermission/
        Team/                 (if validated)
        Product/
        Category/
        Brand/
        Unit/
        Tax/
        Customer/
        Supplier/
        Purchase/
        PurchaseReturn/
        Inventory/
        StockLedger/
        Transfer/
        StockAdjustment/
        POS/
        Sales/
        Billing/
        Invoice/
        Payment/
        SalesReturn/
        Expense/
        Receivable/
        Payable/
        Report/
        Dashboard/
        Notification/
        Audit/
        Settings/
        Subscription/
        Integration/
        Automation/
        AiInsights/
      Workers/
        JobRunner/
        NotificationWorker/
        ProjectionWorker/      (phase-gated)
      Infrastructure/
        Persistence.PostgreSql/
        Caching.Redis/
        Messaging/
        FileStorage/
      Contracts/
        Api/
        Events/
    tests/
      unit/
      integration/
      api/
      security/
      performance/

  database/
    migrations/
      baseline/
      module/
      projections/             (phase-gated)
    scripts/
      seed/
      validation/

  infrastructure/
    docker/
      dev/
      test/
      staging/
      prod/
    compose/
      docker-compose.dev.yml
      docker-compose.test.yml
    env/
      .env.example
    monitoring/
      dashboards/
      alerts/
      log-pipeline/

  deploy/
    ci/
      pipelines/
    cd/
      release/
      rollback/

  scripts/
    setup/
    quality/
    db/
    release/

  .github/
    workflows/
      ci.yml
      pr-checks.yml
      release.yml
```

---

## PART 3 — MODULE IMPLEMENTATION MAP

### 3.1 Core and supporting modules

| Module | Business purpose | Responsibilities | Key entities | Main commands | Main queries | API roots | DB ownership | Events produced | Events consumed | Dependencies | Frontend feature | Tests | Phase |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Health | Runtime readiness | Liveness/readiness diagnostics | HealthSnapshot | CheckHealth | GetStatus | `/api/v1/health` | Infra health tables/logs if needed | HealthChecked | - | Shared/Observability | health | API/ops | Foundation |
| Authentication | Secure access | Login/token/session flows | UserCredential/Session | Login/Refresh/Logout | GetSession | `/api/v1/auth` | Auth schema | UserAuthenticated | - | User, RolePermission | auth | unit/api/security | Foundation |
| Tenant | SaaS boundary | Tenant lifecycle/isolation context | Tenant | CreateTenant/UpdateTenant | GetTenant/ListTenants | `/api/v1/tenants` | Tenant schema | TenantCreated | - | Auth, Audit | tenant | unit/api/isolation | Foundation |
| Organization | Business unit | Org profile and settings | Organization | CreateOrg/UpdateOrg | GetOrg | `/api/v1/organizations` | Org schema | OrganizationUpdated | TenantCreated | Tenant | organization | unit/api | Foundation |
| Store | Multi-store ops | Store lifecycle/mapping | Store | CreateStore/UpdateStore | GetStore/ListStores | `/api/v1/stores` | Store schema | StoreCreated | OrganizationUpdated | Organization, Tenant | store | unit/api | Foundation |
| User | Staff access | User lifecycle and assignment | User | CreateUser/DisableUser | GetUser/ListUsers | `/api/v1/users` | User schema | UserCreated | StoreCreated | Store, RolePermission | users | unit/api/security | Foundation |
| RolePermission | Authorization | RBAC and permission matrix | Role/Permission | CreateRole/GrantPermission | GetRole/CanAccess | `/api/v1/roles`, `/api/v1/permissions` | RBAC schema | RoleUpdated | UserCreated | Auth, Tenant | roles-permissions | unit/api/security | Foundation |
| Team (if applicable) | Staff grouping | Team assignment/workload grouping | Team | CreateTeam/AssignUser | GetTeam/ListTeams | `/api/v1/teams` | Team schema | TeamUpdated | UserCreated | User, Store | team | unit/api | Optional post-foundation |
| Category | Product classification | Category taxonomy | Category | CreateCategory/ArchiveCategory | ListCategories | `/api/v1/categories` | Product master schema | CategoryUpdated | - | Tenant, Organization | categories | unit/api | Master data |
| Brand | Product brand master | Brand lifecycle | Brand | CreateBrand/ArchiveBrand | ListBrands | `/api/v1/brands` | Product master schema | BrandUpdated | - | Tenant, Organization | brands | unit/api | Master data |
| Unit | UOM master | Units and conversions policy | Unit | CreateUnit/UpdateUnit | ListUnits | `/api/v1/units` | Product master schema | UnitUpdated | - | Tenant, Organization | units | unit/api | Master data |
| Tax | Tax configuration | Tax categories/rates | TaxCategory | CreateTaxCategory/UpdateTax | ListTaxCategories | `/api/v1/tax` | Tax schema | TaxUpdated | - | Tenant, Organization | tax | unit/api | Master data |
| Product | Sale/purchase item base | Product catalog/SKU/barcode | Product | CreateProduct/UpdateProduct | SearchProduct/GetProduct | `/api/v1/products` | Product schema | ProductUpdated | CategoryUpdated/BrandUpdated | Category, Brand, Unit, Tax, Store | products | unit/api/ui | Master data |
| Customer | Buyer management | Profiles/history/outstanding | Customer | CreateCustomer/UpdateCreditPolicy | GetCustomer/ListCustomerHistory | `/api/v1/customers` | Customer schema | CustomerUpdated | SaleCompleted | Store, User, Audit | customers | unit/api | Core business |
| Supplier | Vendor management | Profiles/performance/payable links | Supplier | CreateSupplier/UpdateSupplier | GetSupplier/ListSupplierHistory | `/api/v1/suppliers` | Supplier schema | SupplierUpdated | PurchaseReceived | Store, User, Audit | suppliers | unit/api | Core business |
| Purchase | Stock intake | PO/receipt/invoice intake | Purchase/PurchaseItem | CreatePO/ReceiveGoods | GetPurchase/ListPurchases | `/api/v1/purchases` | Purchase schema | PurchaseCreated/PurchaseReceived | SupplierUpdated | Supplier, Product, Tax, Store, RBAC | purchases | unit/int/api | Transactional |
| PurchaseReturn | Reverse purchase | Return to supplier | PurchaseReturn | CreatePurchaseReturn | GetPurchaseReturn | `/api/v1/purchase-returns` | Purchase schema | PurchaseReturnCompleted | PurchaseReceived | Purchase, Inventory, Supplier | purchases | unit/int/api | Transactional |
| Inventory | Stock truth | On-hand, movement, valuation state | InventoryBalance/Movement | ReserveStock/ReleaseStock | GetStock/GetLowStock | `/api/v1/inventory` | Inventory schema | InventoryChanged | PurchaseReceived/SaleCompleted/Transfer events | Product, Store, Purchase, Sales, Transfer | inventory | int/api/db | Transactional |
| StockLedger | Explainability | Immutable stock reason trail | InventoryTransaction | RecordStockTransaction | GetStockHistory | `/api/v1/inventory/ledger` | Ledger schema | StockLedgerRecorded | All stock-changing events | Inventory + all stock modules | inventory | int/db/audit | Transactional |
| Transfer | Store-to-store movement | Request/approve/dispatch/receive | StockTransfer/Item | CreateTransferRequest/Dispatch/Receive | GetTransferStatus | `/api/v1/transfers` | Transfer schema | TransferRequested/Dispatched/Received | InventoryChanged | Inventory, Store, RBAC | transfers | unit/int/api | Transactional |
| StockAdjustment | Corrective stock updates | Controlled manual adjustments | StockAdjustment | AdjustStock | ListAdjustments | `/api/v1/stock-adjustments` | Inventory schema | StockAdjusted | InventoryChanged | Inventory, RBAC, Audit | inventory | unit/int/security | Transactional |
| POS | Billing counter flow | Cart/hold/resume/void execution | Cart/POSBill | CreateCart/HoldBill/ResumeBill/VoidBill | GetOpenBills | `/api/v1/pos` | POS schema | BillHeld/BillVoided/SaleInitiated | ProductUpdated/InventoryChanged | Product, Inventory, Customer(optional), RBAC | pos | ui/int/e2e | Transactional |
| Sales | Completed sales records | Sales finalization and history | Sale/SaleItem | CompleteSale/CancelSale | GetSale/ListSales | `/api/v1/sales` | Sales schema | SaleCompleted/SaleCancelled | POS events | POS, Product, Customer, Store, RBAC | sales | int/api/e2e | Transactional |
| Billing | Invoice amount engine | Tax/discount/total calculations | Bill/TaxLine/DiscountLine | RecalculateBill | GetBillBreakdown | `/api/v1/billing` | Sales schema | BillCalculated | POS cart updates | POS, Sales, Tax rules | billing | unit/int | Transactional |
| Invoice | Fiscal record | Invoice generation/reprint/share metadata | Invoice | GenerateInvoice/ReprintInvoice | GetInvoice | `/api/v1/invoices` | Invoice schema | InvoiceGenerated | SaleCompleted | Billing, Sales, Store, Customer | billing | unit/api | Transactional |
| Payment | Money receipt/disbursement | Payment capture and reconciliation | Payment/PaymentAllocation | RecordSalePayment/RecordSupplierPayment | GetPayments | `/api/v1/payments` | Payment schema | PaymentRecorded | SaleCompleted/PurchaseReceived | Sales, Purchase, Invoice, Receivable, Payable | payments | unit/int/api | Transactional |
| SalesReturn | Post-sale reversal | Return/refund/reversal | SalesReturn/Item | CreateSalesReturn | GetSalesReturn | `/api/v1/sales-returns` | Returns schema | SalesReturnCompleted | SaleCompleted | Sales, Inventory, Payment | returns | unit/int/e2e | Transactional |
| Expense | Cost tracking | Store/category/date expense capture | Expense | CreateExpense/ApproveExpense(future) | ListExpenses | `/api/v1/expenses` | Expense schema | ExpenseRecorded | - | Store, RBAC, Payment method data | expenses | unit/api | Core business |
| Receivable | Customer dues | Outstanding tracking and settlement | ReceivableEntry | CreateReceivable/SettleReceivable | GetCustomerOutstanding | `/api/v1/receivables` | Finance schema | ReceivableUpdated | SaleCompleted/PaymentRecorded | Sales, Customer, Payment | receivables | int/api | Core business |
| Payable | Supplier dues | Outstanding payable tracking | PayableEntry | CreatePayable/SettlePayable | GetSupplierOutstanding | `/api/v1/payables` | Finance schema | PayableUpdated | PurchaseReceived/PaymentRecorded | Purchase, Supplier, Payment | payables | int/api | Core business |
| Reporting | Decision reporting | Aggregated business reports | ReportView models | GenerateReport | Sales/Purchase/Inventory/Finance reports | `/api/v1/reports` | Reporting read models | ReportGenerated | Domain events | All operational modules + RBAC | reports | api/perf | Reporting |
| Dashboard | Operational cockpit | KPI cards/charts/alerts views | DashboardSnapshot | RefreshDashboard | GetDashboard | `/api/v1/dashboard` | Reporting read models | DashboardRefreshed | Domain events | Sales, Purchase, Inventory, Expense, Receivable, Payable, Transfer | dashboard | api/ui/perf | Reporting |
| Notification | Action alerts | In-app/email/SMS/WhatsApp(channel-gated) | Notification | SendNotification/Acknowledge | ListNotifications | `/api/v1/notifications` | Notification schema | NotificationSent | LowStock, DuePayment, Risk alerts | Events from core modules | notifications | unit/int | Core business |
| Audit | Compliance trail | Immutable operational audit logs | AuditLog | RecordAuditEntry | QueryAudit | `/api/v1/audit` | Audit schema | AuditRecorded | Sensitive domain events | Cross-cutting | audit | int/security | Foundation/Core |
| Settings | Tenant/store config | Config and policy management | Setting | UpdateSetting | GetSetting | `/api/v1/settings` | Settings schema | SettingsChanged | - | Tenant, Organization, Store | settings | unit/api | Foundation |
| Subscription | SaaS commercialization | Plans/subscription lifecycle | Subscription/Plan | ActivateSubscription/ChangePlan | GetSubscription | `/api/v1/subscriptions` | SaaS schema | SubscriptionChanged | TenantCreated | Tenant, Org, Billing domain | subscription | unit/api | SaaS |
| Integration | External connections | Controlled external adapters | IntegrationConfig | ConfigureIntegration/DisableIntegration | GetIntegrationStatus | `/api/v1/integrations` | Integration schema | IntegrationTriggered | Domain events | Core APIs, Auth, Audit | integrations | unit/int/security | Hardening+ |
| Automation | Rule-driven ops | If-then workflows with guardrails | WorkflowRule/Execution | CreateRule/RunRule | ListExecutions | `/api/v1/workflows`, `/api/v1/workflows/rules` | Automation schema | WorkflowTriggered | Domain events | Notification + core events | automation | unit/int | AI+Automation phase |
| AI Insights | Assistive intelligence | Analysis/recommendation layer | Insight/Recommendation | GenerateBriefing | GetInsights | `/api/v1/ai/insights` | Read models + AI logs | InsightGenerated | Reporting/Events | Reports, Dashboard, permissions | ai-insights | eval/security | AI+Automation phase |

### 3.2 Advanced capability modules (phased)

| Capability | Purpose | Main touchpoints | Key entities | APIs | Depends on | Phase |
|---|---|---|---|---|---|---|
| Event Sourcing | Preserve critical business history | Sales, Inventory, Purchase, Payment, Transfer, Returns | Event/EventSubscription/EventProjection | `/api/v1/events` | Stable core transactions, strong audit | Advanced |
| CQRS | Separate write/read optimization | Commands in core modules; projections for reports/dashboard | ReadModel/Projection | read APIs under reports/dashboard + projection workers | Event strategy + reporting | Advanced |
| Offline Sync | Controlled offline retail continuity | POS, Inventory validation, Device management | Device/SyncOperation/SyncConflict | `/api/v1/sync`, `/api/v1/sync/conflicts` | PWA foundation, idempotency, conflict policy | Advanced |
| B2B Network | Supplier-retailer collaboration | Supplier, Purchase, Payables, Portal users | SupplierPortalUser/B2BOrder | `/api/v1/b2b`, `/api/v1/suppliers/portal` | Mature supplier/purchase/payable flows | Advanced |
| Omnichannel | Unified physical+digital order flow | Inventory reservations, Orders, Fulfillment | Channel/Order/OrderItem/InventoryReservation | `/api/v1/channels`, `/api/v1/orders`, `/api/v1/reservations` | Stable inventory + order lifecycle | Advanced |
| Fraud/Risk Engine | Detect unusual patterns for human review | Sales, POS, Payments, Inventory adjustments | RiskRule/RiskSignal/RiskAlert/RiskCase | `/api/v1/risk`, `/api/v1/risk/alerts`, `/api/v1/risk/cases` | Event signals, thresholds, case workflow | Advanced |
| Advanced AI | Predictive and agentic assistance | Reporting, risk, automation, procurement | AI models/assist tasks | Future AI endpoints | Reliable data + guardrails + review policies | Advanced+ |

---

## PART 4 — DEPENDENCY GRAPH

### 4.1 Layered graph
```text
FOUNDATION
  Health, Auth, Tenant, Organization, Store, User, Role/Permission, Settings, Audit
      ↓
CORE BUSINESS
  Category/Brand/Unit/Tax/Product, Customer, Supplier
      ↓
TRANSACTIONAL
  Purchase -> Inventory/StockLedger <- Sales/POS/Billing/Invoice
  Transfer, StockAdjustment, Returns, Payment, Receivable, Payable, Expense
      ↓
REPORTING
  Dashboard, Reports, Notifications
      ↓
ADVANCED PLATFORM
  Event Sourcing, CQRS, Offline Sync, B2B, Omnichannel, Fraud/Risk
      ↓
AI/INTELLIGENCE
  Automation, AI Insights, Advanced AI
```

### 4.2 Hard dependencies
- Auth + Tenant + RBAC before any business write path.
- Store/Product/Supplier before Purchase.
- Inventory ledger path before scale rollout of Sales/POS.
- Sales/Purchase/Payment before Receivable/Payable truth.
- Core transactional integrity before reporting reliability.

### 4.3 Optional dependencies
- Team module (if organizational need validated).
- Redis usage for caching/queues only where justified.
- Early projection workers optional until Event/CQRS phase.

### 4.4 Circular dependency risks
- Sales <-> Inventory <-> Payment feedback loops.
- Purchase <-> Inventory <-> Payables settlement loops.
- Reporting querying transactional writes directly (tight coupling risk).
- Automation writing back into source modules without guardrails.

### 4.5 Modules that must remain independent
- Auth/RBAC should not depend on transactional modules.
- Tenant isolation logic must remain cross-cutting and reusable, not business-module-owned.
- Audit logging must remain append-only and not business-write dependent.

---

## PART 5 — IMPLEMENTATION PHASES

> Numbered roadmap from current repo state to advanced evolution.

### Phase 0 — Governance baseline
- **Objective:** Lock project operating rules and planning artifacts.
- **Modules:** Governance docs only.
- **Dependencies:** None.
- **Deliverables:** AGENTS + foundation docs + this plan.
- **Tests:** Document consistency review.
- **Exit:** No contradictions, approved plan.

### Phase 1 — Repository foundation
- **Objective:** Initialize repository layout and conventions.
- **Modules:** N/A (scaffolding only).
- **Dependencies:** Phase 0.
- **Deliverables:** Folder skeleton, coding/build/test conventions.
- **Tests:** Basic lint/build command contracts.
- **Exit:** Reproducible local setup contract.

### Phase 2 — Development infrastructure
- **Objective:** Tooling baseline.
- **Modules:** N/A.
- **Dependencies:** Phase 1.
- **Deliverables:** Environment config templates, scripts.
- **Tests:** Setup validation scripts.
- **Exit:** New developer bootstrap path works.

### Phase 3 — Backend foundation
- **Objective:** ASP.NET modular-monolith host + shared cross-cutting baseline.
- **Modules:** Health, shared building blocks.
- **Dependencies:** Phase 2.
- **Deliverables:** Solution structure only, no business implementation.
- **Tests:** Host health/unit baseline.
- **Exit:** Backend host boots with foundational wiring.

### Phase 4 — Frontend foundation
- **Objective:** Angular app shell/core/shared/layout baseline.
- **Modules:** Auth shell placeholders, tenancy context shell.
- **Dependencies:** Phase 2.
- **Deliverables:** Frontend architecture skeleton.
- **Tests:** Build + core routing tests.
- **Exit:** App shell stable.

### Phase 5 — Database foundation
- **Objective:** DB conventions/migration pipeline strategy.
- **Modules:** N/A.
- **Dependencies:** Phase 3.
- **Deliverables:** Migration approach, naming/index conventions.
- **Tests:** Migration dry-run validations.
- **Exit:** Safe migration workflow established.

### Phase 6 — Authentication
- **Objective:** Secure identity flows.
- **Modules:** Authentication.
- **Dependencies:** Phases 3, 5.
- **Deliverables:** Auth boundaries and contracts.
- **Tests:** Auth + security tests.
- **Exit:** Auth gate operational.

### Phase 7 — Multi-tenancy
- **Objective:** Tenant context and isolation enforcement.
- **Modules:** Tenant, Organization.
- **Dependencies:** Phase 6.
- **Deliverables:** Tenant-aware API/data patterns.
- **Tests:** Tenant isolation tests.
- **Exit:** Cross-tenant access blocked.

### Phase 8 — Org/Store/User/Permissions
- **Objective:** Operational identity hierarchy.
- **Modules:** Store, User, Role/Permission, Settings, Audit baseline.
- **Dependencies:** Phase 7.
- **Deliverables:** RBAC and organization hierarchy operational.
- **Tests:** RBAC + isolation + audit tests.
- **Exit:** Foundation prerequisites complete.

### Phase 9 — Master data
- **Objective:** Product and party masters.
- **Modules:** Category, Brand, Unit, Tax, Product, Customer, Supplier.
- **Dependencies:** Phase 8.
- **Deliverables:** CRUD and search/list operations.
- **Tests:** API and validation tests.
- **Exit:** Master data ready for transactions.

### Phase 10 — Supplier + Purchase
- **Objective:** Procurement flow baseline.
- **Modules:** Supplier enhancements, Purchase, Purchase Return.
- **Dependencies:** Phase 9.
- **Deliverables:** PO/receive/payable source events.
- **Tests:** Purchase flow integration tests.
- **Exit:** Purchase workflow stable.

### Phase 11 — Inventory foundation
- **Objective:** Stock truth and ledger.
- **Modules:** Inventory, StockLedger, StockAdjustment.
- **Dependencies:** Phase 10.
- **Deliverables:** Auditable stock movement chain.
- **Tests:** Inventory integrity tests.
- **Exit:** “Why stock = X?” explainable.

### Phase 12 — Sales + POS + Billing
- **Objective:** End-to-end store sales.
- **Modules:** POS, Sales, Billing, Invoice.
- **Dependencies:** Phases 9, 11.
- **Deliverables:** Cart-to-invoice flow with controls.
- **Tests:** POS/E2E/permission tests.
- **Exit:** Sale completion consistent with inventory.

### Phase 13 — Payments + Returns + Transfers
- **Objective:** Financial and reverse/stock-move operations.
- **Modules:** Payment, SalesReturn, Transfer.
- **Dependencies:** Phase 12.
- **Deliverables:** Settlement and reverse flows.
- **Tests:** Payment/return/transfer integration tests.
- **Exit:** Transaction loop complete.

### Phase 14 — Expenses + Receivables + Payables
- **Objective:** Operational finance visibility.
- **Modules:** Expense, Receivable, Payable.
- **Dependencies:** Phase 13.
- **Deliverables:** Outstanding and expense tracking.
- **Tests:** Financial consistency tests.
- **Exit:** Baseline operational finance complete.

### Phase 15 — Reporting + Dashboard + Notifications + Audit hardening
- **Objective:** Decision visibility and alerts.
- **Modules:** Report, Dashboard, Notification, Audit strengthening.
- **Dependencies:** Phases 11–14.
- **Deliverables:** KPI/report surfaces and alerts.
- **Tests:** Report correctness/perf tests.
- **Exit:** Operational dashboard readiness.

### Phase 16 — Testing hardening + Security hardening + Deployment path
- **Objective:** Production readiness.
- **Modules:** Cross-cutting.
- **Dependencies:** Phase 15.
- **Deliverables:** CI/CD, observability, hardening.
- **Tests:** full regression + perf + security.
- **Exit:** Pilot-ready release candidate.

### Phase 17 — VGS 5-store pilot + SaaS hardening
- **Objective:** Real-world validation and SaaS readiness.
- **Modules:** Subscription, onboarding, support processes.
- **Dependencies:** Phase 16.
- **Deliverables:** Pilot operations + onboarding model.
- **Tests:** pilot acceptance + SaaS readiness tests.
- **Exit:** External-customer-ready baseline.

### Phase 18+ — Advanced platform capabilities (phased)
- **18A Event Sourcing/CQRS (selected domains)**
- **18B Offline-first sync**
- **18C Fraud/Risk engine**
- **18D B2B supplier network**
- **18E Omnichannel**
- **18F Advanced AI**

Each advanced phase requires explicit readiness checks and remains non-MVP.

---

## PART 6 — TASK BREAKDOWN (Phase 1 + immediate foundation)

| Task ID | Task name | Objective | Inputs | Files expected to change | Dependencies | Implementation notes | Tests | Validation | Definition of Done |
|---|---|---|---|---|---|---|---|---|---|
| TASK-001 | Initialize Git workspace | Establish version control baseline | AGENTS + plan | `.git/*`, `.gitignore`, README updates if needed | None | Non-destructive init only | N/A | `git status` clean baseline | Repo is git-initialized with ignore policy |
| TASK-002 | Create root scaffolding | Create agreed top-level folder skeleton | Part 2 tree | folder structure only | TASK-001 | No app logic | N/A | folder audit | Top-level structure matches plan |
| TASK-003 | Add repo policy files | Add contribution/branch/PR templates | AGENTS + Part 8 | `.github/*`, docs policy files | TASK-002 | Governance before coding | N/A | template review | Policy templates exist and align |
| TASK-004 | Backend solution scaffold | Create backend solution/projects skeleton | Architecture + Part 2 | `backend/*` scaffold files | TASK-002 | No business module logic | build smoke | solution load check | Backend skeleton builds |
| TASK-005 | Frontend app scaffold | Create Angular shell/core/shared baseline | Architecture + Part 2 | `frontend/*` scaffold files | TASK-002 | No business features | build smoke | app compiles | Frontend shell boots |
| TASK-006 | Environment config baseline | Add `.env.example` and config conventions | Architecture + security rules | `infrastructure/env/*`, docs | TASK-002 | No secrets committed | config lint | variable matrix check | Required env keys documented |
| TASK-007 | Docker dev baseline scaffold | Add docker structure and compose stubs | Part 2 + stack | `infrastructure/docker/*`, `infrastructure/compose/*` | TASK-002 | Scaffold only; no full runtime stack | compose syntax checks | config parse check | Docker scaffolding ready |
| TASK-008 | Database migration framework setup | Setup migration workflow conventions | DB strategy | `database/*`, backend infra configs | TASK-004 | No production schema yet | migration tool smoke | dry-run | Migration pipeline scaffolded |
| TASK-009 | Redis integration scaffold | Add optional cache abstraction baseline | Architecture | backend infra/shared files | TASK-004 | Feature-flagged use | unit smoke | compile check | Cache abstraction ready |
| TASK-010 | Observability baseline scaffold | Logging/correlation/health framework | Observability strategy | backend shared/infra + docs | TASK-004 | Structured logging first | unit/api smoke | sample log trace | Baseline observability wired |
| TASK-011 | Error handling baseline | Unified exception/error contract | API rules | backend shared + api host | TASK-004 | Consistent error envelope | unit/api tests | error contract checks | Errors standardized |
| TASK-012 | CI skeleton | Build/test workflow skeleton | Part 8 + quality rules | `.github/workflows/*` | TASK-004, TASK-005 | Minimal required checks first | CI dry run | workflow syntax + run | CI pipeline executes baseline checks |

---

## PART 7 — TASK EXECUTION RULE

Every future AI coding task must follow this sequence:
1. Read `AGENTS.md`.
2. Read relevant architecture/foundation documents.
3. Inspect current repository state.
4. Identify affected modules.
5. Check `MODULE_DEPENDENCIES.md`.
6. Explain implementation plan.
7. List expected file changes.
8. Implement **only** requested task scope.
9. Build.
10. Run relevant tests.
11. Review changed files for scope correctness.
12. Report results.
13. **STOP** (never auto-start next task).

---

## PART 8 — GIT STRATEGY

### Branch naming
- `feat/<area>-<short-description>`
- `fix/<area>-<short-description>`
- `chore/<area>-<short-description>`
- `docs/<area>-<short-description>`

### Commit naming
- Conventional style:
  - `feat(module): ...`
  - `fix(module): ...`
  - `chore(infra): ...`
  - `docs(plan): ...`
  - `test(module): ...`

### Workflow
- One task -> one branch (or coherent small set if explicitly linked).
- One task -> clean commit set with clear scope.
- DB migration commits separated and clearly labeled.
- PRs must include: scope, affected modules, dependency impact, tests run, risk notes.
- Commit only after build + relevant tests pass.

---

## PART 9 — DATABASE IMPLEMENTATION STRATEGY

### Ownership and schema strategy
- Modular ownership: each domain module owns its tables.
- Shared cross-cutting tables only for tenancy/auth/audit where justified.
- Keep conceptual model distinct from production schema decisions.

### Migration strategy
- Incremental migrations per module and phase.
- Forward-only with rollback plan at release level.
- Migration review required for destructive changes.

### Naming conventions
- Consistent module-prefixed table names where needed.
- Explicit FK/index naming for maintainability.
- Audit columns standardized for critical entities.

### Tenant isolation
- Tenant key mandatory for tenant-owned data.
- Query filters and authorization must enforce tenant boundary.
- Store context separated under tenant hierarchy.

### Audit strategy
- Immutable audit records for sensitive operations.
- Stock-related writes must include reason/correlation/user context.

### Transaction boundaries
- ACID transaction scope for sale/purchase/transfer/return/payment critical paths.
- Retry-safe patterns for idempotent operations.

### Concurrency strategy
- Optimistic concurrency for high-contention business records where appropriate.
- Deterministic conflict handling for future offline/event flows.

### Index strategy
- Index by tenant/store/time and frequent lookup keys (SKU, barcode, doc IDs).
- Monitor and evolve via observability/performance evidence.

### Soft delete strategy
- Soft delete for master/config entities where business recovery/audit needed.
- Hard delete only for strictly technical transient records.

### Event storage/projection strategy (phased)
- Introduce event store only for selected critical domains.
- Projection/read-model pipeline after stable core transaction correctness.

### Offline synchronization data requirements (phased)
- Device identity, sync queue, idempotency keys, conflict records, reconciliation statuses.

---

## PART 10 — API IMPLEMENTATION STRATEGY

- Versioning: `/api/v1/...` baseline.
- Endpoint conventions: resource-oriented, module-scoped routing.
- Request/response conventions: consistent envelope + error contract.
- Validation: strict request validation with explicit error messages.
- Error responses: non-leaky, correlation-friendly errors.
- Authentication: token/session strategy aligned to security foundation.
- Authorization: role/permission + tenant/store scope enforcement.
- Tenant context: mandatory extraction and propagation in each request pipeline.
- Idempotency: required for repeat-prone write operations (payment/sync/event flows).
- Correlation IDs: required for tracing across API -> DB -> worker.
- Pagination/filtering/sorting: supported for list/report endpoints.
- Audit requirements: sensitive endpoints must emit audit entries.

No API implementation is performed in this document.

---

## PART 11 — FRONTEND IMPLEMENTATION STRATEGY

- Angular structure: app shell + core + shared + layout + feature modules.
- Routing: feature routes with guards and lazy loading where beneficial.
- Authentication flow: guarded routes + token/session handling via interceptors.
- Tenant/store context: explicit context selectors and propagation to API client.
- API communication: centralized HTTP client layer and typed contracts.
- State management: start simple feature-local state; scale to shared state patterns when justified.
- Error handling: global error strategy + per-feature actionable states.
- Loading states: consistent skeleton/spinner/status patterns.
- Forms: reusable validated form components.
- Tables: shared data table patterns for list/report modules.
- POS UI architecture: high-speed workflow-focused feature module (phase-gated).
- Shared UI patterns: common component library for consistency.
- Responsive behavior: desktop-first with tablet/mobile operability.

No frontend component implementation is performed in this document.

---

## PART 12 — TESTING STRATEGY (PHASE-MAPPED)

### Early phases (1–5)
- Build/lint/test harness smoke checks.
- Foundation unit tests and API host health tests.

### Core phases (6–15)
- Unit, integration, API, DB integrity, frontend tests by module rollout.
- Mandatory tenant isolation and RBAC tests from identity phases onward.
- POS and inventory integrity tests before pilot gates.
- Concurrency tests on inventory/payment critical paths.

### Hardening phases (16–17)
- Full E2E, security, performance, and regression packs:
  - inventory truth chain
  - financial consistency chain
  - tenant/RBAC boundary chain

### Advanced phases (18+)
- Event replay/versioning/projection rebuild tests.
- Offline sync reconnect/retry/conflict-resolution tests.
- Fraud/risk rule accuracy and false-positive handling tests.
- B2B, omnichannel order/reservation lifecycle tests.

---

## PART 13 — SECURITY ROADMAP

### Foundation controls
- Authn/authz baseline, RBAC, least privilege.
- Tenant and store isolation.
- Input validation and SQL injection protection.
- Secure secret/config handling.

### Core transactional controls
- Audit for sensitive operations (void, adjustment, returns, permissions).
- Payment-related security and controlled privilege checks.
- Session/token protection and transport security.

### Advanced controls (phased)
- Offline device registration and sync signing.
- Idempotency protections.
- Supplier portal/channel security boundaries.
- Risk data access controls and investigation permissions.

---

## PART 14 — OBSERVABILITY

Implementation requirements:
- Structured logs with tenant/store/user/correlation context.
- Correlation IDs end-to-end.
- Error tracking and alerting.
- Metrics and health checks (API, DB, cache, workers).
- Audit log queryability.
- Background job monitoring and retries visibility.
- Offline sync monitoring (phase-gated).
- Risk alert operational monitoring (phase-gated).
- Operational dashboard telemetry for platform reliability.

---

## PART 15 — ADVANCED ARCHITECTURE INTRODUCTION (NON-MVP)

| Capability | Why it exists | When it enters | Modules touched | Required foundation | Main risks | Testing needs | Migration considerations |
|---|---|---|---|---|---|---|---|
| Event Sourcing & CQRS | Explainability and read scalability in critical domains | After stable VGS pilot core (advanced phase) | Sales, Inventory, Purchase, Payment, Transfer, Returns, Reports | Strong audit + stable transaction semantics | Complexity, projection drift | Event ordering/replay/projection rebuild | Dual-run or phased module adoption |
| Offline-first sync | Store continuity during internet disruption | After core POS/inventory maturity | POS, Inventory, Auth/device, Sync services | Idempotency + conflict policy + device identity | Conflicts, duplicate sync, stock inconsistency | Disconnect/reconnect/conflict tests | Sync schema and reconciliation queue rollout |
| Fraud/Risk engine | Detect unusual patterns for review | After event/rules data quality is reliable | Sales, POS, Payments, Inventory adjustments, Audit | Event signals + configurable thresholds | False positives, misuse | Rule accuracy + investigation lifecycle | Non-blocking rollout with review workflow |
| B2B network | Supplier-retailer collaboration at scale | Post core supplier/purchase/payable maturity | Supplier, Purchase, Payable, Notification, Integration | Stable procurement flows + access control | Data isolation and partner trust | Supplier acceptance/partial fulfillment tests | Separate portal model with strict isolation |
| Omnichannel | Unified physical+digital commerce | After inventory reservations/order lifecycle stability | Inventory, Order, Payment, Customer history, Integration | Reservation model + fulfillment lifecycle | Oversell/allocation errors | Reservation/cancel/return/fulfillment tests | Introduce order/channel schema incrementally |

---

## PART 16 — ARCHITECTURAL DECISIONS TO BE MADE LATER

| Decision | Why it matters | When to decide | What depends on it |
|---|---|---|---|
| Offline conflict-resolution policy | Prevent stock/data corruption during reconciliation | Before offline stock-changing rollout | Offline sync implementation, POS offline scope |
| Event Sourcing/CQRS aggregate boundaries | Controls complexity and consistency model | Before event-driven domain rollout | Event store schema, projection design |
| Final production database schema | Determines long-term maintainability/performance | Before core module implementation freeze | Migrations, data contracts, indexing |
| Cloud provider/deployment topology | Impacts infra services/cost/ops model | Before production-scale deployment | CI/CD, observability, networking, DR |
| Background job runtime details | Affects retries/scheduling/operability | Before heavy async workload rollout | Notifications, projections, automation jobs |
| SaaS pricing and plan design | Commercial model and entitlement logic | Before external SaaS launch | Subscription module, onboarding flows |
| External integration/commercial constraints | Real-world feasibility and compliance of channel integrations | Before each integration release | WhatsApp/channel features, B2B integrations |

All remain **UNRESOLVED** until explicitly decided.

---

## PART 17 — FIRST 20 IMPLEMENTATION TASKS (ORDERED)

| TASK-ID | Name | Objective | Dependencies | Expected files | Tests | Done criteria |
|---|---|---|---|---|---|---|
| TASK-001 | Git Initialization | Initialize repository safely | None | `.git/*`, `.gitignore` | N/A | Git repo active, clean status |
| TASK-002 | Repository Skeleton | Create top-level folders per target tree | TASK-001 | root folders only | N/A | Structure matches plan |
| TASK-003 | Contribution/PR Policy | Add PR/branch/review templates | TASK-002 | `.github/*`, docs | N/A | Templates aligned to AGENTS |
| TASK-004 | Backend Solution Scaffold | Create solution and project skeleton | TASK-002 | `backend/*` scaffold | build smoke | Backend scaffold compiles |
| TASK-005 | Frontend Scaffold | Create Angular shell/core/shared skeleton | TASK-002 | `frontend/*` scaffold | build smoke | Frontend scaffold compiles |
| TASK-006 | Environment Config Baseline | Add env templates and conventions | TASK-002 | `infrastructure/env/*` | config checks | Env contract documented |
| TASK-007 | Docker Scaffold | Add dev/test compose and docker layout | TASK-002 | `infrastructure/docker/*`, `compose/*` | syntax checks | Compose files valid |
| TASK-008 | Database Migration Framework | Setup migration tooling pipeline | TASK-004 | `database/*`, backend infra files | migration smoke | Migration flow ready |
| TASK-009 | Redis Abstraction Baseline | Add cache abstraction contracts | TASK-004 | backend shared/infra | unit smoke | Cache layer scaffolded |
| TASK-010 | Structured Logging Baseline | Add structured log framework | TASK-004 | backend shared/infra | unit/api smoke | Logs include correlation context |
| TASK-011 | Exception/Error Contract | Standardize API error handling | TASK-004 | api host/shared files | api tests | Error envelope consistent |
| TASK-012 | Health Checks | Add liveness/readiness endpoints | TASK-004 | health module files | api tests | Health endpoints reliable |
| TASK-013 | CI Workflow Baseline | Build/test workflow automation | TASK-004, TASK-005 | `.github/workflows/*` | CI run | CI passes baseline checks |
| TASK-014 | Auth Module Foundation | Implement auth module skeleton and contracts | TASK-004, TASK-011 | auth module files | unit/api/security | Auth baseline functional |
| TASK-015 | Tenant Context Foundation | Implement tenant resolution/middleware/contracts | TASK-014 | tenancy shared/module files | isolation tests | Tenant context enforced |
| TASK-016 | Organization Module Foundation | Add organization domain/contracts | TASK-015 | organization module files | unit/api | Org operations available |
| TASK-017 | Store Module Foundation | Add store domain/contracts | TASK-016 | store module files | unit/api/isolation | Store hierarchy functional |
| TASK-018 | User + RBAC Foundation | Add users, roles, permissions baseline | TASK-017 | user/rbac module files | security tests | RBAC enforced |
| TASK-019 | Audit Foundation | Add immutable audit recording pipeline | TASK-018 | audit module/shared files | security/int tests | Sensitive actions auditable |
| TASK-020 | Master Data Start (Category/Brand/Unit/Tax) | Begin master data modules | TASK-019 | product-master modules | unit/api | Master data baseline ready |

The first task is safe to start immediately after this plan approval.

---

## PART 18 — FINAL VALIDATION

### Cross-check result
- Checked against: blueprint, AGENTS, architecture, roadmap, module dependencies, coding standards, testing strategy, AI guidelines.

### Validation findings
- No premature microservices introduced.
- No requirement to force Event Sourcing into MVP.
- Tenant boundary concerns are present across API/DB/AI guidance.
- Testing and security gates included per phase.
- Advanced capabilities remain phased and explicitly non-MVP.

### Contradictions
- No blocking contradictions found across reviewed documents.

### Circular dependency caution list
- Sales/Inventory/Payment coupling.
- Purchase/Inventory/Payables coupling.
- Reporting dependencies on transactional paths.
- Automation feedback loops.

### UNRESOLVED (intentionally deferred)
- Offline conflict-resolution policy.
- Event aggregate boundaries.
- Final production schema details.
- Cloud provider/deployment topology.
- Background job runtime specifics.
- SaaS pricing model specifics.
- External integration/commercial constraints.

