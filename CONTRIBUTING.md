# CONTRIBUTING.md

## 1. Purpose
Define a consistent contribution process for humans and AI agents so changes remain aligned with VGS Retail OS architecture, safety, and delivery phases.

## 2. Source of Truth
1. Active task instruction.
2. `AGENTS.md` (agent operating contract).
3. `VGS_Retail_OS_Master_Blueprint_Updated.md` (product/business/architecture source).
4. Foundation docs: `PROJECT_RULES.md`, `ARCHITECTURE.md`, `DEVELOPMENT_ROADMAP.md`, `MODULE_DEPENDENCIES.md`, `CODING_STANDARDS.md`, `TESTING_STRATEGY.md`, `AI_DEVELOPMENT_GUIDELINES.md`, `IMPLEMENTATION_PLAN.md`.

If documents conflict, stop and report the conflict explicitly.

## 3. Repository Structure
Use the approved modular layout for `backend/`, `frontend/`, `database/`, `infrastructure/`, `deploy/`, `docs/`, `scripts/`, and `.github/` as defined in `IMPLEMENTATION_PLAN.md`.

## 4. Development Philosophy
- Modular monolith first; no microservices-first implementation.
- Business-first, secure-by-default, tenant-safe design.
- Build for phased evolution; do not force advanced capabilities into MVP.

## 5. AI-Assisted Development
AI assistance is allowed for planning, implementation, review, and testing support, but AI is not source of truth for business state or requirements.

## 6. One Task at a Time Rule
- Work on exactly one approved task at a time.
- Do not automatically continue to the next task.
- After finishing a task: report, commit (if requested), and stop.

## 7. Task Lifecycle
1. Read required source documents.
2. Understand scope and constraints.
3. Identify affected modules and dependencies.
4. Plan file-level changes.
5. Implement only requested scope.
6. Build and run relevant tests.
7. Review diff for unrelated changes.
8. Report results and stop.

## 8. Branching Strategy
- One task should map to one branch (or a tightly related set if explicitly approved).
- Keep branches short-lived and scoped.
- Keep migration-heavy and risky changes isolated for focused review.

## 9. Branch Naming Convention
Preferred branch format:
- `feat/TASK-<id>-<short-description>`
- `fix/TASK-<id>-<short-description>`
- `chore/TASK-<id>-<short-description>`
- `docs/TASK-<id>-<short-description>`
- `refactor/TASK-<id>-<short-description>` (only when explicitly approved/scope-justified)

## 10. Commit Convention
Use clear Conventional Commit style with coherent task-level scope:
- `feat(module): ...`
- `fix(module): ...`
- `chore(infra): ...`
- `docs(policy): ...`
- `test(module): ...`
- `refactor(module): ...`

Do not split a single small task into noisy micro-commits without reason.

## 11. Pull Request Requirements
Every PR should include:
- What changed
- Why it changed
- Task ID
- Modules affected
- Database impact
- API impact
- Security impact
- Tenant isolation impact
- Tests performed
- Risks and mitigations
- UI screenshots (when relevant)

## 12. Code Review Requirements
Reviewers must verify:
- Alignment with blueprint and architecture rules
- Module boundary integrity
- Security and tenant isolation safety
- Data integrity and auditability
- Scope discipline (no unrelated changes)
- Adequate test coverage for changed behavior

## 13. Testing Requirements
- Run relevant tests for changed scope (unit/integration/API/UI/E2E/security/performance as applicable).
- Critical workflow and tenant/RBAC coverage must be preserved.
- Do not bypass test gates for convenience.

## 14. Database Change Requirements
- No manual production DB changes.
- Schema changes must use reviewed migrations (in migration phases).
- Destructive migrations require explicit approval.
- Preserve tenant isolation, transactional integrity, and audit traceability.

## 15. API Change Requirements
- Maintain versioned REST conventions (`/api/v1/...` baseline).
- Preserve validation, error envelope consistency, and authz checks.
- Document API contract impact in PR.
- Include pagination/filter/sort behavior where relevant.

## 16. Frontend Change Requirements
- Keep Angular changes aligned to `core/shared/layout/features/state` structure.
- Use existing shared patterns for forms/tables/notifications/guards/interceptors.
- Preserve responsiveness and operational usability.

## 17. Security Requirements
- Enforce authentication, authorization, RBAC, input validation, and secure secret handling.
- Do not disable security controls to speed delivery.
- Sensitive actions require explicit permission and audit trace.

## 18. Multi-Tenant Safety Requirements
- Enforce tenant and store isolation in all reads/writes, API paths, reports, jobs, and AI context.
- Never allow cross-tenant data leakage.

## 19. Audit/Data Integrity Requirements
- Stock-changing actions must always be traceable.
- Financial and inventory consistency must be preserved.
- Do not bypass audit logging for sensitive operations.

## 20. Documentation Requirements
- Update docs when behavior, contracts, or architecture assumptions change.
- Do not modify source-of-truth docs unless explicitly required and approved.
- Clearly mark assumptions and unresolved decisions.

## 21. AI Agent Rules
AI agents must:
- Follow `AGENTS.md` and this file first.
- Read relevant foundation documents before coding.
- Explain planned changes and expected file impacts.
- Implement only requested task scope.
- Build/test/review diff/report/stop.

Mandatory AI workflow:

Read `AGENTS.md`  
↓  
Read relevant documentation  
↓  
Understand task  
↓  
Inspect existing code  
↓  
Create implementation plan  
↓  
Implement ONLY requested task  
↓  
Build  
↓  
Test  
↓  
Review diff  
↓  
Report result  
↓  
Commit  
↓  
Stop

## 22. Prohibited Changes Without Approval
Do not, without explicit approval:
- Rewrite unrelated code
- Change architecture direction
- Add new dependencies without clear justification
- Skip required tests
- Disable security controls
- Bypass tenant isolation
- Directly modify production data
- Invent business requirements
- Silently resolve architecture/document conflicts
- Continue to next task automatically

## 23. Definition of Done
A task is done only when:
- Implementation matches requirements
- Architecture and module boundaries are respected
- Build succeeds
- Relevant tests pass
- Security and tenant considerations are checked
- Documentation is updated when needed
- Diff is reviewed and contains no unrelated changes

## 24. Emergency/Fix Workflow
For urgent fixes:
1. Use a dedicated `fix/TASK-<id>-<short-description>` branch.
2. Keep scope minimal and targeted.
3. Add/execute focused validation tests.
4. Document risk, impact, and rollback notes in PR.
5. Perform post-fix follow-up tasks separately (do not bundle).

