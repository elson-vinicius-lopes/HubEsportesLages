# Dev Handoff — LGPD Compliance

| | |
|---|---|
| **Feature** | `lgpd` (brief: `01-architect-brief.md`) |
| **From** | Dev (`dev-backend`) |
| **To** | QA (`revisor-codigo`) |
| **Date** | 2026-07-01 |
| **Build** | green (5 projects, 0 errors, 0 warnings) |
| **Status** | accepted |

## 1. What was implemented
Consent at signup (site + API), public privacy policy page, data export, password-confirmed
account deletion with last-admin guard, minimization note on the alerts form, and the
authorization gap fixes on the crowd-admin and favorites API controllers.

## 2. Files created / changed
| File | Change |
|---|---|
| `Application/Common/LgpdConstantes.cs` | Policy version (`v1`), anonymized-name constant (new) |
| `Application/DTOs/LgpdDtos.cs`, `Interfaces/ILgpdService.cs` | Export/deletion contracts (new) |
| `Infrastructure/Services/LgpdService.cs` | Export by e-mail; deletion via `ExecuteDeleteAsync` (alert subscriptions) + `ExecuteUpdateAsync` (ticket anonymization) (new) |
| `Infrastructure/Identidade/ApplicationUser.cs` | + `ConsentimentoLgpdEm`, `ConsentimentoVersao` |
| `Infrastructure/Migrations/20260701210503_ConsentimentoLgpd.cs` | Two nullable columns on `AspNetUsers` (new) |
| `Web/Controllers/PrivacidadeController.cs` + `Views/Privacidade/Index.cshtml` | `GET /privacidade` (new) |
| `Web/Controllers/ContaController.cs` | Signup requires consent; `GET /conta/meus-dados`; `POST /conta/excluir` |
| `Web/Controllers/Api/AuthApiController.cs` | `AceitePrivacidade` required on API signup (400 otherwise) |
| `Web/Controllers/Api/TorcidaAdminApiController.cs` | `[Authorize(Roles="Admin")]` (gap fix) |
| `Web/Controllers/Api/FavoritosApiController.cs` | `[Authorize]` (cookie or JWT) |
| `Web/Program.cs` | Unauthenticated `/api` now returns 401/403 instead of login-page redirect |
| `Web/wwwroot/js/torcida.js` | Translates 401/403 to a friendly sign-in message |
| Views: `Registrar`, `Conta/Index`, `Notificacoes/Index`, `_Layout` | Consent checkbox, privacy panel, minimization note, footer link |

## 3. Deviations from the brief
None in scope. Two decisions recorded in `docs/specs/lgpd/design.md`: `CompradorId`
(stores an e-mail — personal data) is replaced with a `removido:<guid>` marker on deletion,
so accounting records survive without personal data and a future signup with the same e-mail
does not inherit old tickets; anonymous crowd interactions (browser `TorcedorId`) are outside
export/deletion by construction, and the policy says so.

## 4. How to verify
Static: `rtk dotnet build HubEsportesLages.slnx`; inspect the migration and the authorize
attributes. Runtime (user-run per `README.md`): sign up without the checkbox (must fail),
export from the account panel, delete a non-admin account, try deleting the only admin (must
be blocked). Note for mobile: the app must send `aceitePrivacidade: true` on registration.

## 5. Suggested QA focus
Deletion integrity (no broken FKs after `ExecuteDelete/ExecuteUpdate`), consent bypass
attempts on both signup paths, authorization on the previously open admin endpoints.
