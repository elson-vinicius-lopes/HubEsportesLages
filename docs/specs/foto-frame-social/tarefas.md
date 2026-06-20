# Tarefas — Foto com Frame para Redes Sociais

> Marque ao concluir. "Done" = critérios de aceite de `requisitos.md` atendidos. Agente sugerido entre [].
> Estende a base `interacao-torcida-ao-vivo` (identidade `X-Torcedor-Id`, gating, `ResultadoInteracao`).
> **Não** reimplementar o que a base já entregou — reusar.

## Fase 0 — Assets e gabarito (desbloqueio)
- [ ] Definir o **gabarito de frame** (dimensões 1080×1080 e 1080×1350, área transparente da foto,
      margem segura de texto, posição do logo). [designer-ui]
- [ ] Produzir o **frame padrão Arena Lages** (PNG com alfa) + logo, e exportar para
      `src/HubEsportesLages.Mobile/Resources/Images/frame_padrao.png`. [designer-ui]

## Fase 1 — Backend mínimo (catálogo de frames)
- [ ] **Domain**: entidades `FrameTemplate`, `CompartilhamentoFrame`; enum `EscopoFrame`;
      propriedade `Evento.PermiteFrame => Status != Cancelado`. [dev-backend]
- [ ] **Infrastructure**: `HubDbContext` (DbSets + Fluent API: **índice único** em `FrameTemplate.Slug`,
      índices de busca por evento/equipe; sem índice único em `CompartilhamentoFrame`). [dev-backend]
- [ ] **Infrastructure**: `DataSeeder` — frame global padrão (`arena-lages-padrao`) + 1 frame do evento
      AoVivo de exemplo (e opcional 1 por equipe). [dev-backend]
- [ ] **Application**: DTOs (`FrameTemplateDto`, `FramePrefillDto`, `FrameCatalogoDto`,
      `RegistrarCompartilhamentoDto`), `IFrameService`, mapeamentos. [dev-backend]
- [ ] **Infrastructure**: `FrameService` (catálogo = global ∪ evento ∪ equipes, ordenado, garante ≥1;
      monta `prefill` com título/equipes/placar/frase) + registro no `DependencyInjection`. [dev-backend]
- [ ] **Web/API**: `FramesApiController` — `GET .../frames` (leitura liberada em qualquer status);
      `POST .../frames/compartilhamentos` com **gating `PermiteFrame`** (409 em `Cancelado`, 400 sem
      `X-Torcedor-Id`). Swagger ok, `Tags("Frames")`. [dev-backend]

## Fase 2 — Mobile (composição e exportação)
- [ ] **Mobile**: botão "📸 Foto com frame" no `EventDetailPage` (habilitado exceto `Cancelado`),
      estilo `ButtonAccent`. [dev-mobile / designer-ui]
- [ ] **Mobile**: `FrameComposerPage` + `FrameComposerViewModel` — carrega `GET .../frames`,
      popula frames + campos via `prefill`; estados loading/erro/vazio; fallback frame embutido offline. [dev-mobile]
- [ ] **Mobile**: seleção de foto (`MediaPicker.PickPhotoAsync`/`CapturePhotoAsync`) + opção "sem foto". [dev-mobile]
- [ ] **Mobile**: preview com **SkiaSharp** (`SKCanvasView`): foto (crop cover) + frame PNG + textos
      (nome do time, placar, frase) atualizando ao vivo. Tokens dark de `design-arena-lages.md`. [dev-mobile / designer-ui]
- [ ] **Mobile**: exportar ≥1080×1080 → **Baixar** (galeria/arquivo) e **Compartilhar** (`Share.RequestAsync`). [dev-mobile]
- [ ] **Mobile**: ping de métrica best-effort `POST .../frames/compartilhamentos` (catch silencioso). [dev-mobile]

## Fase 3 — Extras e robustez (opcional)
- [ ] **Realtime opcional**: reusar `TorcidaHub` (grupo `evento-{id}`) para re-preencher o **placar**
      quando muda; fallback `PeriodicTimer` re-`GET .../frames`. [dev-mobile / dev-backend]
- [ ] **Admin (futuro)**: `FramesAdminApiController` (cadastrar/remover frames, métricas por frame/canal). [dev-backend]
- [ ] **Anti-abuso**: rate-limit no ping de métrica reusando o padrão da base (se virar spam). [dev-backend]
- [ ] **Testes** cobrindo critérios de aceite: catálogo garante ≥1 frame; `prefill` com/sem placar;
      gating (`GET` liberado em todos os status; `POST` 409 só em `Cancelado`, 400 sem torcedor);
      imagem composta não trafega pela API. [dev-backend / dev-mobile]
- [ ] **Spec e código revisados** (não divergem; gating `PermiteFrame` ≠ `AceitaInteracao` confirmado). [revisor-codigo]
