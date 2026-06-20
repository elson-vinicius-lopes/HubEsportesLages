---
description: Publica um novo evento na agenda do hub (via API se estiver rodando, ou no seed).
argument-hint: "<descrição do evento> (ex.: \"Final do Citadino de Futsal, dia 30/07 20h no Jones Minosso\")"
---

Cadastre o evento descrito em: **$ARGUMENTS**.

Escolha o caminho conforme o objetivo:

**A) Evento real (persistido agora)** — preferível se a app já estiver no ar:
- Descubra os IDs de modalidade/local/equipes em `/api/catalogo/...`.
- `POST http://localhost:5210/api/eventos` com o corpo `CriarEventoDto` (título **sem acentos** no
  terminal). Confirme o 201 e a notificação automática de "novo evento" em `/api/notificacoes`.
- Se houver placar/encerramento, use `PUT /api/eventos/{id}/resultado`.

**B) Evento de demonstração (no seed)** — para fazer parte do cenário inicial:
- Adicione uma chamada ao helper `Novo(...)` em `DataSeeder.cs` (modalidade, local, equipes, data
  relativa a `hoje`, status, placar/destaque conforme o caso).
- Apague `src/HubEsportesLages.Web/hubesportes.db` e rode a app para re-semear.

Em ambos: compile (`dotnet build HubEsportesLages.slnx`) e valide que o evento aparece na agenda
(`/api/eventos`) e na página `/Agenda`.
