# Tarefas — Palpite do Jogo (placar)

> Marque ao concluir. "Done" = critérios de aceite de `requisitos.md` atendidos. Agente sugerido entre [].
> **Reusa** a infra da spec base `docs/specs/interacao-torcida-ao-vivo/` (identidade `X-Torcedor-Id`,
> `TorcidaHub`, middleware/contexto, anti-abuso). **Não recriar** essas peças — só estender.

## Fase 1 — REST persistido (sem tempo real)
- [ ] **Domain**: entidade `PalpitePlacar` { Id, EventoId, TorcedorId, GolsCasa, GolsVisitante, CriadoEm,
      AtualizadoEm }; adicionar `Evento.AceitaPalpite => Status == Agendado` (gating **inverso**, sem mexer em
      `AceitaInteracao`). [dev-backend]
- [ ] **Infrastructure**: `HubDbContext` — `DbSet<PalpitePlacar>` + Fluent API com **índice único**
      `(EventoId, TorcedorId)`, `TorcedorId` max 64, validação 0–99 em gols. [dev-backend]
- [ ] **Application**: DTOs camelCase (`PalpiteEstadoDto`, `PalpiteDto`, `PalpiteAgregadoDto`,
      `RegistrarPalpiteDto` com `Range(0,99)`), interface `IPalpiteService`, mapeamentos em
      `MapeamentoExtensions`. (Reusar `ITorcedorContexto`.) [dev-backend]
- [ ] **Infrastructure**: `PalpiteService` — **upsert** por `(EventoId,TorcedorId)` com gating
      `AceitaPalpite` (409 fora da janela), agregação `GROUP BY` placar + comparação casa/empate/visitante
      (top N + "outros"); registrar no `DependencyInjection`. [dev-backend]
- [ ] **Infrastructure**: `DataSeeder` — palpites de exemplo para o evento **Agendado** (agregado não-vazio). [dev-backend]
- [ ] **Web/API**: `PalpitesApiController` — `GET /api/eventos/{slug}/palpites` (liberado),
      `PUT .../palpites/meu` (200/409/422, **gating Agendado**), `DELETE .../palpites/meu` (opcional);
      Swagger ok. [dev-backend]
- [ ] **Mobile**: card "Qual vai ser o placar de hoje?" com os 3 estados por status (Agendado=editável,
      AoVivo/Encerrado=leitura, Adiado/Cancelado=oculto), rótulos dos times do evento. [designer-ui / dev-mobile]
- [ ] **Mobile**: `PalpitePlacarViewModel` via REST — carrega estado, registra/edita palpite (steppers 0–99,
      UI otimista, trava em 409), renderiza agregado (barra de comparação + placares mais cravados);
      estados loading/erro/vazio. [dev-mobile]

## Fase 2 — Tempo real (reusa TorcidaHub)
- [ ] **Web**: `PalpiteService` publica `PalpitesAtualizados(PalpiteAgregadoDto)` no grupo `evento-{id}`
      via `IHubContext<TorcidaHub>` após cada escrita (sem hub novo). [dev-backend]
- [ ] **Mobile**: assinar `PalpitesAtualizados` no cliente SignalR já existente (conectar ao abrir,
      `WithAutomaticReconnect`, desconectar no `OnDisappearing`); **fallback** `PeriodicTimer` (~5s) quando offline. [dev-mobile]

## Fase 3 — Robustez e extras
- [ ] **Anti-abuso**: validação 0–99 + idempotência por índice único + rate limit por torcedor (reusa o da spec base). [dev-backend]
- [ ] **(Futuro/opcional) Acerto do palpite**: comparar palpite com placar final no `Encerrado` e marcar quem
      cravou (depende da lacuna #4 — gamificação). [dev-backend]
- [ ] **Testes** cobrindo os critérios de aceite: 1 palpite por (evento,torcedor) com upsert; **gating
      inverso** (409 quando NÃO `Agendado`); validação 0–99 (422); agregação de percentuais; atualização
      em tempo real ≤ 2s; degradação graciosa sem SignalR. [dev-backend]
- [ ] **Spec e código revisados** (não divergem da spec base; gating correto = `Agendado`). [revisor-codigo]
