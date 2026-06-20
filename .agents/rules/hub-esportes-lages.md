# Regras — Hub Esportes Lages (Google Antigravity)

O acordo de trabalho canônico está em `AGENTS.md` na raiz do workspace — leia e siga-o.
Ele cobre o princípio de Spec-Driven Development, a arquitetura (.NET 10 Clean Architecture
+ app MAUI Arena Lages), os comandos de build/run via `HubEsportesLages.slnx` e a localização
das specs em `docs/`.

- Fonte da verdade do app mobile: `docs/design-arena-lages.md`.
- Specs por feature: `docs/specs/<feature>/{requisitos,design,tarefas}.md`.

Em conflito entre código e spec, a spec prevalece. Mantenha esta regra curta;
não duplique o conteúdo do AGENTS.md aqui.

> Antigravity lê o `AGENTS.md` da raiz do **workspace aberto**. Se abrir só a pasta
> `src/HubEsportesLages.Mobile`, crie ali um `AGENTS.md` apontando para `../../AGENTS.md`.
> MCP é configurado fora do repo, na UI ("View raw config").
