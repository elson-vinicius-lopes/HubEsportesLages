# Tarefas — Esquenta da Torcida (Pontos de Encontro)

> Marque ao concluir. "Done" = critérios de aceite de `requisitos.md` atendidos. Agente sugerido entre [].
> Reutiliza a infra da spec irmã `docs/specs/interacao-torcida-ao-vivo/` (identidade `X-Torcedor-Id`,
> `TorcidaHub`, `ResultadoInteracao`/`StatusInteracao`, gating por `Evento.Status`). **Não duplicar.**

## Fase 1 — REST persistido (sem tempo real)
- [ ] **Domain**: entidades `PontoEncontro` (com `MapaUrl` calculado no padrão de `Local`) e
      `PresencaPontoEncontro`; propriedade `Evento.AceitaEsquenta => Status == Agendado`. [dev-backend]
- [ ] **Infrastructure**: `HubDbContext` — `DbSet`s + Fluent API; **índice único
      `(PontoEncontroId, TorcedorId)`** em `PresencaPontoEncontro`; `Ignore(PontoEncontro.MapaUrl)` e
      `Ignore(Evento.AceitaEsquenta)`; FKs (Cascade via ponto, `Evento` sem cascade duplo). [dev-backend]
- [ ] **Application**: DTOs (`EsquentaEstadoDto`, `PontoEncontroDto`, `CriarPontoEncontroDto`,
      `AtualizarPontoEncontroDto`); interfaces `IEsquentaService` e `IEsquentaAdminService`; mapeamentos +
      helper de distância (`DistanciaTexto`). [dev-backend]
- [ ] **Infrastructure**: `EsquentaService` (estado agregado ordenado por horário/ordem/distância;
      `presencas` por GROUP BY; `confirmadoPorMim`; Haversine ponto→`Local`; confirmar/cancelar idempotentes
      com gating `AceitaEsquenta`) + `EsquentaAdminService` (CRUD de pontos) + registro no
      `DependencyInjection`. [dev-backend]
- [ ] **Infrastructure**: `DataSeeder` — semear 2–3 pontos de encontro para um evento **`Agendado`**
      (com lat/long de exemplo para gerar distância). [dev-backend]
- [ ] **Web/API torcedor**: `EsquentaApiController` (`GET …/esquenta`; `POST`/`DELETE …/{pontoId}/presenca`)
      com **gating `Agendado` nas escritas (409 fora do pré-jogo)**, 400 sem `X-Torcedor-Id`, 404/422.
      Swagger ok. [dev-backend]
- [ ] **Web/API admin**: `EsquentaAdminApiController` (`POST`/`PUT`/`DELETE …/pontos`) sem gating de status.
      Swagger ok. [dev-backend]
- [ ] **Mobile**: seção "Esquenta da Torcida" na `EventDetailPage` (visível em `Agendado`; modo leitura nos
      demais status), com card de ponto (nome, horário, endereço, distância, descrição, regras, contador).
      [dev-mobile / designer-ui]
- [ ] **Mobile**: `EsquentaViewModel` via REST (carrega estado; "Ver Rota" via `Launcher`/`mapaUrl`;
      confirmar/cancelar com UI otimista + trava por `confirmadoPorMim`; header `X-Torcedor-Id`;
      estados loading/erro/vazio; trata 409/400). [dev-mobile]

## Fase 2 — Tempo real (reusa `TorcidaHub`)
- [ ] **Web**: `EsquentaService` publica `PresencaAtualizada(pontoId, total)` no grupo `evento-{id}` via
      `IHubContext<TorcidaHub>` após confirmar/cancelar. **Depende** do `TorcidaHub` (Fase 2 da spec irmã);
      **não criar novo hub**. [dev-backend]
- [ ] **Mobile**: cliente `SignalR.Client` no `TorcidaHub` (conectar ao grupo do evento, handler
      `PresencaAtualizada`, `WithAutomaticReconnect`, desconectar no `OnDisappearing`); fallback polling
      (~10 s) quando offline. [dev-mobile]

## Fase 3 — Robustez e extras
- [ ] **Anti-abuso**: rate limit em confirmar/cancelar por torcedor; validação de tamanho dos campos de
      ponto (admin). [dev-backend]
- [ ] **(Opcional)** Lembrete: gerar `Notificacao` do esquenta para quem confirmou presença (reusa
      `NotificacaoService`). [dev-backend]
- [ ] **Testes** cobrindo os critérios de aceite: 1 presença por ponto/torcedor (idempotência), **gating de
      pré-jogo** (409 fora de `Agendado`), cancelar idempotente, distância formatada, leitura liberada em
      qualquer status, CRUD admin. [dev-backend]
- [ ] **Spec e código revisados** (não divergem; reuso da infra compartilhada confirmado). [revisor-codigo]
