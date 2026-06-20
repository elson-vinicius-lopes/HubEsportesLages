# Design — Interação da Torcida Ao Vivo

> O "como". Camadas e contratos concretos. Segue a Clean Architecture do `AGENTS.md`.

## Visão geral
Feature full-stack: **backend** ganha entidades, serviços, endpoints REST e um **hub SignalR** para
tempo real; o **app MAUI** ganha o botão de entrada (com estados por status do evento) e a tela de
interação consumindo REST + SignalR. Tudo **gated** pelo status `AoVivo` do evento.

```
[App MAUI: botão "Interagir"] --(status AoVivo?)--> [InteractionPage]
        │  GET  /api/eventos/{slug}/torcida          (estado agregado)
        │  POST .../torcida/mvp | /enquete/voto | /mensagens
        └─ SignalR /hubs/torcida  (grupo evento-{id}) ──> push: MvpAtualizado, EnqueteAtualizada, NovaMensagem
```

## Backend (.NET 10)

### Domain (`HubEsportesLages.Domain`) — novas entidades
- `JogadorEvento` { Id, EventoId, EquipeId?, Nome } — candidatos a MVP (escalação do jogo).
- `VotoMvp` { Id, EventoId, JogadorEventoId, TorcedorId, CriadoEm } — **único por (EventoId, TorcedorId)**.
- `Enquete` { Id, EventoId, Pergunta, Ativa } e `OpcaoEnquete` { Id, EnqueteId, Texto }.
- `VotoEnquete` { Id, OpcaoEnqueteId, EnqueteId, TorcedorId, CriadoEm } — **único por (EnqueteId, TorcedorId)**.
- `MensagemTorcida` { Id, EventoId, TorcedorId, Autor, Texto, CriadoEm, Removida }.
- `EquipeFavorita` { Id, TorcedorId, EquipeId, CriadoEm } — **único por (TorcedorId, EquipeId)**.

Regra de domínio: escritas de interação exigem `Evento.Status == AoVivo` (método `Evento.AceitaInteracao`).

### Application (`HubEsportesLages.Application`)
- DTOs (camelCase):
  - `TorcidaEstadoDto` { eventoStatus, mvp: { candidatos:[{jogadorEventoId,nome,votos}], meuVotoJogadorId? },
    enquete?: { id, pergunta, opcoes:[{id,texto,percentual,votos}], minhaOpcaoId? }, mensagens:[MensagemDto], favoritado }
  - `MensagemDto` { id, autor, texto, criadoEm }
  - Comandos: `VotarMvpDto { jogadorEventoId }`, `VotarEnqueteDto { opcaoId }`, `EnviarMensagemDto { texto }`.
- Interfaces: `ITorcidaService` (estado agregado, votar MVP, votar enquete, enviar mensagem, favoritar),
  `IModeracaoService` (remover mensagem), `ITorcedorContexto` (resolve o `TorcedorId` da requisição).
- Mapeamentos em `MapeamentoExtensions`.

### Infrastructure (`HubEsportesLages.Infrastructure`)
- `HubDbContext`: novos `DbSet`s + Fluent API com **índices únicos** que garantem 1 voto
  (`(EventoId,TorcedorId)` em `VotoMvp`; `(EnqueteId,TorcedorId)` em `VotoEnquete`; `(TorcedorId,EquipeId)` em favoritos).
- `TorcidaService`: votos idempotentes (tratar violação de índice único como "já votou"), tally agregado por
  `GROUP BY`, mensagens com limite/rate-limit, favoritos toggle. Registrar no `DependencyInjection`.
- `DataSeeder`: para o evento **AoVivo** já existente, semear 1 enquete + escalação (jogadores) de exemplo.

### Web (`HubEsportesLages.Web`)
- **API REST** (`Controllers/Api/TorcidaApiController.cs`), todas com `slug` do evento:
  - `GET  /api/eventos/{slug}/torcida` → `TorcidaEstadoDto` (permitido em qualquer status; Encerrado = leitura).
  - `POST /api/eventos/{slug}/torcida/mvp` { jogadorEventoId } → 200/409 (gating AoVivo).
  - `POST /api/eventos/{slug}/torcida/enquete/{enqueteId}/voto` { opcaoId } → 200/409.
  - `GET  /api/eventos/{slug}/torcida/mensagens?desde=...` e `POST .../mensagens` { texto } → 201/409/429.
  - `POST` e `DELETE /api/favoritos/equipes/{equipeId}`.
  - **Admin**: `POST /api/eventos/{id}/torcida/enquete`, `POST .../jogadores`, `DELETE .../mensagens/{id}`.
- **SignalR** (`Hubs/TorcidaHub.cs`, rota `/hubs/torcida`): in-box no ASP.NET Core (`AddSignalR` + `MapHub`).
  - Cliente entra no grupo `evento-{id}` ao abrir a tela de um evento **AoVivo**; sai ao fechar.
  - Server→client: `MvpAtualizado(candidatosComVotos)`, `EnqueteAtualizada(percentuais)`,
    `NovaMensagem(MensagemDto)`, `MensagemRemovida(id)`. O `TorcidaService` publica via `IHubContext<TorcidaHub>`
    após cada escrita (REST e hub usam o mesmo serviço → uma fonte de verdade).
- **Gating** central: filtro/serviço valida `Evento.AceitaInteracao` nas escritas; leitura liberada.
- **Identidade do torcedor** (fallback): middleware lê o header `X-Torcedor-Id` (GUID do app) e popula
  `ITorcedorContexto`. Trocar por auth real quando a spec de usuário existir.

## API (contrato camelCase) — resumo
| Método | Rota | Body | Resposta |
|---|---|---|---|
| GET | `/api/eventos/{slug}/torcida` | — | `TorcidaEstadoDto` |
| POST | `/api/eventos/{slug}/torcida/mvp` | `{ jogadorEventoId }` | estado MVP atualizado / 409 |
| POST | `/api/eventos/{slug}/torcida/enquete/{id}/voto` | `{ opcaoId }` | percentuais / 409 |
| GET/POST | `/api/eventos/{slug}/torcida/mensagens` | `{ texto }` | lista / msg criada / 429 |
| POST·DELETE | `/api/favoritos/equipes/{equipeId}` | — | 204 |

## Mobile (Arena Lages, MAUI)
- **Botão de entrada** (componente reutilizável, estilo `ButtonAccent`/`ButtonGhost` de `Styles.xaml`):
  - `AoVivo` → habilitado, "🔴 Interagir com a Torcida" (em EventDetailPage e na CheckInPage pós check-in).
  - `Agendado` → desabilitado + legenda "Disponível no início do jogo".
  - `Encerrado` → "Ver resultados" (abre a tela em modo leitura: MVP vencedor, % final, mural arquivado).
- `InteractionPage` + `InteractionViewModel`:
  - Carrega `GET .../torcida` (estados loading/erro/vazio).
  - **SignalR client** (`Microsoft.AspNetCore.SignalR.Client`): conecta ao grupo do evento; handlers atualizam
    tally/poll/mensagens; `WithAutomaticReconnect`; desconecta em `OnDisappearing`.
  - Comandos `VotarMvp`, `VotarEnquete`, `EnviarMensagem`, `ToggleFavorito` — UI **otimista** + confirma via REST;
    trava o voto após registrado (espelha o protótipo `voted`/`poll`).
  - **Degradação graciosa** sem SignalR: `PeriodicTimer` de refresh (ex.: 5s) enquanto a tela está aberta.
- Visual: dark-only; `accentLime` no MVP, `accentVioletMid` na enquete/mural (conforme `App.tsx` e `docs/design-arena-lages.md`).

## Decisões, trade-offs e riscos
- **Tempo real:** SignalR (in-box, WebSockets) para "durante o jogo"; fallback polling cobre redes ruins.
  Backplane in-memory basta para 1 instância (hackathon); para escalar, Redis backplane depois.
- **Idempotência:** garantida por índice único no banco (não confiar só na UI).
- **Anti-abuso:** tamanho ≤ 140, rate limit por torcedor, filtro de palavrão simples + moderação manual.
- **Identidade anônima por dispositivo** primeiro; evoluir para login (lacuna #3) sem quebrar o contrato.
- **Janela "ao vivo":** usar `Status == AoVivo`. Quem vira o status para AoVivo/Encerrado? Decisão: organizador
  via Admin (ou job futuro). Enquanto isso, leitura sempre disponível.
