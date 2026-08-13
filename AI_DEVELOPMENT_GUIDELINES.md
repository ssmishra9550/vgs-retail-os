# AI_DEVELOPMENT_GUIDELINES.md

## 1) Purpose
Define how AI is used to build and evolve VGS Retail OS safely, aligning with the blueprint’s AI vision and guardrails.

## 2) AI Role in Product vs System of Record
- AI is an assistant for analysis, explanation, recommendation, and selected automation.
- AI is **not** the source of truth.
- PostgreSQL/business transaction records remain authoritative.

## 3) AI Development Workflow (Engineering)
1. Start from blueprint-defined requirements and module boundaries.
2. Use AI tools (Claude Code/GitHub Copilot) to accelerate implementation drafts.
3. Validate generated logic against domain rules (inventory, payments, permissions, tenancy).
4. Require human review for sensitive workflows and production promotion.

## 4) Safety and Guardrails
- Never allow autonomous AI writes to critical business state without explicit policy.
- Sensitive actions must require permissions and/or approval workflows.
- Preserve audit trace for AI-assisted actions and recommendations.
- Use cautious risk language: “unusual/suspicious/elevated risk/requires review”.

## 5) Approved AI Use Cases (Phased)
- Operational briefing and explanation of metrics.
- Recommendation assistance (purchase hints, low stock attention, trend highlights).
- Report summarization for owners/managers.
- Future risk signal assistance and workflow suggestions.

## 6) Restricted/High-Control Use Cases
- Direct stock adjustments
- Financial postings
- Permission/role changes
- Fraud accusations or disciplinary conclusions

These require explicit human-controlled workflows and audit trails.

## 7) Data and Privacy Rules for AI Features
- Enforce tenant isolation for AI prompts, context, and outputs.
- Scope model context to least required data.
- Avoid exposing secrets or cross-tenant records in prompts/outputs.
- Keep model interactions observable and reviewable.

## 8) AI Quality Evaluation
- Measure usefulness, correctness, and actionability of AI outputs.
- Track hallucination rate and unsafe recommendation rate.
- Validate recommendations against actual transactional outcomes where possible.
- Use phased rollout with feedback loops before broad enablement.

## 9) AI + Advanced Capabilities Alignment
- Event-driven/read-model foundations improve explainability for AI insights.
- Offline/B2B/Omnichannel/Risk modules should expose reliable interfaces before AI orchestration.
- Do not front-run architecture maturity with AI complexity.

## 10) Delivery Principle
Build core retail reliability first; add AI where it creates measurable operational value without compromising data integrity, security, or tenant trust.

