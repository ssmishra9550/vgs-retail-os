# ADR-001: Backend .NET Target Version

## 1. Decision
VGS Retail OS targets **.NET 10 LTS** for the backend.

This includes:
- **ASP.NET Core 10** for backend web/API hosting.
- **C# version supported by .NET 10**.

Future framework upgrades must be intentional architectural decisions and must not be performed automatically by AI agents.

## 2. Context
VGS Retail OS is a new, long-term enterprise SaaS project being built as a modular monolith with ASP.NET Core for the backend. The project needs a stable and actively supported framework baseline that minimizes near-term upgrade pressure and aligns with multi-year product evolution.

## 3. Options considered
### .NET 8
- LTS release.
- Support ends on **November 10, 2026**.
- Valid option, but shorter remaining support window for a newly starting long-term platform.

### .NET 9
- Standard-term (non-LTS) release.
- Not aligned with the project preference for an LTS baseline.

### .NET 10
- LTS release.
- Active support through **November 14, 2028**.
- Matches local development readiness (`10.0.203` SDK installed) and long-term support goals.

## 4. Decision rationale
The team selected .NET 10 LTS because it provides the strongest support runway for a new enterprise platform, avoids adopting a non-LTS baseline, and aligns with the existing local environment and long-horizon architecture goals.

## 5. Consequences
- Backend scaffolding and all new backend projects should target .NET 10.
- Build/test/tooling baselines should be aligned to .NET 10.
- Planning and implementation tasks should assume ASP.NET Core 10 unless a new ADR supersedes this decision.
- This decision reduces immediate framework churn risk but still requires periodic support-lifecycle review.

## 6. Migration/review policy
- Framework upgrades (major/minor strategy changes) require an explicit architecture decision record update and review.
- No automatic framework upgrades by AI agents.
- Upgrade decisions must evaluate:
  - LTS/support timeline
  - compatibility impact
  - security posture
  - operational and migration risk

## 7. Source/reference
- Microsoft .NET and .NET Core official support policy:  
  https://dotnet.microsoft.com/platform/support/policy/dotnet-core
- Project decision statement (task instruction):  
  .NET 10 is LTS (support through November 14, 2028); .NET 8 support ends November 10, 2026; .NET 9 is non-LTS.
