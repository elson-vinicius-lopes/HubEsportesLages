# Design — Palpite do Jogo (placar)

> O "como". Camadas e contratos concretos. Segue a Clean Architecture do `AGENTS.md`.
> **Reusa a infra compartilhada** da spec base `docs/specs/interacao-torcida-ao-vivo/design.md`
> (identidade `X-Torcedor-Id` → `ITorcedorContexto`, `TorcidaHub` em `/hubs/torcida`, padrão de gating,
> índice único para idempotência, anti-abuso). Aqui só descrevemos o que é **novo/diferente** para palpite.

## Visão geral
Feature full-stack **pré-jogo**: o torcedor registra um palpite de placar exato enquanto o evento está
`Agendado`; vê o consenso da torcida (distribuição de placares + comparação entre os times) em tempo real.
**Gating INVERSO** ao da interação ao vivo: a escrita exige `Evento.Status == Agendado` e **trava** quando
vira `AoVivo`/`Encerrado`.

```
[App MAUI: card "Qual vai ser o placar?"] --(status Agendado?)--> inputs : leitura
        │  GET  /api/eventos/{slug}/palpites          (agregado + meu palpite)
        │  PUT  /api/eventos/{slug}/palpites/meu      { golsCasa, golsVisitante }   (upsert, 200/409/422)
        └─ SignalR /hubs/torcida (grupo evento-{id}) ──> push: PalpitesAtualizados(PalpiteAgregadoDto)
```

## Backend (.NET 10)

### Domain (`HubEsportesLages.Domain`) — nova entidade
- `PalpitePlacar`
  - `Id` (Guid)
  - `EventoId` (Guid, FK → `Evento`)
  - `TorcedorId` (string, max 64 — mesmo contrato da identidade anônima)
  - `GolsCasa` (int, 0–99)
  - `GolsVisitante` (int, 0–99)
  - `CriadoEm` (DateTime, UTC)
  - `AtualizadoEm` (DateTime, UTC) — atualizado a cada upsert enquanto `Agendado`
  - **Índice único**: `(EventoId, TorcedorId)` → garante **1 palpite por torcedor por evento** (idempotência/upsert).
- Regra de domínio (gating **inverso**): escrita de palpite exige `Evento.Status == Agendado`.
  Como `Evento.AceitaInteracao` já existe para `AoVivo`, **adicionar** `Evento.AceitaPalpite => Status == Agendado`
  (propriedade/método de domínio, espelhando o padrão existente, sem alterar `AceitaInteracao`).

### Application (`HubEsportesLages.Application`)
- DTOs (camelCase):
  - `PalpiteEstadoDto`
    `{ eventoStatus, aceitaPalpite, equipeCasa, equipeVisitante, meuPalpite?: PalpiteDto, agregado: PalpiteAgregadoDto }`
  - `PalpiteDto` `{ golsCasa, golsVisitante, atualizadoEm }`
  - `PalpiteAgregadoDto`
    `{ totalPalpites, percentualCasa, percentualEmpate, percentualVisitante,
       placares: [ { golsCasa, golsVisitante, votos, percentual } ] }`  // top N + "outros"
  - Comando: `RegistrarPalpiteDto { golsCasa, golsVisitante }` (validação 0–99 via DataAnnotations/`Range`).
- Interface: `IPalpiteService`
  - `Task<PalpiteEstadoDto> ObterEstadoAsync(string slug, string torcedorId, CancellationToken ct)`
  - `Task<PalpiteEstadoDto> RegistrarOuAtualizarAsync(string slug, string torcedorId, RegistrarPalpiteDto cmd, CancellationToken ct)`
    — faz **upsert**; lança/retorna **conflito** (mapeado a 409) se `!evento.AceitaPalpite`.
  - (Reusa `ITorcedorContexto` da spec base — **não** redeclarar.)
- Mapeamentos em `MapeamentoExtensions` (`PalpitePlacar` → `PalpiteDto`; agregação → `PalpiteAgregadoDto`).

### Infrastructure (`HubEsportesLages.Infrastructure`)
- `HubDbContext`: novo `DbSet<PalpitePlacar> Palpites` + Fluent API:
  - `HasIndex(p => new { p.EventoId, p.TorcedorId }).IsUnique()` (idempotência/upsert).
  - `Property(p => p.TorcedorId).HasMaxLength(64)`; `Range`/check 0–99 em `GolsCasa`/`GolsVisitante` (validação na app + check opcional).
- `PalpiteService : IPalpiteService`:
  - **Upsert**: busca por `(EventoId, TorcedorId)`; se existe e `AceitaPalpite`, atualiza gols + `AtualizadoEm`;
    senão insere. Tratar violação do índice único (corrida) como "atualizar o existente".
  - **Agregação**: `GROUP BY (GolsCasa, GolsVisitante)` para a distribuição; `totalPalpites = COUNT`;
    `percentualCasa/Empate/Visitante` derivados de `sign(GolsCasa - GolsVisitante)`. Limitar `placares` ao **top N**
    (ex.: 6) e somar o resto em "outros" no DTO. Percentuais arredondados, somando ~100%.
  - Após cada escrita bem-sucedida, **publica** `PalpitesAtualizados(PalpiteAgregadoDto)` no grupo `evento-{id}`
    via `IHubContext<TorcidaHub>` (mesmo hub e mesmo padrão "REST e hub usam o mesmo serviço" da spec base).
  - Registrar no `DependencyInjection` (`AddScoped<IPalpiteService, PalpiteService>()`).
- `DataSeeder`: para o evento **Agendado** já existente, semear alguns palpites de exemplo (variados) para
  o agregado não nascer vazio na demo.

### Web (`HubEsportesLages.Web`)
- **API REST** (`Controllers/Api/PalpitesApiController.cs`), com `slug` do evento; identidade pelo
  middleware `X-Torcedor-Id` já existente:
  - `GET /api/eventos/{slug}/palpites` → `PalpiteEstadoDto` (**liberado em qualquer status**; AoVivo/Encerrado = leitura).
  - `PUT /api/eventos/{slug}/palpites/meu` `{ golsCasa, golsVisitante }` → **200** (estado atualizado) /
    **409** se `!AceitaPalpite` (jogo já começou) / **422** se fora de 0–99. `PUT` por ser **upsert idempotente**.
  - (Opcional) `DELETE /api/eventos/{slug}/palpites/meu` → 204, apaga meu palpite enquanto `Agendado` (409 fora da janela).
- **Gating** (inverso): o serviço/endpoint de escrita valida `Evento.AceitaPalpite` (`Status == Agendado`);
  fora disso retorna **409**. Leitura (`GET`) é sempre permitida.
- **SignalR**: **reusa** `TorcidaHub` (`/hubs/torcida`, grupo `evento-{id}`) — **não** criar hub novo.
  - Server→client: `PalpitesAtualizados(PalpiteAgregadoDto)`. O cliente entra no grupo `evento-{id}` ao abrir
    a tela; relevante enquanto o evento está `Agendado` (depois o agregado é estático).
- **Identidade**: middleware `X-Torcedor-Id` → `ITorcedorContexto` já existente (spec base). Sem nada novo.

## API (contrato camelCase) — resumo
| Método | Rota | Body | Resposta |
|---|---|---|---|
| GET | `/api/eventos/{slug}/palpites` | — | `PalpiteEstadoDto` (sempre disponível) |
| PUT | `/api/eventos/{slug}/palpites/meu` | `{ golsCasa, golsVisitante }` | `PalpiteEstadoDto` 200 / 409 (não `Agendado`) / 422 (fora 0–99) |
| DELETE | `/api/eventos/{slug}/palpites/meu` | — | 204 / 409 (não `Agendado`) |

Push tempo real (via `TorcidaHub`, grupo `evento-{id}`): `PalpitesAtualizados(PalpiteAgregadoDto)`.

## Mobile (Arena Lages, MAUI)
- **Card de palpite** dentro/junto da tela TORCIDA (`InteractionPage`) ou da `EventDetailPage`, reaproveitando
  o layout do card do protótipo ("Qual vai ser o placar de hoje?") com os **rótulos dos times do evento**:
  `EquipeCasa  [ golsCasa ] x [ golsVisitante ]  EquipeVisitante`.
- **Estados por status do evento** (espelha o padrão de botão da spec base, mas com gating invertido):
  - `Agendado` → **editável**: dois steppers/`Entry` numéricos (0–99) + CTA "Palpitar placar" (`ButtonAccent`).
  - `AoVivo`/`Encerrado` → **leitura**: mostra meu palpite (se houver) + agregado; CTA vira "Ver palpites da torcida".
  - `Adiado`/`Cancelado` → card oculto ou desabilitado.
- `PalpitePlacarViewModel` (CommunityToolkit.Mvvm):
  - Carrega `GET .../palpites` (estados loading/erro/vazio); propriedades `GolsCasa`, `GolsVisitante`,
    `MeuPalpite`, `Agregado`, `PodeEditar` (= `aceitaPalpite`).
  - `Command RegistrarPalpite` → `PUT .../palpites/meu`; UI **otimista** + confirma via REST; em **409**
    re-sincroniza (jogo começou → trava em leitura) com mensagem.
  - **Tempo real**: reusa o cliente `Microsoft.AspNetCore.SignalR.Client` da spec base; handler para
    `PalpitesAtualizados` atualiza o agregado; `WithAutomaticReconnect`; conecta ao abrir, desconecta no `OnDisappearing`.
  - **Degradação graciosa**: sem SignalR, `PeriodicTimer` (ex.: 5s) refaz o `GET` enquanto a tela está aberta e `Agendado`.
- **Visualização do agregado** (dark-only): barra de comparação dos times (`percentualCasa` / empate /
  `percentualVisitante`) + lista dos placares mais cravados com `percentual`.
- **Tokens (dark-only, de `docs/design-arena-lages.md` §2)**: superfícies `Background #1F1633` / `Card #150F23`;
  texto `Foreground #FFFFFF` / `MutedForeground #BDB8C0`; ênfase do bloco de palpite em **violeta**
  (`accentVioletDeep #422082`, `accentVioletMid #79628C`) como na tela TORCIDA; `Accent #C2EF4E` no CTA primário;
  inputs `Input #3F3849` (fundo do campo branco, texto escuro). Status do card seguem `StatusAgendado #79628C` etc.

## Decisões, trade-offs e riscos
- **Gating inverso explícito**: usar uma propriedade de domínio dedicada `Evento.AceitaPalpite => Status == Agendado`
  (não reaproveitar `AceitaInteracao`, que é o contrário) — evita confusão e mantém uma única fonte da regra.
- **`PUT` upsert idempotente** em vez de `POST`: 1 palpite por torcedor por evento; reenvio atualiza. A idempotência
  real é garantida pelo **índice único** `(EventoId, TorcedorId)`, não pela UI.
- **Mesmo `TorcidaHub`**: evita um segundo hub/conexão no app; um único canal por evento serve interação ao vivo
  e palpite. Custo: o app filtra o evento por `PalpitesAtualizados` vs eventos da interação ao vivo.
- **Agregação top N + "outros"**: limita payload e ruído visual; o detalhe completo não é necessário no card.
- **Janela de palpite curta**: quem não palpita antes do apito perde a janela — isso é intencional ("cravar antes").
  Risco de borda: corrida entre o palpite e a virada de status; mitigado pela revalidação de `AceitaPalpite` no commit.
- **Acerto/pontuação fora de escopo agora**: a entidade já guarda gols + tem `EventoId`, então pontuar depois
  (comparar com o placar final no `Encerrado`) é aditivo, sem migração disruptiva.
- **Anti-abuso**: payload mínimo (dois ints validados 0–99); idempotência por índice; rate limit por torcedor
  reaproveitado da spec base. Sem texto livre, a superfície de abuso é baixa.
