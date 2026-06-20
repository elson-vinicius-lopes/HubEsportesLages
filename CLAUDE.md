# CLAUDE.md

As regras de trabalho deste repositório são canônicas no `AGENTS.md`. Importe-as:

@AGENTS.md

**Spec-Driven Development:** a fonte da verdade são as specs em `docs/`. Antes de
trabalhar no app mobile Arena Lages, leia `docs/design-arena-lages.md` (não importado
aqui de propósito — tem ~550 linhas; abra sob demanda). Specs por feature ficam em
`docs/specs/<feature>/{requisitos,design,tarefas}.md`.

Em conflito entre código e spec, a spec prevalece.

Não duplique regras aqui — para mudar o acordo de trabalho, edite o `AGENTS.md`.
Comandos e subagentes específicos do Claude Code ficam em `.claude/commands/` e `.claude/agents/`.
