# Tarefas — Disputa entre Torcidas (Cabo de Guerra)

> Marque ao concluir. "Done" = critérios de aceite de `requisitos.md` atendidos. Agente sugerido entre [].
> **Estende** `docs/specs/interacao-torcida-ao-vivo/` — reusa hub, identidade e gating; **não** recria essa infra.
> Pré-requisito: Fase 1 da base (REST persistido + `ITorcedorContexto` + middleware `X-Torcedor-Id`) concluída.

## Fase 1 — REST persistido (sem tempo real)
- [ ] **Domain**: enum `LadoTorcida` (`Casa=0`, `Visitante=1`); entidade `ApoioTorcida`
      { Id, EventoId, Lado, TorcedorId (max 64), CriadoEm }; (opcional) helper `Evento.AceitaDisputa`. [dev-backend]
- [ ] **Infrastructure**: `HubDbContext` — `DbSet<ApoioTorcida>` + Fluent API com **índice único**
      `(EventoId, TorcedorId)`, `Lado` como int, FK `EventoId` cascade. [dev-backend]
- [ ] **Application**: DTOs `DisputaDto`, `LadoTorcidaDto`, comando `DeclararApoioDto`; interface
      `IDisputaTorcidasService`; mapeamentos (Equipe→`LadoTorcidaDto`, cálculo de `percentual` com guarda de `total==0`). [dev-backend]
- [ ] **Infrastructure**: `DisputaTorcidasService` — `ObterAsync` (agrega por `GROUP BY Lado`, marca `meuLado`,
      trata não-confronto), `DeclararApoioAsync` (gating `409`/`422`, insert idempotente por índice único, recalcula). [dev-backend]
- [ ] **Infrastructure/DI**: registrar `IDisputaTorcidasService`; (opcional) seed de `ApoioTorcida` de exemplo
      para o evento AoVivo de confronto. [dev-backend]
- [ ] **Web/API**: `GET /api/eventos/{slug}/torcida/disputa` (leitura, qualquer status) e
      `POST /api/eventos/{slug}/torcida/disputa/apoio` { lado } com **gating AoVivo (409) + confronto (422)**; Swagger. [dev-backend]
- [ ] **Mobile**: card "Disputa entre Torcidas" na `InteractionPage` (só se `ehConfronto`), com os 3 estados por
      status (AoVivo/Agendado/Encerrado), barra cabo de guerra (lima × rosa) e 2 botões "Apoiar". [designer-ui / dev-mobile]
- [ ] **Mobile**: `DisputaViewModel` via REST — `CarregarCommand`, `ApoiarCasa/VisitanteCommand`
      (UI otimista + trava após apoiar), estado `MeuLado`/`PodeApoiar`; estados loading/erro/vazio. [dev-mobile]
- [ ] **Mobile**: `CompartilharCommand` via `Share.RequestAsync` com o texto do placar parcial. [dev-mobile]

## Fase 2 — Tempo real (reusa o TorcidaHub)
- [ ] **Web**: `DisputaTorcidasService` publica `DisputaAtualizada(DisputaDto)` no grupo `evento-{id}`
      via `IHubContext<TorcidaHub>` após cada apoio (sem hub/grupo novos). [dev-backend]
- [ ] **Mobile**: assinar `DisputaAtualizada` na conexão SignalR já aberta pela `InteractionViewModel`
      (atualiza percentuais ≤ 2s); fallback no `PeriodicTimer` de refresh da base quando offline. [dev-mobile]
- [ ] **Mobile**: animar a largura dos segmentos da barra ao receber atualização (efeito "puxão"). [designer-ui / dev-mobile]

## Fase 3 — Robustez e testes
- [ ] **Testes** cobrindo os critérios de aceite: 1 apoio por torcedor/evento (idempotência por índice único);
      gating `409` fora de AoVivo; `422` em evento não-confronto; `50/50` com zero apoios; `%` correto e somando 100;
      card oculto quando `ehConfronto=false`; atualização em tempo real ≤ 2s. [dev-backend / dev-mobile]
- [ ] **Spec e código revisados** (não divergem da base nem desta spec). [revisor-codigo]
