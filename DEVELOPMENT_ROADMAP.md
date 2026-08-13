# DEVELOPMENT_ROADMAP.md

## 1) Delivery Strategy
Follow phased execution from the blueprint: stabilize core retail operations first, then harden for VGS pilot, then evolve toward SaaS and advanced platform capabilities.

## 2) Phase Plan (Phase 0 -> Phase 10)

### Phase 0 — Discovery
**Focus:** Product understanding, scope boundaries, module map, success metrics, risks.  
**Exit criteria:** Approved foundation docs and aligned implementation scope.

### Phase 1 — Foundation
**Focus:** Project skeleton, modular monolith boundaries, auth baseline, tenancy context, roles/permissions baseline, shared patterns.  
**Exit criteria:** Secure, multi-tenant-ready foundation with enforceable boundaries.

### Phase 2 — Master Data
**Focus:** Product/category/brand/unit/tax plus core business masters (store/customer/supplier).  
**Exit criteria:** Master data lifecycle and permissions stabilized.

### Phase 3 — Purchase + Inventory
**Focus:** Purchase flow, goods receiving, inventory ledger, stock movements, adjustments, transfer foundations.  
**Exit criteria:** Traceable stock truth with auditable inventory transactions.

### Phase 4 — POS + Billing
**Focus:** POS cart-to-invoice flow, payment modes, receipt/reprint/hold-resume/void controls, inventory impact integration.  
**Exit criteria:** End-to-end sale completion with stock and payment consistency.

### Phase 5 — Customers + Suppliers + Expenses
**Focus:** Customer/supplier financial visibility (receivable/payable), expense capture and classification.  
**Exit criteria:** Operational entity management and basic financial tracking complete.

### Phase 6 — Reports + Dashboard
**Focus:** Operational dashboard and core report sets (sales, purchase, inventory, financial/operational).  
**Exit criteria:** Decision-ready visibility for owners/managers.

### Phase 7 — VGS Pilot
**Focus:** Run in VGS stores, collect real usage feedback, close operational gaps.  
**Exit criteria:** Pilot stability and validated fit for daily store operations.

### Phase 8 — Hardening
**Focus:** Performance, security hardening, observability, reliability, disaster-readiness, audit strengthening.  
**Exit criteria:** Production-grade baseline.

### Phase 9 — SaaS
**Focus:** Tenant onboarding/subscription flows, scalable multi-tenant operations, support readiness.  
**Exit criteria:** External customer onboarding capability.

### Phase 10 — AI + Automation
**Focus:** Assistive intelligence and workflow automation with guardrails and approvals for sensitive actions.  
**Exit criteria:** Controlled AI/automation value without loss of data authority.

## 3) Time Horizon Guidance (Blueprint-Aligned)

### 0–2 years
- Foundation -> Core operations -> POS -> VGS pilot -> SaaS launch readiness.
- Timeline must remain scope/team-quality dependent, not fixed promise.

### 3–5 years
- Customer growth, onboarding maturity, advanced reporting/integrations.
- Start selected Event Sourcing/CQRS and offline-first foundations.
- Begin B2B supplier capabilities, omnichannel groundwork, risk engine foundations.

### 6–10 years
- Connected retail platform expansion.
- Stronger mobile operations, AI recommendations, API ecosystem, larger multi-store customers.

### 11–20 years (Strategic Horizon)
- Platform maturity -> AI-assisted operations -> ecosystem expansion -> predictive operations -> full Retail OS vision.
- These are strategic possibilities, not guaranteed dated commitments.

## 4) Advanced Capability Sequencing (Do Not Pull Forward Prematurely)
1. MVP: Core retail workflows + basic audit/roles.
2. V1 Pilot: Strong inventory ledger, strong audit, selective event-driven operations.
3. V2: Selected Event Sourcing/CQRS + offline-first POS/sync foundations.
4. V3: Risk/fraud engine + workflow upgrades + supplier portal/B2B flows.
5. V4: Omnichannel (online store, channel orders, reservations).
6. V5+: Advanced AI, predictive inventory, procurement intelligence, agentic workflows.

