# Handoff Protocol — Bora pro Jogo (Hub Esportes Lages)

> **Language standard: all engineering documentation is written in American English.**
> Product UI text and domain code identifiers remain in Brazilian Portuguese (the product's
> ubiquitous language). See `AGENTS.md` §8.

## Purpose
Every feature moves through a chain of explicit, written handoffs. No role starts work
without reading the document addressed to it, and no role finishes work without writing
the document for the next role. This keeps context transferable across humans, AI agents,
and future roles (PM, analyst, scrum master).

## The chain

```
ARCHITECT ──01-architect-brief.md──▶ DEV ──02-dev-handoff.md──▶ QA ──03-qa-report.md──▶ ARCHITECT
   ▲                                                                                        │
   └────────────────────────── reads the QA report, decides next step ─────────────────────┘
```

| # | Document | Written by | Read by | Answers |
|---|---|---|---|---|
| 01 | `01-architect-brief.md` | Architect | Dev (backend/mobile/UI) | *What must be built, why, and within which constraints?* |
| 02 | `02-dev-handoff.md` | Dev | QA (code reviewer) | *What was built, how, and what deserves scrutiny?* |
| 03 | `03-qa-report.md` | QA | Architect | *Does it meet the spec? What was found? Ship or fix?* |

## Location and naming
```
docs/handoffs/<feature-slug>/01-architect-brief.md
docs/handoffs/<feature-slug>/02-dev-handoff.md
docs/handoffs/<feature-slug>/03-qa-report.md
```
`<feature-slug>` matches the spec folder under `docs/specs/` (e.g., `lgpd`, `ingresso-qr`).
Templates live in `docs/handoffs/_templates/` — copy, do not edit the templates.

## Rules
1. **American English**, concise, imperative mood ("Add X", not "X could be added").
2. Every handoff has the front-matter header (feature, from, to, date, status) filled in.
3. Handoffs **reference** specs (`docs/specs/<feature>/`) — they do not duplicate them.
   In a conflict, the spec wins (SDD, `AGENTS.md` §0).
4. A handoff is a **contract**: the receiving role may reject it as *incomplete* and send it
   back rather than guessing.
5. Status values: `draft` → `ready` → `accepted` (or `rejected: <reason>`).
6. Dates in ISO format (`2026-07-01`).

## Relationship to specs (SDD)
Specs (`requisitos/design/tarefas`) describe the **feature**; handoffs describe the
**transfer of work** at a point in time. Specs are timeless and updated in place;
handoffs are historical records and are never rewritten after acceptance.
