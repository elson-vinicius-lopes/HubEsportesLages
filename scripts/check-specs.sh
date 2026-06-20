#!/usr/bin/env bash
# Valida a estrutura de Spec-Driven Development do repositório:
#  - toda feature em docs/specs/ tem requisitos.md, design.md e tarefas.md
#  - os adaptadores de regra por IDE ainda apontam para o AGENTS.md canônico
set -euo pipefail

fail=0

# (a) Estrutura das specs
if compgen -G "docs/specs/*/" > /dev/null; then
  for dir in docs/specs/*/; do
    [ "$dir" = "docs/specs/_template/" ] && continue
    for f in requisitos.md design.md tarefas.md; do
      if [ ! -f "$dir$f" ]; then echo "FALTA: $dir$f"; fail=1; fi
    done
  done
fi

# (b) Adaptadores não divergiram (apontam para AGENTS.md)
[ -f AGENTS.md ] || { echo "FALTA: AGENTS.md (acordo canônico)"; fail=1; }
grep -q "@AGENTS.md" CLAUDE.md 2>/dev/null || { echo "CLAUDE.md não importa @AGENTS.md"; fail=1; }
grep -q "AGENTS.md" .github/copilot-instructions.md 2>/dev/null || { echo ".github/copilot-instructions.md não aponta para AGENTS.md"; fail=1; }

if [ "$fail" -ne 0 ]; then
  echo "Validação de specs FALHOU."
  exit 1
fi
echo "Specs e adaptadores OK."
