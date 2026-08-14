# ADR-002: Angular Frontend Tech Stack

## 1. ADR Number
ADR-002

## 2. Title
Angular Frontend Tech Stack

## 3. Status
Accepted

## 4. Date
2026-08-15

## 5. Context
VGS Retail OS is a long-term enterprise retail SaaS platform planned for a 10–20 year evolution. The architecture features an ASP.NET Core 10 backend structured as a Modular Monolith with PostgreSQL. It supports multi-tenant, multi-store operations with POS/Billing as core functionality. The frontend will be built in Angular and must accommodate future offline-first/PWA requirements.

The frontend technology stack must optimize for:
- maintainability
- predictable architecture
- performance
- enterprise scalability
- simpler development
- future offline capabilities

## 6. Decision
We will establish the frontend foundation using the following technology stack:

- **Angular Version:** 22.x
- **Angular CLI:** 22.x
- **Node.js:** 24.x LTS
- **TypeScript:** 6.0.x
- **Component Architecture:** 100% Standalone Components
- **Reactivity:** Signals-first
- **Change Detection:** Zoneless by default
- **State Management:** Native Angular Signals + injectable state services initially
- **NgRx:** Deferred until a demonstrated domain-level need exists
- **PWA:** Deferred until the Offline-First phase

## 7. Decision Details & Important Design Rationale
- **Standalone Components:** Reduce `NgModule` complexity. Align with the selected modern Angular architecture.
- **Signals:** Native reactive state. Simpler local/shared UI state. Avoid premature state-management complexity.
- **Zoneless:** Selected as the default frontend strategy. Must be validated through application testing and real-world POS workflows.
- **NgRx:** Deferred. Revisit only if concrete state complexity justifies it.
- **PWA:** Deferred because offline-first is a later VGS capability. Do not introduce service-worker/cache complexity in the MVP foundation.

## 8. Alternatives Considered
- **Angular 21:** Considered for maturity, but rejected because it misses out on stable Signal APIs and default zoneless performance enhancements that are critical for our 10-20 year platform lifespan.
- **Immediate NgRx Integration:** Considered for strict enterprise state management, but rejected as it violates the project rule of "do not over-engineer MVP."

## 9. Consequences
- The frontend will be highly performant and aligned with the latest Angular paradigms.
- The team will avoid the heavy technical debt associated with migrating away from NgModules and Zone.js in the future.
- The initial development will remain lean and focused on core retail workflows without being slowed down by PWA caching or NgRx boilerplate.

## 10. Risks
- Team learning curve for modern Angular patterns
- Zoneless compatibility/integration considerations
- Framework upgrade effort
- Future state-management complexity
- Offline-first integration complexity later

## 11. Upgrade Strategy
AI coding agents must not automatically upgrade Angular, Node.js, TypeScript, or related framework versions.

Any future framework upgrade must:
1. Be proposed
2. Be researched
3. Be documented as an ADR
4. Be tested
5. Be reviewed
6. Be explicitly approved

Formal Angular and related framework reviews should be conducted every 12–18 months to ensure the platform remains within the active support lifecycle.

## 12. When This Decision Should Be Revisited
- During Phase 18B (Offline Sync) when PWA integration is scheduled.
- If state management complexity in complex domains (e.g., POS) demonstrably outgrows native Angular Signals and state services.
- Every 12-18 months for standard framework version upgrades.

## 13. Official References
- Official Angular Release & Compatibility Documentation
- Node.js Official Release Schedule
- `AGENTS.md`
- `ARCHITECTURE.md`
- `PROJECT_RULES.md`
- `IMPLEMENTATION_PLAN.md`
