# Instruções do Copilot — Hub Esportes Lages

O acordo de trabalho canônico está em [`AGENTS.md`](../AGENTS.md) na raiz.
**Leia e siga o AGENTS.md** — ele descreve o princípio de Spec-Driven Development,
a arquitetura (.NET 10 Clean Architecture + app MAUI Arena Lages), os comandos de
build/run via `HubEsportesLages.slnx` e onde ficam as specs.

A fonte da verdade muda conforme a área:
- App mobile Arena Lages: [`docs/design-arena-lages.md`](../docs/design-arena-lages.md).
- Features: `docs/specs/<feature>/{requisitos,design,tarefas}.md`.

Em conflito entre código e spec, **a spec prevalece**.

Dica: habilite a leitura do AGENTS.md em Settings → `chat.useAgentsMdFile`.
Para regras por área, veja `.github/instructions/*.instructions.md`.
