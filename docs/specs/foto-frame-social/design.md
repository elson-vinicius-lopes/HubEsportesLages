# Design — Foto com Frame para Redes Sociais

> O "como". Camadas e contratos concretos. Segue a Clean Architecture do `AGENTS.md`.
> **Estende** `docs/specs/interacao-torcida-ao-vivo/design.md` (identidade, hub, gating, padrão
> `ResultadoInteracao`/`StatusInteracao`). Reusa, não duplica.

## Visão geral
Feature **predominantemente mobile**: o app MAUI faz a composição (foto + frame + textos) e exporta
localmente. O **backend é fino**: serve o **catálogo de frames** por evento/equipe e (opcional)
recebe um **ping de métrica** de compartilhamento. A imagem final **nunca trafega pela API**.

```
[App MAUI: botão "Foto com frame"] ──> [FrameComposerPage]
   │  GET  /api/eventos/{slug}/frames                 (catálogo de frames + pré-preenchimento)
   │  (composição 100% local: SkiaSharp/GraphicsView + MediaPicker + Share/SaveFile)
   └─ POST /api/eventos/{slug}/frames/compartilhamentos  (opcional, métrica — sem imagem)
   └─ (opcional) SignalR /hubs/torcida grupo evento-{id} → PlacarAtualizado (re-preenche placar)
```

## Backend (.NET 10)

### Domain (`HubEsportesLages.Domain`) — novas entidades
- **`FrameTemplate`** `{ Id, EventoId?, EquipeId?, Nome, Slug, ImagemUrl, Ordem, Ativo, Escopo:EscopoFrame }`
  — molde de moldura. `EventoId`/`EquipeId` nulos ⇒ frame **global** (marca Arena Lages). `ImagemUrl`
  aponta para um PNG com canal alfa (área transparente onde entra a foto).
  - **Índice único**: `(Slug)` único globalmente (slug estável p/ cache no app).
  - Índices de busca: `(EventoId, Ativo, Ordem)` e `(EquipeId, Ativo, Ordem)`.
- **`EscopoFrame`** (enum): `0 Global`, `1 Evento`, `2 Equipe`.
- **`CompartilhamentoFrame`** `{ Id, EventoId, FrameTemplateId?, TorcedorId, Canal:string?, CriadoEm }`
  — métrica **opcional** (conta quantos torcedores geraram/compartilharam). **Sem índice único**
  (não é voto; contagem cumulativa). Índice de busca: `(EventoId, CriadoEm)`.

Regra de domínio (gating desta feature — **diferente da base**):
- `Evento` ganha **`PermiteFrame => Status != StatusEvento.Cancelado`** (Agendado, AoVivo, Encerrado,
  Adiado liberam; Cancelado não). A base usa `AceitaInteracao => Status == AoVivo`; aqui a janela é maior
  de propósito (frame de hype antes, "Eu tô no jogo" durante, placar final depois).

### Application (`HubEsportesLages.Application`)
DTOs (camelCase no JSON):
- `FrameTemplateDto { id, slug, nome, imagemUrl, escopo, ordem }`
- `FramePrefillDto { titulo, equipeCasa, equipeVisitante, placar?, fraseSugerida }`
  - `equipeCasa`/`equipeVisitante`: `{ nome, sigla, escudo, corPrimaria }` (reusa dados de `Equipe`).
  - `placar`: string pronta (ex.: "3 x 1") ou `null` se o evento não tem placar.
  - `fraseSugerida`: default "Eu tô no jogo".
- `FrameCatalogoDto { frames: FrameTemplateDto[], prefill: FramePrefillDto }` — resposta do GET.
- Comando: `RegistrarCompartilhamentoDto { frameTemplateId?, canal? }` — `canal` é livre (ex.: "instagram", "download").

Interfaces:
- `IFrameService`:
  - `Task<FrameCatalogoDto?> ObterCatalogoAsync(string slug, CancellationToken ct)` — null = evento não encontrado.
  - `Task<ResultadoInteracao<bool>> RegistrarCompartilhamentoAsync(string slug, RegistrarCompartilhamentoDto dto, CancellationToken ct)`
    — usa o padrão `ResultadoInteracao`/`StatusInteracao` da base (404 `NaoEncontrado`, 409 `NaoAoVivo`
    reaproveitado como "evento cancelado", 400 `SemTorcedor`).
- Mapeamentos em `MapeamentoExtensions` (entidade → DTO; montagem do `prefill` a partir do `Evento`+`Equipe`).

### Infrastructure (`HubEsportesLages.Infrastructure`)
- `HubDbContext`: novos `DbSet<FrameTemplate>` e `DbSet<CompartilhamentoFrame>` + Fluent API
  (índice único em `FrameTemplate.Slug`; índices de busca acima; sem índice único em compartilhamento).
- `FrameService` (implementa `IFrameService`):
  - `ObterCatalogoAsync`: `AsNoTracking` + `Include` do evento/equipes; resolve frames aplicáveis =
    **global** ∪ **do evento** ∪ **das equipes do confronto**, ativos, ordenados por `Ordem`; sempre
    inclui o **frame global padrão** (garante ≥ 1). Monta `prefill` (título, equipes, `Evento.Placar`,
    frase default).
  - `RegistrarCompartilhamentoAsync`: valida `Evento.PermiteFrame` (senão `NaoAoVivo`→409); exige
    `TorcedorId` (`ITorcedorContexto`, senão `SemTorcedor`→400); grava `CompartilhamentoFrame`. Idempotente-tolerante.
  - Registrar no `DependencyInjection` (`AddScoped<IFrameService, FrameService>()`).
- `DataSeeder`: semear o **frame global padrão Arena Lages** (`slug = "arena-lages-padrao"`) e, para o
  evento AoVivo de exemplo, 1 frame de evento; (opcional) 1 frame por equipe usando `CorPrimaria`/`Escudo`.

### Web (`HubEsportesLages.Web`)
- **API REST** (`Controllers/Api/FramesApiController.cs`, `[Route("api/eventos/{slug}/frames")]`,
  `[Tags("Frames")]`, espelhando `TorcidaApiController`):
  - `GET  /api/eventos/{slug}/frames` → `FrameCatalogoDto` (200) | 404. **Leitura liberada** em qualquer status.
  - `POST /api/eventos/{slug}/frames/compartilhamentos` `{ frameTemplateId?, canal? }` → 201 | 400 (sem
    `X-Torcedor-Id`) | 404 | 409 (evento `Cancelado`). Traduz `StatusInteracao` → HTTP igual à base.
  - **Admin (futuro, fora do MVP)** em `FramesAdminApiController`: `POST /api/eventos/{id}/frames`,
    `DELETE .../frames/{frameId}`, `GET .../frames/metricas`.
- **Identidade**: reusa o middleware/`TorcedorContexto` da base (header `X-Torcedor-Id`). Nada novo.
- **Gating**: leitura do catálogo **sempre liberada**; a escrita de métrica valida `Evento.PermiteFrame`
  (≠ `Cancelado`). **Não** reaproveitar `AceitaInteracao` aqui — é a diferença central desta feature.
- **SignalR (opcional)**: **não cria hub novo**. Se quiser placar ao vivo no pré-preenchimento, o app
  entra no grupo `evento-{id}` do **`TorcidaHub`** existente e escuta um evento de placar; o `FrameService`
  não publica nada (a publicação de placar, se existir, é responsabilidade de quem altera o placar).

## API (contrato camelCase) — resumo
| Método | Rota | Body | Resposta |
|---|---|---|---|
| GET | `/api/eventos/{slug}/frames` | — | `FrameCatalogoDto` (200) / 404 |
| POST | `/api/eventos/{slug}/frames/compartilhamentos` | `{ frameTemplateId?, canal? }` | 201 / 400 / 404 / 409 |
| POST (admin) | `/api/eventos/{id}/frames` | `{ nome, imagemUrl, escopo, equipeId?, ordem }` | 201 |
| DELETE (admin) | `/api/eventos/{id}/frames/{frameId}` | — | 204 |
| GET (admin) | `/api/eventos/{id}/frames/metricas` | — | `{ total, porFrame[], porCanal[] }` |

Exemplo `FrameCatalogoDto`:
```json
{
  "frames": [
    { "id": 1, "slug": "arena-lages-padrao", "nome": "Arena Lages", "imagemUrl": "/frames/padrao.png", "escopo": 0, "ordem": 0 },
    { "id": 7, "slug": "citadino-futsal-2026", "nome": "Citadino Futsal", "imagemUrl": "/frames/ev-12.png", "escopo": 1, "ordem": 1 }
  ],
  "prefill": {
    "titulo": "Lages FC x Serrano",
    "equipeCasa": { "nome": "Lages FC", "sigla": "LAG", "escudo": "️", "corPrimaria": "#1f6feb" },
    "equipeVisitante": { "nome": "Serrano", "sigla": "SER", "escudo": "", "corPrimaria": "#C2EF4E" },
    "placar": "3 x 1",
    "fraseSugerida": "Eu tô no jogo"
  }
}
```

## Mobile (Arena Lages, MAUI)
- **Botão de entrada** (componente reutilizável, estilo `ButtonAccent` lima de `Styles.xaml`):
  - No `EventDetailPage`: "Foto com frame". **Habilitado** em todo status **exceto `Cancelado`**
    (oculto/disabled). Não depende de `AoVivo` (diferente do botão "Interagir" da base).
- **`FrameComposerPage` + `FrameComposerViewModel`** (`CommunityToolkit.Mvvm`):
  - Carrega `GET .../frames` → popula lista de frames e os campos (nome do time, placar, frase) via `prefill`.
    Estados loading/erro/vazio; em erro/offline cai no **frame padrão embutido** (`Resources/Images/frame_padrao.png`).
  - **Seleção de foto**: `MediaPicker.PickPhotoAsync()` (galeria) e `MediaPicker.CapturePhotoAsync()` (câmera);
    opção "Sem foto" usa fundo do frame.
  - **Composição/preview**: canvas com **SkiaSharp** (`SKCanvasView`) — desenha foto (cover/crop), o PNG do
    frame por cima, e os textos sobrepostos (nome do time, placar, frase). Alternativa simples: `GraphicsView`
    com `IDrawable`. Preview atualiza ao vivo a cada mudança (binding nos campos + frame selecionado).
  - **Exportar**: render para `SKImage` em ≥ 1080×1080 → `SKData` (PNG/JPEG).
    - **Baixar**: salvar em arquivo (`FileSaver`/`MediaPicker`/galeria) — best-effort por plataforma.
    - **Compartilhar**: `Share.RequestAsync(new ShareFileRequest{ File = new ShareFile(path) })`.
  - **Métrica (best-effort)**: ao baixar/compartilhar, dispara `POST .../frames/compartilhamentos`
    `{ frameTemplateId, canal }` com `X-Torcedor-Id`; falha **não bloqueia** o fluxo (catch silencioso).
  - **Realtime/fallback (opcional)**: se a tela ficar aberta durante o jogo, conectar ao `TorcidaHub`
    (`WithAutomaticReconnect`, conecta no `OnAppearing`, desconecta no `OnDisappearing`) só para
    re-preencher o **placar** quando mudar; **fallback**: re-`GET .../frames` num `PeriodicTimer` (ex.: 15s).
    Sem realtime, o preview já funciona com o placar do carregamento inicial.
- **Tokens dark** (de `docs/design-arena-lages.md`): fundo `Background #1F1633`, cartões `Card #150F23`,
  CTA `Accent #C2EF4E` (lima) com `AccentForeground #1F1633`; cantos `RadiusXl 12`; tipografia/escala 8px.
  Frase default "Eu tô no jogo" como placeholder editável.

## Decisões, trade-offs e riscos
- **Composição client-side**: privacidade (a foto do torcedor nunca sai do device), latência zero, backend
  barato. Trade-off: paridade visual entre Android/iOS exige cuidado no SkiaSharp (fontes, DPI). Mitiga:
  exportar em tamanho fixo (1080×1080 e 1080×1350) independente da tela.
- **Frame como PNG com alfa servido pela API + asset embutido de fallback**: funciona offline e permite
  trocar/adicionar frames sem republicar o app. Risco: arte/área transparente inconsistente — definir um
  **gabarito de frame** (margem segura para textos) com design. **Lacuna**: logo/PNG padrão a fornecer.
- **Gating ≠ base (proposital)**: `PermiteFrame` em vez de `AceitaInteracao`. A foto-frame é divulgação,
  não interação ao vivo; bloquear fora de `AoVivo` mataria o uso (hype/pós-jogo). Só `Cancelado` bloqueia.
- **Métrica opcional e idempotente-tolerante**: sem índice único (conta, não vota); se virar spam, aplicar
  rate-limit por torcedor reusando o padrão da base. Pode ficar para Fase 3 sem afetar o MVP.
- **Reuso do `TorcidaHub`**: evita um segundo hub. Se no futuro o frame precisar de push próprio, criar um
  grupo/evento dedicado dentro do mesmo hub, não um hub novo.
- **Sem moderação server-side**: como a imagem não trafega, não há o que moderar no backend; o app deve
  exibir aviso de responsabilidade do usuário ao compartilhar.
