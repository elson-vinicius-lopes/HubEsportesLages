# Design — Esquenta da Torcida (Pontos de Encontro)

> O "como". Camadas e contratos concretos. Segue a Clean Architecture do `AGENTS.md`.
> **Estende** `docs/specs/interacao-torcida-ao-vivo/design.md`: reutiliza `ITorcedorContexto`, `TorcidaHub`,
> `ResultadoInteracao<T>`/`StatusInteracao` e o padrão de gating. Difere no **status da janela**: aqui o
> gating das escritas é **`Agendado`** (pré-jogo), não `AoVivo`.

## Visão geral
Feature full-stack: o **backend** ganha duas entidades (`PontoEncontro`, `PresencaPontoEncontro`), DTOs,
serviço, endpoints REST (torcedor + admin) e — na Fase 2 — push pelo `TorcidaHub`. O **app MAUI** ganha a
seção "Esquenta da torcida" no detalhe do evento `Agendado`, com cards de ponto (rota + confirmar presença).

```
[App MAUI: EventDetailPage (Agendado)] --> seção "Esquenta da Torcida"
        │  GET  /api/eventos/{slug}/esquenta                 (lista de pontos + minha presença)
        │  POST /api/eventos/{slug}/esquenta/{pontoId}/presenca     (confirmar) → gating Agendado (409 fora)
        │  DEL  /api/eventos/{slug}/esquenta/{pontoId}/presenca     (cancelar)
        └─ SignalR /hubs/torcida (grupo evento-{id}) ──> push: PresencaAtualizada(pontoId, total)   [Fase 2]
   [Admin] POST/PUT/DELETE /api/eventos/{eventoId}/esquenta/pontos...
```

## Backend (.NET 10)

### Domain (`HubEsportesLages.Domain`) — novas entidades

`PontoEncontro` (ponto curado pelo organizador):
- `Id` (int)
- `EventoId` (int) + `Evento? Evento` — FK, `OnDelete: Cascade`
- `Nome` (string, max 120, req) — ex.: "Bar do Centro"
- `Endereco` (string, max 200)
- `Bairro` (string, max 80, default "Centro")
- `Cidade` (string, max 80, default "Lages"), `Uf` (string, max 2, default "SC")
- `Horario` (`TimeOnly`) — horário do esquenta (ex.: 17:00)
- `Descricao` (string, max 400)
- `Regras` (string?, max 400) — ex.: "Camisa do time; respeito à galera adversária"
- `Latitude` (double?), `Longitude` (double?)
- `Ordem` (int, default 0) — desempate de exibição definido pelo organizador
- `CriadoEm` (DateTime, default `DateTime.Now`)
- `ICollection<PresencaPontoEncontro> Presencas`
- **Propriedade calculada** `MapaUrl` — **mesma regra de `Local.MapaUrl`** (coordenadas →
  `https://www.google.com/maps/search/?api=1&query={lat},{long}`, `InvariantCulture`; fallback por
  `"{Nome}, {Cidade} - {Uf}"`). Marcar `Ignore` no EF.

`PresencaPontoEncontro` (confirmação de presença de um torcedor):
- `Id` (int)
- `PontoEncontroId` (int) + `PontoEncontro? PontoEncontro` — FK, `OnDelete: Cascade`
- `EventoId` (int) — desnormalizado para validar a janela/gating sem join (FK `Restrict` p/ evitar cascade
  duplo no SQLite)
- `TorcedorId` (string, max 64, req) — cabeçalho `X-Torcedor-Id`
- `CriadoEm` (DateTime, default `DateTime.Now`)

**Índice único:** `(PontoEncontroId, TorcedorId)` em `PresencaPontoEncontro` → **1 presença por torcedor por
ponto** (idempotência garantida no banco, igual ao padrão `VotoMvp`/`VotoEnquete`).

**Regra de domínio (janela):** as escritas exigem o evento em **pré-jogo**. Adicionar ao `Evento` a
propriedade calculada irmã de `AceitaInteracao`:
```csharp
/// <summary>Indica se o evento aceita confirmação de presença no esquenta — apenas no pré-jogo (agendado).</summary>
public bool AceitaEsquenta => Status == StatusEvento.Agendado;
```
(Mantém a mesma forma de `AceitaInteracao => Status == AoVivo`; ambas marcadas `Ignore` no EF.)

### Application (`HubEsportesLages.Application`)

DTOs (camelCase no JSON):
- `EsquentaEstadoDto { eventoStatus: StatusEvento, aceitaEsquenta: bool, pontos: PontoEncontroDto[] }`
- `PontoEncontroDto { id, nome, endereco, bairro, horario (string "HH:mm"), descricao, regras?, mapaUrl,
   distanciaMetros?: int, distanciaTexto?: string, presencas: int, confirmadoPorMim: bool }`
- Comandos admin: `CriarPontoEncontroDto { nome, endereco, bairro?, cidade?, uf?, horario, descricao,
   regras?, latitude?, longitude?, ordem? }` (DataAnnotations: `Nome`/`Endereco` `[Required]`, `[StringLength]`),
   `AtualizarPontoEncontroDto` (mesmos campos).
- **Sem comando de body** para confirmar/cancelar presença: o torcedor vem do header, o ponto da rota.

Interfaces:
- `IEsquentaService`:
  - `Task<EsquentaEstadoDto?> ObterEstadoAsync(string slug, CancellationToken ct)` — `null` se evento não existe.
  - `Task<ResultadoInteracao<EsquentaEstadoDto>> ConfirmarPresencaAsync(string slug, int pontoId, CancellationToken ct)`
  - `Task<ResultadoInteracao<EsquentaEstadoDto>> CancelarPresencaAsync(string slug, int pontoId, CancellationToken ct)`
- `IEsquentaAdminService` (curadoria): `CriarPontoAsync(int eventoId, CriarPontoEncontroDto)`,
  `AtualizarPontoAsync(int eventoId, int pontoId, AtualizarPontoEncontroDto)`, `RemoverPontoAsync(int eventoId, int pontoId)` →
  retornam `StatusInteracao` (igual ao `IModeracaoService`).
- Reaproveita `ITorcedorContexto`, `ResultadoInteracao<T>`, `StatusInteracao` (já existem).

Mapeamentos em `MapeamentoExtensions` (`PontoEncontro` + contagem/flag → `PontoEncontroDto`; helper de
formatação de distância `DistanciaTexto(int? metros)` → "a 800 m" / "a 1,2 km", `null` se sem distância).

### Infrastructure (`HubEsportesLages.Infrastructure`)

`HubDbContext`:
- Novos `DbSet<PontoEncontro> PontosEncontro` e `DbSet<PresencaPontoEncontro> PresencasPontoEncontro`.
- Fluent API (bloco "Esquenta da torcida"):
  - `PontoEncontro`: `Nome` max 120 req; `Endereco` max 200; `Bairro`/`Cidade` max 80; `Uf` max 2;
    `Descricao` max 400; `Regras` max 400; `e.Ignore(x => x.MapaUrl)`; `HasIndex(x => x.EventoId)`;
    FK `Evento` `Cascade`.
  - `PresencaPontoEncontro`: `TorcedorId` max 64 req; **`HasIndex(x => new { x.PontoEncontroId, x.TorcedorId }).IsUnique()`**;
    FK `PontoEncontro` `Cascade`; FK `Evento` (via `EventoId`) **sem navegação ou `Restrict`** para evitar
    múltiplos caminhos de cascade no SQLite.
- `Evento`: adicionar `e.Ignore(x => x.AceitaEsquenta)`.

`EsquentaService : IEsquentaService` (espelha `TorcidaService`):
- `ObterEstadoAsync`: carrega evento por slug (`AsNoTracking`); monta `EsquentaEstadoDto` com pontos
  ordenados por `Horario`, depois `Ordem`, depois distância; `presencas` via `GROUP BY PontoEncontroId`;
  `confirmadoPorMim` consultando `PresencasPontoEncontro` pelo `TorcedorId` do contexto; `distanciaMetros`
  por **Haversine** (coords do ponto × `Evento.Local` lat/long; `null` se faltarem coords).
- `ConfirmarPresencaAsync`: valida (evento existe → senão `NaoEncontrado`; **`evento.AceitaEsquenta` → senão
  `NaoAoVivo`**; `TorcedorId` presente → senão `SemTorcedor`; ponto pertence ao evento → senão `Invalido`).
  Insere `PresencaPontoEncontro` e `SalvarIdempotenteAsync` (trata violação de índice único como "já
  confirmou" — mesmo helper/conceito do `TorcidaService`). Retorna estado atualizado.
- `CancelarPresencaAsync`: mesmo gating; remove a presença do torcedor no ponto (se existir; ausente = no-op
  idempotente). Retorna estado.
- **Reuso de `StatusInteracao.NaoAoVivo`**: o enum é semântico ("escrita fora da janela"); a mensagem HTTP do
  controller é específica do esquenta ("disponível apenas no pré-jogo"). Evita inflar o enum compartilhado.

`DataSeeder`: para um evento **`Agendado`** existente, semear 2–3 pontos de encontro de exemplo (ex.: "Bar do
Centro" 17:00 com lat/long ~800 m do estádio; "Praça da Torcida" 16:30; regras de exemplo).

`DependencyInjection`: registrar `services.AddScoped<IEsquentaService, EsquentaService>();` e
`services.AddScoped<IEsquentaAdminService, EsquentaAdminService>();`.

### Web (`HubEsportesLages.Web`)

**API REST do torcedor** (`Controllers/Api/EsquentaApiController.cs`, `[Route("api/eventos/{slug}/esquenta")]`,
`[Tags("Esquenta")]`), traduzindo `ResultadoInteracao`/`StatusInteracao` para HTTP como no `TorcidaApiController`:
- `GET  /api/eventos/{slug}/esquenta` → `EsquentaEstadoDto` (200; 404 se evento não existe). **Leitura
  liberada em qualquer status.**
- `POST `…`/{pontoId:int}/presenca` → confirma. 200 (estado) · 400 (sem `X-Torcedor-Id`) · 404 · 409
  (`NaoAoVivo` → **"O esquenta da torcida só aceita confirmações antes do jogo (evento agendado)."**) · 422.
- `DELETE `…`/{pontoId:int}/presenca` → cancela. Mesmos códigos (200 com estado; 409 fora da janela).

**API REST admin** (`Controllers/Api/EsquentaAdminApiController.cs`,
`[Route("api/eventos/{eventoId:int}/esquenta")]`, `[Tags("Esquenta (organização)")]`, espelha
`TorcidaAdminApiController`):
- `POST   `…`/pontos` `{ CriarPontoEncontroDto }` → 201/404/422.
- `PUT    `…`/pontos/{pontoId:int}` `{ AtualizarPontoEncontroDto }` → 204/404/422.
- `DELETE `…`/pontos/{pontoId:int}` → 204/404.

**Gating** (ponto crítico desta feature): as escritas do torcedor validam **`Evento.AceitaEsquenta`
(`Status == Agendado`)**, espelhando como o `TorcidaService` valida `AceitaInteracao` (`AoVivo`). Admin **não**
é gated por status (organizador cura a lista a qualquer momento). Leitura sempre liberada.

**Identidade**: reusa o `TorcedorContexto` (header `X-Torcedor-Id`) já registrado no `Program.cs` — nada novo.

**SignalR (Fase 2)**: reusar o **mesmo `TorcidaHub`** (`/hubs/torcida`, grupo `evento-{id}`). Após
confirmar/cancelar, `EsquentaService` publica `PresencaAtualizada(pontoId, total)` via
`IHubContext<TorcidaHub>` (REST e hub na mesma fonte de verdade, exatamente como na spec irmã). **Não criar
um segundo hub.** Observação: o `TorcidaHub` é entregue na Fase 2 da spec irmã; esta feature **depende** dele
para o tempo real e, até lá, opera só por REST + refresh.

## API (contrato camelCase) — resumo
| Método | Rota | Body | Resposta |
|---|---|---|---|
| GET | `/api/eventos/{slug}/esquenta` | — | `EsquentaEstadoDto` (200) / 404 |
| POST | `/api/eventos/{slug}/esquenta/{pontoId}/presenca` | — | `EsquentaEstadoDto` (200) / 400 / 404 / **409 fora do pré-jogo** / 422 |
| DELETE | `/api/eventos/{slug}/esquenta/{pontoId}/presenca` | — | `EsquentaEstadoDto` (200) / 400 / 404 / 409 |
| POST | `/api/eventos/{eventoId}/esquenta/pontos` | `CriarPontoEncontroDto` | 201 / 404 / 422 |
| PUT | `/api/eventos/{eventoId}/esquenta/pontos/{pontoId}` | `AtualizarPontoEncontroDto` | 204 / 404 / 422 |
| DELETE | `/api/eventos/{eventoId}/esquenta/pontos/{pontoId}` | — | 204 / 404 |

Exemplo `PontoEncontroDto`:
```json
{ "id": 3, "nome": "Bar do Centro", "endereco": "R. Cel. Córdova, 120", "bairro": "Centro",
  "horario": "17:00", "descricao": "Esquenta com a galera antes do clássico.",
  "regras": "Camisa do time; respeito à torcida adversária.",
  "mapaUrl": "https://www.google.com/maps/search/?api=1&query=-27.8160,-50.3260",
  "distanciaMetros": 800, "distanciaTexto": "a 800 m", "presencas": 42, "confirmadoPorMim": true }
```

## Mobile (Arena Lages, MAUI)
- **Entrada**: seção **"Esquenta da Torcida"** dentro da `EventDetailPage` (logo após o card Local / "Ver
  Rota"), visível quando `status == Agendado`. Em `AoVivo`/`Encerrado` a seção entra em **modo leitura**
  (mostra os pontos e contagens; botões "Confirmar presença" desabilitados com legenda "Encontro já
  rolou / jogo começou"). Reutiliza o gating de UI por status já usado para o botão "Interagir".
- **Card de ponto** (componente `Border` arredondado, tokens dark):
  - Título (`Nome`) + chip de horário; linha de endereço + **distância** ("a 800 m"); `Descricao`;
    bloco "Regras" quando houver; rodapé com contador "{presencas} confirmados".
  - Botão **"Ver Rota"** → `Launcher.OpenAsync(mapaUrl)` (mesmo padrão do "Ver Rota" do `Local`).
  - Botão toggle **"Confirmar presença" / "Cancelar"** → POST/DELETE; **UI otimista** (incrementa/decrementa
    local) confirmada via REST; reflete `confirmadoPorMim`.
- `EsquentaViewModel` (MVVM, `CommunityToolkit.Mvvm`):
  - `LoadAsync(slug)` → `GET …/esquenta` (estados loading/erro/vazio — "Nenhum ponto de encontro ainda").
  - `ConfirmarCommand(ponto)` / `CancelarCommand(ponto)` → POST/DELETE; envia header `X-Torcedor-Id` (GUID
    anônimo persistido no app, mesma infra da spec irmã); trata 409 (fora do pré-jogo) e 400 (sem header).
  - **Tempo real (Fase 2)**: cliente `Microsoft.AspNetCore.SignalR.Client` no `TorcidaHub`,
    `WithAutomaticReconnect`, conecta ao grupo `evento-{id}` no `OnAppearing`, desconecta no
    `OnDisappearing`; handler `PresencaAtualizada(pontoId, total)` atualiza o contador. **Fallback**: sem
    SignalR, `PeriodicTimer` de refresh (~10 s) enquanto a tela está aberta.
- **Tokens dark** (`docs/design-arena-lages.md` §2): superfície do card `Card #150F23` sobre
  `Background #1F1633`, texto `Foreground #FFFFFF`, atenuado `MutedForeground #BDB8C0`, borda
  `Border #362D59`; ação primária "Confirmar presença" em `Accent #C2EF4E` (`AccentForeground #1F1633`);
  chip de horário com `Muted #362D59`. Botão "Ver Rota" no estilo ghost/secundário, como na `EventDetailPage`.

## Decisões, trade-offs e riscos
- **Janela = `Agendado` (pré-jogo)**, o oposto da spec irmã (`AoVivo`). Decisão central: o esquenta é
  encontro **antes** do jogo. Reusamos a forma de `AceitaInteracao` numa nova propriedade `AceitaEsquenta`
  para deixar o gating explícito e testável, em vez de espalhar `Status ==` pelo serviço.
- **Reuso vs. novo enum**: mantemos `StatusInteracao.NaoAoVivo` para "fora da janela" (não criamos
  `ForaDoPreJogo`) para não inflar o contrato compartilhado; a **mensagem** HTTP é específica do esquenta.
  Trade-off: o nome do enum fica levemente impreciso; documentado aqui e no controller.
- **Idempotência** por índice único `(PontoEncontroId, TorcedorId)` + `SalvarIdempotenteAsync` (mesmo padrão
  dos votos) — não confiar só na UI; corrida de duplo clique vira no-op.
- **Distância** calculada no servidor (Haversine, ponto → `Local` do evento) e enviada pronta
  (`distanciaMetros` + `distanciaTexto`). Risco: `Local` sem coordenadas → distância `null` (UI omite). Não
  usamos GPS do dispositivo (privacidade + simplicidade no hackathon).
- **Curadoria (sem UGC)**: lista cadastrada pelo organizador. Evita moderação pesada de ponto falso/spam;
  reavaliar se a comunidade pedir pontos criados por torcedores (reusaria `IModeracaoService`).
- **Tempo real opcional**: o contador de presenças tolera atraso; SignalR (no `TorcidaHub` existente) é um
  plus de Fase 2. Sem ele, REST + refresh já atende os critérios. Backplane in-memory basta p/ 1 instância.
- **Cascade no SQLite**: `PresencaPontoEncontro` tem FKs para `PontoEncontro` (Cascade) e `Evento`. Manter
  apenas **um** caminho de cascade (via `PontoEncontro`); a FK de `Evento` fica sem navegação/`Restrict` para
  evitar o erro de múltiplos caminhos de cascade do SQLite.
