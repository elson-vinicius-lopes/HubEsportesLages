# Design — Disputa entre Torcidas (Cabo de Guerra)

> O "como". Camadas e contratos concretos. Segue a Clean Architecture do `AGENTS.md`.
> **Estende** a base `docs/specs/interacao-torcida-ao-vivo/design.md`: reusa `ITorcedorContexto`,
> o hub único `TorcidaHub` (`/hubs/torcida`, grupos `evento-{id}`) e o gating `Evento.AceitaInteracao`.
> Não reintroduz infra já especificada lá — só descreve o que é **novo** para o cabo de guerra.

## Visão geral
Nova **entidade de voto** (`ApoioTorcida`) + um **novo card** na `InteractionPage`. Cada torcedor
declara apoio a **um dos dois lados** do confronto (casa/visitante), 1 vez por evento. O backend
agrega os apoios por lado, calcula o `%` e publica em tempo real no mesmo grupo do hub. Tudo **gated**
por `Status == AoVivo` **e** por `Evento.EhConfronto`.

```
[App MAUI: card "Disputa entre Torcidas"]
        │  GET  /api/eventos/{slug}/torcida/disputa        (placar agregado)
        │  POST /api/eventos/{slug}/torcida/disputa/apoio  { lado }   (gating AoVivo + confronto)
        └─ SignalR /hubs/torcida (grupo evento-{id}) ──> push: DisputaAtualizada(DisputaDto)
```

## Backend (.NET 10)

### Domain (`HubEsportesLages.Domain`) — nova entidade
- **`LadoTorcida`** (novo enum): `Casa = 0`, `Visitante = 1`. Mapeia para `Evento.EquipeCasaId` /
  `Evento.EquipeVisitanteId` — evita guardar `EquipeId` solto e funciona mesmo sem escudo/sigla.
- **`ApoioTorcida`** (nova entidade):
  - `Id` (int)
  - `EventoId` (int) + `Evento? Evento`
  - `Lado` (`LadoTorcida`) — qual torcida o torcedor apoia
  - `TorcedorId` (string, max 64) — identidade anônima (`X-Torcedor-Id`), igual à base
  - `CriadoEm` (DateTime)
  - **Índice único**: `(EventoId, TorcedorId)` → garante **1 apoio por torcedor por evento** (independente do lado).
- Reusa a regra de domínio existente `Evento.AceitaInteracao` (`Status == AoVivo`) e `Evento.EhConfronto`.
  Opcional: helper `Evento.AceitaDisputa => AceitaInteracao && EhConfronto` (conveniência de leitura no serviço/controller).

### Application (`HubEsportesLages.Application`)
- DTOs (camelCase):
  - `DisputaDto` { `eventoStatus`, `ehConfronto`, `casa`: `LadoTorcidaDto`, `visitante`: `LadoTorcidaDto`,
    `totalApoios`, `meuLado?` (`"casa"|"visitante"|null`) }
  - `LadoTorcidaDto` { `equipeId?`, `nome`, `sigla`, `escudo`, `corPrimaria`, `apoios`, `percentual` }
    (preenchido a partir de `Evento.EquipeCasa` / `EquipeVisitante`; `percentual` 0–100, 1 casa decimal).
  - Comando: `DeclararApoioDto` { `lado`: `"casa"|"visitante"` }.
- Interface: **`IDisputaTorcidasService`**
  - `Task<DisputaDto> ObterAsync(string slug, CancellationToken ct)` — leitura, qualquer status.
  - `Task<DisputaDto> DeclararApoioAsync(string slug, LadoTorcida lado, CancellationToken ct)` — escrita gated.
  - (Resolve o `TorcedorId` via `ITorcedorContexto`, igual aos serviços da base.)
- Mapeamentos em `MapeamentoExtensions` (Equipe → `LadoTorcidaDto`; cálculo de `percentual`).
- **Regra de %**: `pctCasa = total == 0 ? 50 : round(apoiosCasa / total * 100, 1)`; `pctVisitante = 100 - pctCasa`
  (calcular um e derivar o outro evita 99,9/100,1 por arredondamento).

### Infrastructure (`HubEsportesLages.Infrastructure`)
- `HubDbContext`: novo `DbSet<ApoioTorcida> ApoiosTorcida` + Fluent API:
  - `HasIndex(a => new { a.EventoId, a.TorcedorId }).IsUnique()` — idempotência de 1 apoio/evento.
  - `Property(a => a.TorcedorId).HasMaxLength(64)`; `Property(a => a.Lado).HasConversion<int>()`.
  - FK `EventoId` com `DeleteBehavior.Cascade`.
- **`DisputaTorcidasService : IDisputaTorcidasService`**:
  - `ObterAsync`: carrega o evento por `slug` com `Include(EquipeCasa)`/`Include(EquipeVisitante)` (`AsNoTracking`);
    `404` se não existe. Conta apoios por lado com `GROUP BY Lado` (uma query agregada). Marca `meuLado` pelo
    `TorcedorId` atual. Para evento **não-confronto** retorna `DisputaDto` com `ehConfronto=false` e contagens zeradas.
  - `DeclararApoioAsync`: **gating** — se `!evento.AceitaInteracao` → `409` (regra de negócio "evento não está ao vivo");
    se `!evento.EhConfronto` → `422` (regra "evento não é confronto"). Insere `ApoioTorcida`; trata violação do índice
    único como **idempotência** (já apoiou → recarrega e retorna o estado atual, **não** troca o lado). Recalcula o
    agregado e **publica `DisputaAtualizada`** no grupo `evento-{id}` via `IHubContext<TorcidaHub>` (mesmo padrão da base).
  - Anti-abuso: voto único por índice (principal); rate limit por torcedor herdado do middleware/serviço da base.
  - Registrar no `DependencyInjection` (`AddScoped<IDisputaTorcidasService, DisputaTorcidasService>()`).
- `DataSeeder`: nada novo obrigatório — o evento **AoVivo** semeado pela base já é um **confronto** (tem casa/visitante).
  Opcional: semear alguns `ApoioTorcida` de exemplo para o cabo de guerra não nascer 50/50.

### Web (`HubEsportesLages.Web`)
- **API REST** — adicionar ao `Controllers/Api/TorcidaApiController.cs` da base (ou um `DisputaApiController`):
  - `GET  /api/eventos/{slug}/torcida/disputa` → `DisputaDto`. **Liberado em qualquer status** (Encerrado = leitura do final).
  - `POST /api/eventos/{slug}/torcida/disputa/apoio` { `lado` } → `DisputaDto` atualizado.
    **Gating**: `409` se não `AoVivo`; `422` se não confronto; `200` no sucesso; idempotente (re-apoio devolve estado atual).
- **SignalR**: **reusa o `TorcidaHub`** já existente (`/hubs/torcida`). Novo evento server→client
  **`DisputaAtualizada(DisputaDto)`** publicado pelo `DisputaTorcidasService` no grupo `evento-{id}` após cada apoio.
  O cliente já entra/sai do grupo do evento na base — não precisa de novo hub nem novo grupo.
- **Identidade**: reusa o middleware `X-Torcedor-Id` → `ITorcedorContexto` da base.
- Swagger: anotar os 2 endpoints (200/409/422) e o schema `DisputaDto`.

## API (contrato camelCase) — resumo
| Método | Rota | Body | Resposta |
|---|---|---|---|
| GET | `/api/eventos/{slug}/torcida/disputa` | — | `DisputaDto` (qualquer status; leitura) |
| POST | `/api/eventos/{slug}/torcida/disputa/apoio` | `{ "lado": "casa" \| "visitante" }` | `200` `DisputaDto` · `409` fora de AoVivo · `422` não-confronto |

`DisputaDto` (exemplo):
```json
{
  "eventoStatus": 1,
  "ehConfronto": true,
  "totalApoios": 184,
  "meuLado": "casa",
  "casa":      { "equipeId": 3, "nome": "Time A", "sigla": "TMA", "escudo": "🦁", "corPrimaria": "#C2EF4E", "apoios": 114, "percentual": 62.0 },
  "visitante": { "equipeId": 7, "nome": "Time B", "sigla": "TMB", "escudo": "🐺", "corPrimaria": "#FA7FAA", "apoios": 70,  "percentual": 38.0 }
}
```
Push em tempo real: `DisputaAtualizada` carrega o mesmo `DisputaDto` (sem `meuLado`, que é por torcedor — o cliente preserva o seu).

## Mobile (Arena Lages, MAUI)
- **Novo card "Disputa entre Torcidas"** na `InteractionPage` (abaixo do card MVP, antes da enquete):
  só renderiza se `Disputa.EhConfronto`. Estilo dark-only, tokens de `docs/design-arena-lages.md` §2:
  - Barra "cabo de guerra": dois segmentos horizontais com largura proporcional ao `percentual`,
    `casa` em **`Accent`** (`#C2EF4E` lima) e `visitante` em **`Destructive`** (`#FA7FAA` rosa) — um acento por lado,
    coerente com "favoritar = rosa". Marcador central (corda/nó) desloca conforme a diferença.
  - Cabeçalho do card: `{escudo} {sigla casa}  {pctCasa}% × {pctVisitante}%  {sigla visitante} {escudo}`.
  - Dois botões "Apoiar {Time}" (estilos `ButtonAccent` / `ButtonGhost` do `Styles.xaml`); após apoiar,
    o lado escolhido fica **selecionado/realçado** e ambos os botões travam (espelha o `voted` da base).
  - Estados por status (alinhado à base): `AoVivo` → botões ativos; `Agendado` → card desabilitado +
    legenda "Disponível no início do jogo" (`StatusAgendado` `#79628C`); `Encerrado` → modo leitura
    (sem botões; mostra resultado final). Fundo do card `Card` `#150F23`, borda `Border` `#362D59`,
    texto secundário `MutedForeground`.
  - Botão **"Compartilhar"**: usa `Share.RequestAsync` (MAUI Essentials) com o texto
    `"{nome casa} {pctCasa}% × {pctVisitante}% {nome visitante} — {título do evento} • Arena Lages"`.
- **`DisputaViewModel`** (`CommunityToolkit.Mvvm`) — pode ser sub-VM da `InteractionViewModel`:
  - Propriedades observáveis: `Casa`, `Visitante` (com `Nome/Sigla/Escudo/Percentual`), `TotalApoios`, `MeuLado`,
    `PodeApoiar` (`= eventoStatus == AoVivo && MeuLado == null`), `EhConfronto`, `EstaCarregando`.
  - Comandos: `CarregarCommand` (`GET .../disputa`), `ApoiarCasaCommand` / `ApoiarVisitanteCommand`
    (`POST .../apoio` com UI **otimista** + confirma via REST; trava após sucesso), `CompartilharCommand`.
  - **Tempo real**: assina `DisputaAtualizada` na conexão SignalR já aberta pela `InteractionViewModel`
    (não abre conexão nova); atualiza percentuais/`TotalApoios` ao receber. `WithAutomaticReconnect` herdado.
  - **Degradação graciosa**: sem SignalR, reusa o `PeriodicTimer` de refresh da base (re-`GET .../disputa`).
- Animação: a largura dos segmentos anima suavemente (ex.: `WidthRequest` via `Animation`/`TranslateTo`) para o efeito de "puxão" do cabo de guerra.

## Decisões, trade-offs e riscos
- **Não duplicar infra**: reusa hub, grupo, identidade e gating da base. Acréscimo é 1 enum + 1 entidade + 1 serviço + 1 evento de hub + 1 card.
- **`Lado` (enum) vs `EquipeId`**: guardamos o **lado** (Casa/Visitante), não a equipe — robusto a eventos sem escudo/sigla
  e à eventual troca de equipe antes do `AoVivo`. O DTO resolve nome/escudo a partir do evento na leitura.
- **Apoio definitivo (sem trocar de lado)**: 1 voto por evento, idempotente por índice único. Permitir troca abriria
  flip-flop e brigas de % — fica **fora de escopo**; se for desejado depois, vira `UPDATE` do `Lado` no mesmo registro.
- **Gating duplo específico desta feature**: além de `AoVivo` (`409`), exige `EhConfronto` (`422`). Eventos não-confronto
  **não expõem** o card e recusam a escrita — evita cabo de guerra sem dois lados.
- **Empate inicial 50/50**: `total == 0` → 50/50 para a barra nascer centrada; sem divisão por zero.
- **Idempotência no banco, não na UI**: violação do índice único = "já apoiou" (retorna estado atual), não erro 500.
- **Compartilhamento client-side**: texto montado no app (sem render server-side) — simples e offline-friendly; imagem rica fica para depois.
- **Escala**: backplane in-memory do SignalR basta para 1 instância (hackathon); Redis backplane se escalar (mesma decisão da base).
