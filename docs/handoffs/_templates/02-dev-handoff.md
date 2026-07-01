# Dev Handoff — <Feature Name>

| | |
|---|---|
| **Feature** | `<feature-slug>` (brief: `01-architect-brief.md`) |
| **From** | Dev (`dev-backend` / `dev-mobile` / `designer-ui`) |
| **To** | QA (`revisor-codigo`) |
| **Date** | YYYY-MM-DD |
| **Build** | green \| red (`rtk dotnet build HubEsportesLages.slnx`) |
| **Status** | draft \| ready \| accepted |

## 1. What was implemented
Bullet list of delivered behavior, mapped to the brief's scope.

## 2. Files created / changed
| File | Change |
|---|---|
| `src/...` | ... |

## 3. How it works
Short technical narrative: flow of a request/interaction through the layers.
Mention migrations generated, DI registrations, and configuration keys added.

## 4. Deviations from the brief
Anything done differently than specified, and why. "None" if fully compliant.

## 5. How to verify
Steps QA can run **without starting the app** (build, migration inspection, code paths).
If runtime verification is required, write the steps for the **user** to execute
(see `README.md` run manual) — agents must not leave the app running (`AGENTS.md` §6).

## 6. Known limitations and technical debt
Honest list: edge cases not covered, follow-ups deferred, TODOs left in code.

## 7. Suggested QA focus
Where bugs are most likely: concurrency, authorization, validation, data integrity.
