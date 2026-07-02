# Architect Brief — LGPD Compliance

| | |
|---|---|
| **Feature** | `lgpd` (spec: `docs/specs/lgpd/`) |
| **From** | Architect |
| **To** | Dev (`dev-backend`) |
| **Date** | 2026-07-01 |
| **Status** | accepted |

## 1. Objective
Make the product compliant with LGPD (Brazilian data-protection law) before the presentation
to the Lages Department of Education: explicit consent at signup, a public privacy policy,
and data-subject rights (export and account deletion). Institutional credibility depends on it.

## 2. Scope
**In scope:** consent checkbox (site and API signup) persisted with date/version; `/privacidade`
policy page linked in the footer; "export my data" (JSON download); "delete my account"
(password-confirmed, last-admin guard, ticket anonymization); data-minimization note on the
alerts form; **security gap fix**: `[Authorize(Roles="Admin")]` on `TorcidaAdminApiController`
and `[Authorize]` on `FavoritosApiController`.

**Out of scope:** real e-mail provider, consent re-prompt on policy change, DPO tooling.

## 3. Technical approach
`ApplicationUser` gains `ConsentimentoLgpdEm`/`ConsentimentoVersao` (+ EF migration).
New `ILgpdService`/`LgpdService` (export + deletion with `ExecuteDelete/ExecuteUpdate`).
`PrivacidadeController` + static policy view (pt-BR — product UI). Account endpoints on
`ContaController`; API consent on `AuthApiController.Registrar`.

## 4. Constraints
Build-only validation (`AGENTS.md` §6); rtk prefix (§7); Clean Architecture dependency
direction; camelCase API; migration applied by existing `MigrateAsync` at user-run startup.

## 5. Acceptance criteria
See `docs/specs/lgpd/requisitos.md`. Key: signup without consent is rejected (site and API);
deletion requires the account password; the only remaining Admin cannot self-delete;
export returns only the requesting user's data.

## 6. Definition of done
- [x] Solution builds green
- [x] Migration generated (`ConsentimentoLgpd`)
- [x] `02-dev-handoff.md` written
