<!--
SYNC IMPACT REPORT
==================
Version Change: NEW → 1.0.0
Created: 2026-06-11

PRINCIPLES ADDED:
✅ I. Spec-Driven Development
✅ II. Security by Design
✅ III. Service-Layer Validation
✅ IV. Maintainable Code for Learning
✅ V. Documentation Over Guessing

SECTIONS ADDED:
✅ Technology Standards (C#/.NET 10, Blazor Server, SQLite, Testing framework)
✅ Development Workflow (5-step process: Spec → Design → Impl → Review → Merge)

GOVERNANCE COMPLETED:
✅ Amendment process defined
✅ Compliance verification strategy documented
✅ Guidance document references

TEMPLATE CONSISTENCY CHECK:
✅ spec-template.md - Uses spec-first approach aligned with principle 1
✅ plan-template.md - Constitution check gate present, supports SDD workflow
✅ tasks-template.md - Task organization by story supports independent delivery
✅ copilot-instructions.md - References plan for additional context (needs update)

DEPENDENT ARTIFACTS STATUS:
⚠ copilot-instructions.md - Recommend update to reference this constitution
✅ .specify/templates/ - All templates aligned with SDD principles

FOLLOW-UP ACTIONS:
- Consider updating .github/copilot-instructions.md to reference this constitution
- Consider creating Architecture Decision Records (ADR) directory structure
-->

# ContosoDashboard Constitution

Training-focused project implementing Spec-Driven Development (SDD) principles with GitHub Spec Kit

## Core Principles

### I. Spec-Driven Development

Every feature begins with a formal specification document. Specifications MUST precede implementation and define acceptance criteria. All PRs MUST reference their corresponding spec. Incomplete or vague specs are rejected at code review.

### II. Security by Design

Security decisions are made at architecture time, not bolted on later. All authentication/authorization changes require design review. Mock authentication for training is clearly documented as training-only. User isolation and role-based access control (RBAC) are enforced at service layer. No shortcuts or security bypasses in code.

### III. Service-Layer Validation

Business logic lives in services, not controllers or pages. Services enforce authorization and data integrity independently. Services are unit-testable without UI. Controllers delegate to services; they do not contain business logic. Data access is validated at service boundaries.

### IV. Maintainable Code for Learning

Code clarity takes precedence over cleverness. Comments explain "why," not "what" (code shows what). Naming MUST be explicit and searchable. Complex algorithms require documentation. Abstractions serve learning, not resume-padding. Remove dead code immediately.

### V. Documentation Over Guessing

Architecture decisions are recorded in ADR (Architecture Decision Records). Features have runbook documentation. Ambiguity in requirements is flagged and clarified before coding. Training value is maximized through clear examples and inline guidance.

## Technology Standards

**Language & Framework**: C# with .NET 10, Blazor Server for UI
**Database**: SQLite for local/training, structured for future migration
**Authentication**: Cookie-based mock auth for training (production requires OAuth 2.0/OpenID Connect)
**Logging**: Structured logging via `Microsoft.Extensions.Logging`
**Testing**: xUnit for unit tests, integration tests for service boundaries
**Version Control**: Git with conventional commits; branch strategy defined in workflows

## Development Workflow

1. **Specification**: Feature spec written and approved before work begins
2. **Design**: Architecture decisions documented if non-trivial
3. **Implementation**: Code written following principles above
4. **Review**: PR must reference spec; reviewer verifies compliance with constitution
5. **Merge**: Approved PRs merged to main; CI pipeline must pass
6. **Documentation**: Changes to docs and runbooks MUST be included in PR

## Governance

This constitution is the source of truth for development standards. All architectural decisions, technology choices, and code patterns MUST align with these principles.

**Amendment Process**:

- Amendments require consensus from core contributors
- Amendment rationale MUST be documented
- Dependent artifacts (templates, guidelines) MUST be updated synchronously
- Version numbers follow semantic versioning (MAJOR.MINOR.PATCH)

**Compliance Verification**:

- Code reviews explicitly check constitution compliance
- CI pipeline enforces structural constraints where possible
- Architecture reviews validate design decisions
- Spec alignment is verified before merge

**Guidance Documents**:

- See `.github/copilot-instructions.md` for Copilot behavior
- See `.specify/templates/` for standardized document formats
- See project ADRs for architectural decisions

**Version**: 1.0.0 | **Ratified**: 2026-06-11 | **Last Amended**: 2026-06-11
