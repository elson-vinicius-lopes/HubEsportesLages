# QA Report — <Feature Name>

| | |
|---|---|
| **Feature** | `<feature-slug>` (dev handoff: `02-dev-handoff.md`) |
| **From** | QA (`revisor-codigo`) |
| **To** | Architect |
| **Date** | YYYY-MM-DD |
| **Build** | green \| red |
| **Verdict** | approved \| approved with issues \| rejected |

## 1. Scope reviewed
What was inspected (files, layers, flows) and what was **not** (with reason).

## 2. Verification performed
- Static review against `docs/specs/<feature-slug>/` and the architect brief
- `rtk dotnet build HubEsportesLages.slnx` result
- Adversarial checks attempted (authorization bypass, double-write race, invalid input, ...)

## 3. Findings
Ordered by severity. Every finding has a location and a concrete fix.

| # | Severity | Finding | Location | Suggested fix |
|---|---|---|---|---|
| 1 | Bug \| Risk \| Architecture \| Improvement | ... | `file:line` | ... |

## 4. Spec compliance
Acceptance criteria from `requisitos.md`, each marked **met / not met / not verifiable statically**.

## 5. Recommendation
What the architect should decide: ship as-is, fix findings #N first, or send back to dev.
Include anything that must become a follow-up spec or technical-debt entry.
