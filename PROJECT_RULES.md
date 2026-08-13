# PROJECT_RULES.md

## 1) Source of Truth
- `VGS_Retail_OS_Master_Blueprint_Updated.md` is the single source of truth.
- If implementation ideas conflict with the blueprint, follow the blueprint.
- Do not modify the blueprint file; extend planning and implementation through new docs/code.

## 2) Current Scope Boundary
- Build development foundations first.
- Do not implement business modules in this stage.
- Do not over-engineer MVP for long-term features.

## 3) Product Positioning Rules
- Position as a **Retail Business Operating System**, not only billing software.
- Keep language professional and realistic; avoid unsupported marketing claims.
- Treat VGS as first pilot customer and future SaaS as validated evolution.

## 4) Architecture Rules
- Start with **Modular Monolith** architecture.
- Enforce clear domain/module boundaries from day one.
- Keep cloud-ready and provider-neutral design.
- Multi-tenant design is mandatory from the beginning.

## 5) Data Integrity Rules
- Never mutate stock without auditable transactional reason.
- Stock-changing operations must be traceable via ledger/history.
- Preserve tenant data isolation in all modules, APIs, and jobs.
- Use selective Event Sourcing/CQRS only where business-critical (phased, not blanket use).

## 6) Security Rules
- Security is v1 requirement, not post-MVP enhancement.
- Enforce authentication, authorization, RBAC, tenant isolation, input validation, audit logging, and secure secrets handling.
- Apply least-privilege access for users, services, and operations.

## 7) Roadmap & Delivery Rules
- Follow phased delivery sequence defined in blueprint (Phase 0 to Phase 10 and long-range horizons).
- Treat advanced capabilities (Event Sourcing/CQRS, B2B, Omnichannel, Risk, Offline-first sync) as staged expansions after core stability.
- Mark long-term capabilities as strategic possibilities, not guaranteed commitments.

## 8) Engineering Quality Rules
- API-first with versioning, validation, explicit error handling, pagination/filtering/sorting patterns.
- Observability (logs, metrics, health checks, monitoring) must be part of production readiness.
- Testing must cover critical workflows before production promotion.

## 9) Documentation Rules
- Keep architecture, roadmap, dependencies, standards, testing, and AI guidelines aligned to blueprint revisions.
- Clearly label assumptions as assumptions; avoid inventing business requirements.
- Record rationale for any intentional deviation and get explicit approval before execution.

