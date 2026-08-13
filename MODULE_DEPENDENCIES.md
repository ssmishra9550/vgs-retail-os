# MODULE_DEPENDENCIES.md

## 1) Dependency Principles
- Build modules in dependency order to avoid rework.
- Foundational modules must be complete before transaction-heavy modules.
- Inventory, payment, and audit dependencies are cross-cutting and must remain consistent.

## 2) Foundational Dependency Chain
1. Authentication
2. Tenant Management
3. Organization
4. Store Management
5. User Management
6. Roles & Permissions
7. Settings
8. Audit Logs

These modules are prerequisites for all business operations.

## 3) Master Data Dependencies

| Module | Depends On |
|---|---|
| Category Management | Tenant, Organization |
| Brand Management | Tenant, Organization |
| Unit Management | Tenant, Organization |
| Tax Configuration | Tenant, Organization |
| Product Management | Category, Brand, Unit, Tax, Store, Audit |
| Customer Management | Store, User/RBAC, Audit |
| Supplier Management | Store, User/RBAC, Audit |

## 4) Core Transaction Dependencies

| Module | Depends On |
|---|---|
| Purchase | Supplier, Product, Tax, Store, User/RBAC |
| Purchase Return | Purchase, Inventory, Supplier |
| Inventory | Product, Store, Purchase, Sales, Transfer, Adjustment |
| Stock Ledger | Inventory transactions (all stock-changing modules) |
| Stock Transfer | Inventory, Store, User/RBAC, Approval policies |
| Stock Adjustment | Inventory, User/RBAC, Audit |
| POS | Product, Inventory, Customer(optional), User/RBAC |
| Sales | POS/Product/Customer/Store/User/RBAC |
| Billing | POS/Sales/Tax/Discount rules |
| Invoice | Billing, Sales, Store, Customer |
| Payment | Sales/Purchase/Invoice/Receivables/Payables |
| Sales Return | Sales, Inventory, Payment |
| Expenses | Store, User/RBAC, Payment method data |
| Receivables | Sales, Customer, Payment |
| Payables | Purchase, Supplier, Payment |

## 5) Visibility, Platform, and Growth Dependencies

| Module | Depends On |
|---|---|
| Dashboard | Sales, Purchase, Expenses, Inventory, Receivables, Payables, Transfers |
| Reports | All operational and financial modules + RBAC |
| Notifications | Events from core modules + user/store preferences |
| Integrations | Stable core APIs + auth/security + audit |
| Subscription / SaaS | Tenant/Organization/Store/User + billing/subscription domain |
| Automation | Events/rules from core operations + notifications |
| AI Insights | Reliable data from reports/dashboard/events + permissions |

## 6) Advanced Capability Dependencies (Phased)

| Capability | Depends On |
|---|---|
| Selected Event Sourcing/CQRS | Stable core transactions, strong audit, clear aggregates |
| Offline-First Sync | PWA foundation, device identity, idempotency, conflict policies |
| Fraud Detection & Risk Engine | Event streams/transaction signals, thresholds, case workflow |
| B2B Network & Supplier Portal | Mature supplier/purchase/payable flows and secure isolation |
| Omnichannel Commerce | Stable inventory reservations, orders, fulfillment, channel auth |

## 7) Critical Coupling Rules
- Any stock change must update inventory state **and** write ledger/audit context.
- Any financial movement must preserve receivable/payable/payment consistency.
- Role/permission checks are mandatory on sensitive operations (voids, adjustments, returns, approvals).
- Tenant/store boundaries must be enforced in reads, writes, reports, and exports.

