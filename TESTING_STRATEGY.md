# TESTING_STRATEGY.md

## 1) Testing Objectives
- Protect stock accuracy, financial correctness, tenant isolation, and role security.
- Validate critical retail workflows before production use.
- Introduce advanced test suites as advanced capabilities are phased in.

## 2) Test Layers (Blueprint-Aligned)
- Unit Tests
- Integration Tests
- API Tests
- UI Tests
- End-to-End Tests
- Security Tests
- Performance Tests
- Database/transaction integrity tests

## 3) Mandatory Critical Workflow Coverage
- Sale
- Sales return
- Purchase
- Stock transfer
- Payment
- Tenant isolation
- Roles and permissions

These workflows must pass consistently before pilot/production promotion.

## 4) Core Domain Test Focus
### POS/Sales/Billing
- Cart pricing/tax/discount calculations
- Payment mode handling (cash/UPI/card/credit where configured)
- Invoice/receipt generation and reprint flows
- Hold/resume/void permission checks

### Inventory/Purchase/Transfer
- Stock in/out correctness and ledger traceability
- Purchase receive and payable consistency
- Transfer request/dispatch/receive state transitions
- Adjustment and return impact correctness

### Financial/Operational
- Receivable/payable updates from sale/purchase/payment events
- Expense recording and reporting dimensions
- Dashboard/report consistency against transactional data

## 5) Security and Isolation Testing
- Authentication flow validity
- Authorization and RBAC boundary checks
- Tenant data isolation in all query/mutation endpoints
- Injection/input validation resilience
- Sensitive action access control and auditability

## 6) Advanced Capability Test Tracks (Phased)
### Event Architecture
- Event ordering/versioning/replay
- Projection rebuild and read-model consistency
- Idempotency and duplicate-event handling

### Offline
- Disconnect during sale flow
- Reconnect/retry/duplicate sync handling
- Conflict detection/resolution behavior
- Device recovery path

### B2B
- Supplier order acceptance/rejection
- Partial fulfillment and dispatch/delivery updates
- Invoice upload and payment status transitions

### Omnichannel
- Channel order ingestion
- Inventory reservation/release
- Cancellation/return/store-fulfillment interactions

### Risk Engine
- Rule evaluation accuracy
- Threshold behavior and false-positive handling
- Alert/case lifecycle and review workflow

## 7) Environments and Gates
- **Dev:** fast unit/integration/API checks.
- **Test/Staging:** full critical workflow + security/performance baselines.
- **Pilot/Prod readiness gate:** no open critical defects in stock, payment, security, tenancy.

## 8) Regression Strategy
- Maintain regression packs for:
  - Inventory truth chain
  - Financial consistency chain
  - Tenant/RBAC boundary chain
- Expand packs after each phase milestone (especially Phase 4, 7, 8, 9, 10).

