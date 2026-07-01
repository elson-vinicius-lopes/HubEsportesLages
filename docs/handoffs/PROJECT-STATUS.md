# PROJECT-STATUS — Bora pro Jogo (Hub Esportes Lages)

> **Audience:** every current and future role on this project — architect, dev (backend/mobile/UI),
> QA, PM, analyst, scrum master. This is the single English-language record of what has been
> built so far, how it is organized, and what comes next.
>
> **Date:** 2026-07-01 · **Branch:** `master` · **Latest commit:** `fefa0f7` (JWT Bearer auth).
> LGPD compliance work is in the working tree, not yet committed.
>
> Per SDD (`AGENTS.md` §0), the specs in `docs/specs/` remain the source of truth for each
> feature. This document is a status snapshot, not a spec. Claims below were verified against
> the repository; anything that could not be confirmed is marked "(unconfirmed)".

**Glossary (Portuguese domain terms, kept as the product's ubiquitous language):**
*torcedor* = fan; *torcida* = fan crowd/fanbase; *ingresso* = ticket; *enquete* = poll;
*mural* = message wall; *palpite* = score prediction; *esquenta* = pre-game fan meetup.

---

## 1. Executive summary

**Bora pro Jogo** (repo: Hub Esportes Lages) is the sports agenda, notification, and fan-experience
hub for the city of Lages/SC, Brazil — built for the **HackathOrion** challenge *"Improve the fan
experience at Lages sporting events"* (`docs/desafio.md`). It is a .NET 10 Clean Architecture web
app (MVC site + camelCase REST API + Swagger) on PostgreSQL 16, with ASP.NET Identity, JWT Bearer
for the API, live crowd interaction (MVP vote, poll, message wall, favorites), and a paid QR-code
ticket flow with simulated Pix payment and admin gate validation. Current state: the web product is
feature-complete for demo; **LGPD compliance is in progress right now** (code done, uncommitted);
the Arena Lages mobile app and four engagement features are fully specified but not yet built.

---

## 2. Architecture overview

Monorepo, solution `HubEsportesLages.slnx`, **.NET 10**, Clean Architecture with strict dependency
direction **Domain ← Application ← Infrastructure ← Web** (never inverted).

| Project | Responsibility |
|---|---|
| `src/HubEsportesLages.Domain` | Entities and enums, zero dependencies: `Evento`, `Modalidade`, `Equipe`, `Local`, `Inscricao`, `Notificacao`, `Ingresso`, plus crowd entities (`Enquete`, `OpcaoEnquete`, `VotoEnquete`, `VotoMvp`, `JogadorEvento`, `MensagemTorcida`, `EquipeFavorita`) |
| `src/HubEsportesLages.Application` | DTOs, service interfaces (`IEventoService`, `ICatalogoService`, `IInscricaoService`, `INotificacaoService`, `ITorcidaService`, `IIngressoService`, `IPagamentoService`, `ITokenIngresso`, `ILgpdService`), mappings |
| `src/HubEsportesLages.Infrastructure` | EF Core 10 + **PostgreSQL (Npgsql 10.0.2)**, `HubDbContext` (inherits `IdentityDbContext<ApplicationUser>`), EF Migrations, service implementations, `DataSeeder` + `IdentidadeSeeder`, DI registration, **QRCoder 1.8.0** |
| `src/HubEsportesLages.Web` | MVC site (Razor + own CSS, no front-end build pipeline) + REST API + Swagger + **Serilog** + `NotificacaoLembreteWorker` (reminder background service) + `TorcedorIdentidadeMiddleware` |
| `src/HubEsportesLages.Mobile` | **Does not exist yet** — planned .NET MAUI app "Arena Lages" (spec: `docs/design-arena-lages.md`) |

Key infrastructure facts (all verified in code):

- **Database:** PostgreSQL 16 running in WSL2 (Ubuntu 24.04), provisioned by
  `scripts/wsl-postgres-setup.sh`. Migrations `20260701144931_InicialPostgres` and
  `20260701210503_ConsentimentoLgpd` create the schema; seed runs on first start.
  The project started on SQLite and was migrated to PostgreSQL on 2026-07-01.
- **Identity:** ASP.NET Core Identity (`ApplicationUser : IdentityUser` + `NomeCompleto` +
  LGPD consent fields), roles `Admin` and `Torcedor`, strong-password policy and lockout,
  cookie auth for the MVC site (`/conta/login`, separate `/admin/login` redirect).
- **JWT:** `Microsoft.AspNetCore.Authentication.JwtBearer` added **alongside** the Identity
  cookie (dual-scheme default authorization policy in `Program.cs`), HMAC-SHA256, issuer/audience/
  lifetime/signing-key validation, 30 s clock skew, `POST /api/auth/login` issues the token.
- **Logging:** Serilog to console + daily rolling file in `logs/` (30-file retention),
  `Log.Fatal` wrapper around startup.
- **QR codes:** server-side via QRCoder (payment QR and ticket QR, PNG → base64);
  admin scanner uses vendored `html5-qrcode` in `wwwroot/lib` with a manual-entry fallback.
- **Anonymous fan identity:** `X-Torcedor-Id` header (device GUID) resolved by
  `TorcedorIdentidadeMiddleware` → `ITorcedorContexto`; one-vote idempotency enforced by
  unique DB indexes, not the UI.
- **API contract:** all JSON in **camelCase**; endpoint inventory in `README.md` and `AGENTS.md` §2.

> Known doc debt: `AGENTS.md` §2/§3 still says "EF Core 10 + SQLite"; the code and `README.md`
> are on PostgreSQL. `AGENTS.md` should be updated (spec/doc divergence).

---

## 3. Feature inventory

Status legend — **shipped-verified:** implemented, spec tasks checked, manually validated (there is
no automated test project yet); **shipped:** implemented in code, but the spec's task checklist was
not updated and/or no recorded verification; **in-progress:** being built right now;
**spec-only:** specification exists, no code.

| Feature | Status | Spec | Notes |
|---|---|---|---|
| Agenda, results, event detail, notifications, alert subscriptions, catalog | shipped-verified | API contract in `AGENTS.md` §2 (predates per-feature spec folders) | Core of the first delivery (2026-06-20). Filters, pagination, slug detail, reminder worker. |
| Live crowd interaction — MVP ("player of the match") vote, poll (*enquete*), message wall (*mural*), favorite team | shipped | [`docs/specs/interacao-torcida-ao-vivo/`](../specs/interacao-torcida-ao-vivo/) | Phase 1 (REST, no real time) is implemented: `TorcidaService`, `TorcidaApiController`, `TorcidaAdminApiController`, web views + `torcida.js`, `IModeracaoService`, event-status gating (409 outside `AoVivo`). **Divergence:** `tarefas.md` checkboxes were never marked. Phase 2 (SignalR `TorcidaHub`) **not started** — no SignalR code in `src/`. |
| QR-code paid ticket (*ingresso*) with simulated Pix + admin gate validation | shipped | [`docs/specs/ingresso-qr/`](../specs/ingresso-qr/) | Full flow: buy (`Pendente`) → fake Pix QR → confirm (simulated, always approves) → `Pago` + HMAC-signed token + ticket QR → admin single-use check-in (`Utilizado`). `MockPixPagamentoService` is behind `IPagamentoService` for a real provider swap. `tarefas.md` checkboxes not marked. See §4 for the check-in race debt. |
| ASP.NET Identity authentication (strong password, lockout, roles) | shipped-verified | [`docs/specs/auth-identity/`](../specs/auth-identity/) | T1–T12 checked. Replaced the old hard-coded `admin`/`lages2026` login. Seeded admin `elsouzalopes@gmail.com` (see §4 on the fallback password). New sign-ups get role `Torcedor`. |
| JWT Bearer auth for the REST API | shipped-verified | [`docs/specs/auth-jwt/`](../specs/auth-jwt/) | Commit `fefa0f7` (2026-07-01). Dual-scheme policy keeps cookie auth working on the site; Swagger has a Bearer security definition; `/api` returns 401/403 JSON-style instead of login-page redirects. |
| PostgreSQL migration (SQLite → PostgreSQL 16 on WSL) | shipped | no dedicated spec folder | Verified via `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.2`, migration `InicialPostgres` (2026-07-01), `scripts/wsl-postgres-setup.sh`, and the `README.md` run manual. `Npgsql.EnableLegacyTimestampBehavior` switch is on because the seeder uses local `DateTime`. |
| **LGPD compliance** (Brazilian data-protection law 13.709/2018) | **in-progress (right now)** | [`docs/specs/lgpd/`](../specs/lgpd/) | Code complete in the working tree (uncommitted): consent checkbox + `aceitePrivacidade` on both sign-up paths, consent timestamp/version on the user, `/privacidade` policy page + footer link, data export `GET /conta/meus-dados` (JSON), account deletion with password confirmation + last-admin guard + ticket anonymization, privacy note on the alerts form, and a security fix adding `[Authorize]`/`[Authorize(Roles="Admin")]` to `FavoritosApiController` and `TorcidaAdminApiController`. Remaining: automated tests (none exist repo-wide), commit, and the handoff/QA cycle. |
| Score prediction (*palpite-placar*) | spec-only | [`docs/specs/palpite-placar/`](../specs/palpite-placar/) | Pre-game window; idempotent upsert per fan; aggregate view. |
| Fanbase duel / tug-of-war (*disputa-torcidas*) | spec-only | [`docs/specs/disputa-torcidas/`](../specs/disputa-torcidas/) | Live-only; home vs. away support bar; flagged in the roadmap as the highest "wow per effort". |
| Social photo frame (*foto-frame-social*) | spec-only | [`docs/specs/foto-frame-social/`](../specs/foto-frame-social/) | Client-side composition; minimal backend; blocked on a brand PNG asset. |
| Fan warm-up meetups (*esquenta-torcida*) | spec-only | [`docs/specs/esquenta-torcida/`](../specs/esquenta-torcida/) | Pre-game meetup points, map/route, presence confirmation; largest surface of the four. |
| Base-interaction refinements (MVP ranking, reactions + moderation queue, halftime polls) | spec-only | [`docs/specs/interacao-torcida-ao-vivo/refinamentos.md`](../specs/interacao-torcida-ao-vivo/refinamentos.md) | Addendum extending shipped entities; "almost free" once the base is done. |
| Arena Lages mobile app (.NET MAUI, dark-only, phone-first) | spec-only | [`docs/design-arena-lages.md`](../design-arena-lages.md) | Complete technical spec (screens, MAUI design tokens, navigation, API contract) derived from the Figma/React prototype in `figma/`. `src/HubEsportesLages.Mobile` not scaffolded. A PWA alternative is not mentioned anywhere in the repo (unconfirmed). |

Cross-feature engagement plan (dependency graph, gating matrix, build waves):
[`docs/specs/_roadmap-engajamento-torcida.md`](../specs/_roadmap-engajamento-torcida.md).

---

## 4. Security posture

**In place (verified in code):**

- **RBAC:** roles `Admin`/`Torcedor`; admin-only endpoints (`POST /api/ingressos/validar`,
  `TorcidaAdminApiController`, `/admin` area) use `[Authorize(Roles = "Admin")]`.
- **Password policy:** Identity strong password (8+, upper, lower, digit, special) + lockout.
- **Token signing:** JWT HMAC-SHA256 with full validation and 30 s clock skew; secret key must be
  ≥ 32 chars — the app **refuses to start in production** without `Jwt__SecretKey`. Ticket tokens
  are HMAC-signed (`ITokenIngresso`, secret from `Ingressos:Segredo`), so a QR cannot be forged
  from an ID; validation is single-use.
- **API auth behavior:** unauthenticated `/api` calls get 401/403 (never an HTML login redirect).
- **Secrets policy:** no real secrets committed. `appsettings.json` ships a deliberately empty
  `Jwt.SecretKey` and a dev placeholder for `Ingressos:Segredo`; production values come from
  environment variables (`ConnectionStrings__Default`, `Jwt__SecretKey`, `Ingressos__Segredo`,
  `Admin__SenhaInicial`, `Resend__ApiKey`) — see `README.md`.
- **Consent & data rights (LGPD, in progress):** explicit consent recorded with timestamp/version;
  data export; self-service account deletion with anonymized ticket records.

**Known debts (accepted, tracked here):**

1. **Check-in race condition.** `IngressoService.ValidarAsync` → `MarcarUtilizadoAsync`
   (`src/HubEsportesLages.Infrastructure/Services/IngressoService.cs`) is a read-then-write with
   no concurrency token on `Ingresso` (the only `IsConcurrencyToken` columns in the model are
   Identity's own `ConcurrencyStamp`). Two simultaneous validations of the same paid ticket can
   both return "entry allowed". Fix direction: `RowVersion`/`xmin` optimistic concurrency or a
   conditional UPDATE. Low practical risk at a single gate, unacceptable at scale.
2. **Admin fallback password in source.** `IdentidadeSeeder.SenhaAdminPadrao = "Admin@Lages2026"`
   is public in the repo and used when `Admin:SenhaInicial` is not configured (a warning is
   logged). Production must set `Admin__SenhaInicial` and rotate the password.
3. **Dev JWT fallback key in source.** A literal dev signing key exists in `Program.cs`; it is
   gated to the Development environment (production throws), but it is public.
4. **Hard-coded dev connection string fallback** in `Program.cs`
   (`Password=hub` for local WSL Postgres) — dev convenience, harmless only while local.
5. **Anonymous fan identity is spoofable.** `X-Torcedor-Id` is a client-supplied GUID; one device
   = one "fan", and reinstalling resets it. Accepted for the hackathon (see roadmap risk table);
   DB unique indexes cap the damage. Future: real login without breaking the header contract.
6. **Swagger is exposed in production** on purpose for the demo (`Program.cs` comment).
7. **No automated tests** anywhere in the solution — all verification is manual (see §5/§7).
8. **Simulated payment**: `MockPixPagamentoService.ConfirmarPagamento` always returns true;
   the "confirm payment" endpoint trusts the buyer. Real provider + webhook is a roadmap item.

---

## 5. Process standards

- **Spec-Driven Development (SDD).** Markdown specs in `docs/` are the source of truth; in a
  code-vs-spec conflict the spec wins. Per-feature folders
  `docs/specs/<feature>/{requisitos,design,tarefas}.md`; new features copy `docs/specs/_template/`.
  Multi-IDE strategy (Claude Code, Copilot, Antigravity, GitLab Duo consume the same spec through
  thin adapters) in [`docs/sdd-multi-ide.md`](../sdd-multi-ide.md). `AGENTS.md` is the single
  canonical working agreement.
- **Handoff protocol** ([`docs/handoffs/README.md`](README.md)): every feature moves
  Architect → `01-architect-brief.md` → Dev → `02-dev-handoff.md` → QA → `03-qa-report.md` →
  Architect. Templates in `docs/handoffs/_templates/`. No role starts without reading its inbound
  document; no role finishes without writing the outbound one. As of this snapshot no feature
  handoff folder exists yet — the protocol was just adopted; LGPD should be the first user.
- **Documentation language:** all new engineering documentation in **American English**
  (`AGENTS.md` §8). Product UI text, domain identifiers, and code comments stay in pt-BR;
  existing pt-BR specs remain until migrated.
- **AI agents never leave the app running** (`AGENTS.md` §6): agents validate with `dotnet build`
  only; the user runs the app per the README manual. A hung instance locks DLLs (MSB3027) and
  port 5210.
- **Token economy:** prefix terminal commands with `rtk` (`AGENTS.md` §7).
- **CI:** `.github/workflows/spec-ci.yml` validates spec-folder structure
  (`scripts/check-specs.sh`), rule-adapter consistency, and solution build.
- **Roles as subagents:** `.claude/agents/{architect,dev-backend,dev-mobile,designer-ui,revisor-codigo}.md`
  and project commands in `.claude/commands/`.

---

## 6. How to run

Follow the complete manual in [`README.md`](../../README.md) ("Como executar") — WSL Postgres up,
then `dotnet run --project src/HubEsportesLages.Web`; do not duplicate or shortcut it.

---

## 7. Roadmap / next steps

Agreed sequence (from [`docs/specs/_roadmap-engajamento-torcida.md`](../specs/_roadmap-engajamento-torcida.md)
§5 and feature "Futuro" sections):

1. **Finish LGPD** — commit, first full handoff cycle (brief → dev handoff → QA report).
2. **Wave 1 engagement features** (highest value / lowest marginal cost, in order):
   *disputa-torcidas* (fanbase tug-of-war), *palpite-placar* (score prediction), then base
   refinements (wall reactions + MVP ranking; halftime poll if time allows).
3. **SignalR real time** — single `TorcidaHub` at `/hubs/torcida`, groups per event, published by
   the domain services; unlocks live updates for every feature (currently REST + polling only).
4. **Wave 2:** *foto-frame-social* (needs the brand PNG asset) and *esquenta-torcida* (map,
   Haversine distance, admin CRUD).
5. **Real Pix provider** (Mercado Pago/Asaas/Efí) + payment webhook replacing
   `MockPixPagamentoService`; ticket-validation concurrency fix (§4.1) belongs with this hardening.
6. **Automated tests + CI enforcement** — create the test project, cover spec acceptance criteria
   (one-vote idempotency, status gating 409s, single-use check-in, auth 401/403), enable
   `dotnet test` in `spec-ci.yml`.
7. **Arena Lages mobile app** — scaffold `src/HubEsportesLages.Mobile` (.NET MAUI, MVVM,
   dark-only) per `docs/design-arena-lages.md`; MAUI QR scanner (`ZXing.Net.Maui`) for admins.
   (A PWA fallback has been discussed by the team but is not recorded in the repo — unconfirmed.)
8. **Presentation to the municipal Secretaria** — the QR-ticket feature is explicitly the
   showcase differentiator (`docs/specs/ingresso-qr/requisitos.md`); the repo does not name which
   secretariat (Secretary of Education per team plan — unconfirmed).
9. **Post-hackathon backlog:** gamification/points, real login replacing the anonymous fan GUID
   (keeping the `X-Torcedor-Id` contract), Redis backplane for SignalR, AI-assisted moderation,
   real e-mail provider (Resend) beyond log output.
