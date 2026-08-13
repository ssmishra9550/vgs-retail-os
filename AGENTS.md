# AGENTS.md

## 1. Project Identity
- **Project:** VGS Retail OS
- **Type:** Retail Business Operating System (not only billing software)
- **Pilot:** VGS Group (first real customer)
- **Long-term direction:** Multi-tenant SaaS for independent retail businesses

## 2. Product Vision
- Build a reliable retail core that unifies POS, billing, purchase, inventory, transfers, customers, suppliers, payments, expenses, reports, and operations visibility.
- Deliver practical value to current VGS stores first.
- Evolve in phases toward advanced capabilities (Event Sourcing/CQRS, Offline-first sync, B2B network, Omnichannel, Risk/Fraud engine, AI/Automation).

## 3. Source of Truth Hierarchy
1. Direct task instructions from user for current work.
2. `AGENTS.md` (primary **agent operating contract** for this repository).
3. `VGS_Retail_OS_Master_Blueprint_Updated.md` (primary **product and architecture** source of truth).
4. Foundation documents (derived from blueprint):
   - `PROJECT_RULES.md`
   - `ARCHITECTURE.md`
   - `DEVELOPMENT_ROADMAP.md`
   - `MODULE_DEPENDENCIES.md`
   - `CODING_STANDARDS.md`
   - `TESTING_STRATEGY.md`
   - `AI_DEVELOPMENT_GUIDELINES.md`
5. Existing code patterns and conventions.

**Interpretation rule:** `AGENTS.md` governs agent behavior/workflow. The blueprint governs product requirements and architecture intent.

**Conflict rule:** If two project documents conflict, stop and report the conflict explicitly. Do not silently choose.

## 4. Technology Stack
- Frontend: Angular (PWA-first)
- Backend: ASP.NET Core Web API
- Database: PostgreSQL
- Cache: Redis
- Background Jobs: .NET Worker Services / Hangfire-style processing
- Containerization: Docker (later implementation stage)
- Source Control: Git + GitHub
- AI Development Support: GitHub Copilot + Claude Code
- Cloud posture: cloud-ready, provider-neutral

## 5. Architecture Rules
- Start as **Modular Monolith**.
- Do not start with microservices.
- Keep strong module/domain boundaries.
- Preserve cloud-neutral design.
- Do not over-engineer MVP for future-only scenarios.

## 6. Modular Monolith Rules
- Organize implementation by vertical business modules.
- Keep cross-cutting concerns shared and reusable (auth, validation, tenancy, audit, errors).
- Maintain clear write/read boundaries where needed.
- Background jobs must be deterministic and retry-safe.

## 7. Module Boundary Rules
- Do not bypass module boundaries with direct cross-module data mutations.
- Integrate through explicit contracts (application services/API/events as designed).
- Stock, payment, and audit consistency are cross-cutting and mandatory.
- **DO NOT REWRITE OR RESTRUCTURE EXISTING CODE WITHOUT A SPECIFIC REASON.**

## 8. Multi-Tenancy Rules
- Multi-tenancy is mandatory from the start.
- Hierarchy: `Tenant -> Organization -> Store -> User`.
- Access model: `User -> Role -> Permission`.
- Enforce tenant isolation in API, data access, caches, jobs, reports, exports, and AI context.

## 9. Security Rules
- Security is version-1 scope.
- Enforce authentication, authorization, RBAC, tenant isolation, input validation, secure secret handling, HTTPS, and audit logging.
- Apply least privilege for users and services.
- Sensitive actions require explicit permissions and (where defined) approval flow.

## 10. Database Rules
- PostgreSQL is the system of record.
- Never change stock without auditable reason/context.
- Every stock-changing operation must preserve ledger traceability.
- Use transactions for multi-step critical operations.
- Event-sourcing data structures are phased; do not blanket-apply to all entities.

## 11. API Rules
- REST API with versioning (`/api/v1/...` baseline).
- Consistent validation and error response patterns.
- Support pagination/filtering/sorting where applicable.
- Enforce authn/authz/RBAC/tenant isolation on every endpoint.
- Include audit/correlation context for sensitive flows.
- Use idempotency for repeat-prone operations (especially sync/event-driven flows).

## 12. Angular Frontend Rules
- Feature modules must map to domain boundaries.
- Use route guards and interceptors consistently.
- Reuse shared UI primitives for forms/tables/notifications.
- Keep workflows simple and operationally clear.
- Build responsive UX (desktop/tablet/mobile).
- Treat offline behavior as phased and policy-driven, not default.

## 13. Backend Rules
- Use feature-vertical module structure.
- Keep domain logic explicit and testable.
- Do not hide business side effects.
- Enforce tenancy/security context at service and repository boundaries.
- Keep background workers observable and safe to retry.

## 14. Event Sourcing & CQRS Rules
- Apply selectively to critical/audit-sensitive domains only (sales, inventory, purchase, payments, transfers, returns, key financial changes).
- Do not event-source all tables.
- Preserve immutable event history and projection consistency where enabled.
- Keep CRUD for suitable master/configuration domains.

## 15. Offline-First Rules
- Offline-first is phased, not immediate MVP default.
- Define conflict rules before enabling offline stock-changing workflows.
- Require device identity, sync queue, idempotency, reconciliation, and conflict handling strategy.
- Do not claim offline sync is trivial.

## 16. B2B & Supplier Network Rules
- B2B is a post-core capability.
- Do not introduce supplier-portal/network complexity before core purchase/supplier/payable flows are stable.
- Preserve strict tenant and partner data isolation.

## 17. Omnichannel Rules
- Omnichannel is phased after core operations maturity.
- Use one business core with explicit inventory states (physical/reserved/available/in-transit).
- Validate third-party platform constraints (e.g., WhatsApp/Meta) before commitments.

## 18. Fraud/Risk Engine Rules
- Use risk language: unusual/suspicious/elevated risk/requires review.
- Never auto-accuse users/employees.
- Keep thresholds configurable and data-validated.
- Human review workflow is mandatory for investigation outcomes.

## 19. Automation Rules
- Automations must follow explicit rules and permissions.
- Automations cannot bypass domain integrity, tenancy, or audit requirements.
- Start with notification/assistive automations before high-risk autonomous actions.

## 20. AI Rules
- AI assists analysis/recommendations/automation; AI is not source of truth.
- Sensitive actions require human-controlled authorization/approval.
- Enforce tenant isolation and data minimization for AI context.
- Keep AI output observable and auditable.

## 21. Testing Rules
- Required layers: unit, integration, API, UI, end-to-end, security, performance, and DB integrity tests (as applicable to scope).
- Critical workflow coverage is mandatory before release gates:
  - sale/return
  - purchase
  - stock transfer
  - payment
  - tenant isolation
  - roles/permissions
- Add advanced test suites only when advanced capabilities are introduced.

## 22. Logging & Observability Rules
- Use structured logs for critical operations.
- Include metrics, health checks, and error tracking.
- Monitor API latency, DB behavior, background jobs, and audit trails.
- Keep observability production-ready before scale rollout.

## 23. Git & Commit Rules
- Keep commits scoped to one task.
- Use clear commit messages describing intent and impact.
- Do not include unrelated refactors in task commits.
- Do not rewrite shared history unless explicitly requested.

## 24. Code Review Rules
- Verify architectural alignment, module boundaries, security, tenancy, and data integrity.
- Verify dependency impacts and regression risk.
- Reject changes that add hidden coupling or bypass audit/authorization.
- Require tests for changed behavior.

## 25. Migration Rules
- Apply schema/data migrations incrementally and reversibly.
- No destructive data changes without explicit approved migration plan.
- Preserve backward compatibility during phased rollout where required.
- Do not introduce advanced schema complexity before corresponding phase.

## 26. Dependency Rules
- Follow `MODULE_DEPENDENCIES.md` before implementing cross-module features.
- Implement prerequisites first; do not skip foundational dependencies.
- Document and review dependency impact for every non-trivial change.

## 27. What AI Agents MUST NOT Do
- Do not modify `VGS_Retail_OS_Master_Blueprint_Updated.md`.
- Do not invent requirements, claims, compliance, or timelines.
- Do not implement unrelated tasks automatically.
- Do not bypass tenancy, authorization, audit, or stock-traceability rules.
- Do not introduce microservices-first architecture.
- Do not force advanced capabilities into MVP.
- Do not rewrite/restructure existing code without specific reason and approval context.
- Do not execute risky data/schema changes without explicit plan and review.

## 28. What AI Agents MUST Do Before Coding
1. Read `AGENTS.md`.
2. Read relevant foundation documents for the task scope.
3. Identify affected module(s).
4. Check `MODULE_DEPENDENCIES.md` for prerequisite and coupling impact.
5. Check existing implementation patterns in the codebase.
6. Explain the planned change clearly.
7. Identify files expected to be modified.
8. Identify database/API/domain impact.
9. Identify tests required for changed behavior.
10. Only then implement the task.

## 29. Definition of Done
- Change is complete, scoped, and aligned with blueprint + foundation docs.
- Relevant build and tests pass for changed scope.
- Security/tenancy/data-integrity rules are preserved.
- Changed files are listed and rationale is clear.
- No unrelated modifications included.

## 30. Development Workflow
- **ONE TASK AT A TIME.**
- Do not continue to unrelated tasks automatically.
- For each task:
  1. Analyze using Section 28 checklist.
  2. Implement only approved scope.
  3. Build.
  4. Run relevant tests.
  5. Report result.
  6. Show changed files.
  7. Stop and wait for the next task.
