# Architect Brief — <Feature Name>

| | |
|---|---|
| **Feature** | `<feature-slug>` (spec: `docs/specs/<feature-slug>/`) |
| **From** | Architect |
| **To** | Dev (`dev-backend` / `dev-mobile` / `designer-ui`) |
| **Date** | YYYY-MM-DD |
| **Status** | draft \| ready \| accepted |

## 1. Objective
One paragraph: the outcome this work must produce and why it matters now.

## 2. Scope
**In scope:**
- ...

**Out of scope (do not build):**
- ...

## 3. Technical approach
Layer-by-layer plan (Domain → Application → Infrastructure → Web / Mobile), with the
files to create or change. Reference the design spec instead of repeating it.

## 4. Contracts
API routes, DTO shapes (camelCase), events, or UI states this work introduces or changes.

## 5. Constraints
- Architecture rules that apply (dependency direction, EF Core patterns, DI registration).
- Security/authorization requirements (roles, gating, secrets policy).
- Process rules: build-only validation (`AGENTS.md` §6), rtk prefix (`AGENTS.md` §7).

## 6. Acceptance criteria
Link to `docs/specs/<feature-slug>/requisitos.md` and list any brief-specific criteria.

## 7. Risks and open questions
Known risks, trade-offs already decided (with the rationale), and questions the dev
must raise before coding if unclear.

## 8. Definition of done
- [ ] Solution builds green (`rtk dotnet build HubEsportesLages.slnx`)
- [ ] Spec tasks checked off in `tarefas.md`
- [ ] `02-dev-handoff.md` written for QA
