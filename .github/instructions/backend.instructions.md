---
applyTo: "src/HubEsportesLages.Domain/**,src/HubEsportesLages.Application/**,src/HubEsportesLages.Infrastructure/**,src/HubEsportesLages.Web/**"
---
Backend .NET 10 (Clean Architecture). Respeite a direção de dependência
Domain ← Application ← Infrastructure ← Web. Use EF Core com `AsNoTracking`/`Include`
e registre serviços no DI. Publique notificações ao criar evento/atualizar placar.
Regras completas em [`AGENTS.md`](../../AGENTS.md); specs em `docs/`.
