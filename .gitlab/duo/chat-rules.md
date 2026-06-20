# Regras do GitLab Duo — Hub Esportes Lages

O acordo de trabalho canônico está em `AGENTS.md` na raiz do repositório — leia e siga-o.
Cobre o princípio de Spec-Driven Development, a arquitetura (.NET 10 Clean Architecture +
app MAUI Arena Lages), os comandos de build/run via `HubEsportesLages.slnx` e a localização
das specs em `docs/`.

- Fonte da verdade do app mobile: `docs/design-arena-lages.md`.
- Specs por feature: `docs/specs/<feature>/{requisitos,design,tarefas}.md`.
- Ao revisar ou implementar, valide o código contra os critérios de aceite da spec.

Em conflito entre código e spec, a spec prevalece.
