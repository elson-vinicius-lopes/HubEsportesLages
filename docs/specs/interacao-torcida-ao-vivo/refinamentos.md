# Refinamentos da Interação Base — MVP, Comentários, Enquetes de Intervalo

> **Addendum** sobre a spec base `interacao-torcida-ao-vivo/` (`requisitos.md`, `design.md`,
> `tarefas.md`). Aqui ficam apenas os **deltas**: o que muda/estende sobre o que já existe. Não
> duplica a base — referencia-a. Mantém a mesma infra compartilhada (identidade `X-Torcedor-Id`
> → `ITorcedorContexto`; hub único `TorcidaHub` em `/hubs/torcida`, grupos `evento-{id}`;
> publicação via `IHubContext` no mesmo serviço de domínio; gating por `Evento.Status`;
> anti-abuso e `IModeracaoService`; API camelCase; idempotência por índice único).
> Origem do design: `figma/src/app/App.tsx` → `InteractionScreen`. Visual: `docs/design-arena-lages.md` (§4.6).

## Escopo deste addendum
Três refinamentos da interação base, na numeração do desafio:

1. **(2) Melhor Jogador (MVP) — ranking em tempo real.** Ranking dos mais votados atualizando ao vivo;
   votação permitida **durante OU após** a partida; resultado final com destaque visual do eleito.
2. **(4) Comentários — reações + moderação anti-spam/ofensivo.** O feed MVP (nome + horário) **já está
   coberto** pela base (`MensagemTorcida`/mural). Aqui adiciona-se **curtidas/reações** nas mensagens e
   reforça-se a **moderação básica** (anti-spam/filtro ofensivo).
3. **(6) Enquetes de intervalo.** Enquetes rápidas de **múltipla escolha** criadas pelo app/organização
   no **intervalo** do jogo, com **resultado parcial após votar** (ex.: "Quem foi melhor no 1º tempo?",
   "O que achou da arbitragem?").

> **O que NÃO muda:** entidades, DTOs, endpoints e fluxo SignalR já descritos na base continuam válidos.
> Onde abaixo aparece "estende X", leia "adiciona campos/rotas/eventos a X sem reescrevê-lo".

---

## Delta de Gating (importante — difere da base)

A base aplica gating **`AoVivo`** uniformemente em todas as escritas. Estes refinamentos exigem
**três janelas distintas** — centralizar num único helper em vez de repetir `AceitaInteracao`:

| Sub-feature | Janela de **escrita** permitida | Status (enum) | Fora da janela |
|---|---|---|---|
| **MVP / ranking** (delta) | `AoVivo` **OU** `Encerrado` | 1 ou 2 | 409/422 |
| Mural / **reações** (delta) | `AoVivo` (igual à base; reações só no jogo) | 1 | 409/422 |
| **Enquete de intervalo** | `AoVivo` **E** enquete `Ativa` na janela de intervalo | 1 + `Ativa` | 409/422 |
| Leitura (qualquer um) | sempre liberada | qualquer | — |

Domain ganha métodos de janela explícitos (substituem o uso genérico de `AceitaInteracao` nestes fluxos):
- `Evento.AceitaVotoMvp` ⇒ `Status is AoVivo or Encerrado`.
- `Evento.AceitaInteracaoAoVivo` ⇒ `Status == AoVivo` (alias de `AceitaInteracao`; usado por reações).
- `EnqueteIntervalo.AceitaVoto(agora)` ⇒ `Ativa && evento.Status == AoVivo` (a enquete já é, por natureza,
  publicada no intervalo; não há janela de horário rígida — controla-se por `Ativa`).

---

## 1. (2) Melhor Jogador (MVP) — ranking em tempo real

### O que muda em relação à base
A base já tem `JogadorEvento`, `VotoMvp` (único por `(EventoId, TorcedorId)`), tally agregado e o evento
SignalR `MvpAtualizado`. **Não recriar nada disso.** Deltas:

- **Gating ampliado**: voto também aceito em `Encerrado` (ver tabela acima) — torcedor que entra na tela em
  modo "Ver resultados" ainda pode votar enquanto o organizador não fechar a votação. Adicionar flag de
  corte opcional `Evento.VotacaoMvpEncerradaEm?` (DateTime?) para o organizador **congelar** o resultado;
  quando preenchida, voto retorna 409 mesmo `AoVivo`.
- **Ranking ordenado**: o tally passa a ser um **ranking** (ordenado por votos desc, depois nome) com
  `posicao` e `percentual`, não só contagem.
- **Eleito final**: ao congelar (ou ao `Encerrado`), o 1º colocado é marcado como `eleito = true`; empate
  resolve por timestamp do voto que primeiro atingiu a contagem (determinístico).

### Domain
- `Evento`: novo campo `VotacaoMvpEncerradaEm? : DateTime?` e método `AceitaVotoMvp`.
- Nenhuma entidade nova obrigatória. (Opcional, se quiser persistir o resultado: `ResultadoMvp`
  { EventoId, JogadorEventoId, Votos, CongeladoEm } — só se houver requisito de auditoria pós-jogo.)

### Application
- Estender `TorcidaEstadoDto.mvp` (não criar DTO novo): adicionar `votacaoAberta: bool`,
  `eleito?: { jogadorEventoId, nome, votos, percentual }`, e em cada candidato `posicao` e `percentual`.
- Novo DTO de push: `RankingMvpDto { eventoId, votacaoAberta, candidatos:[{jogadorEventoId, nome, posicao, votos, percentual}], eleito? }`.
- `ITorcidaService`: novo método `EncerrarVotacaoMvp(eventoId)` (admin) e ajuste de `VotarMvp` para usar
  `AceitaVotoMvp`.

### Infrastructure
- `TorcidaService.VotarMvp`: gating por `AceitaVotoMvp` + checagem de `VotacaoMvpEncerradaEm`. Mantém
  idempotência pelo índice único existente. Recalcula ranking (GROUP BY + ORDER BY) e publica.
- `TorcidaService.EncerrarVotacaoMvp`: seta `VotacaoMvpEncerradaEm = now`, calcula eleito, publica push final.

### Web
- **SignalR**: reaproveita o evento existente, agora carregando ranking ordenado:
  `MvpAtualizado(RankingMvpDto)` (payload estendido — clientes antigos ainda leem os campos básicos).
- **API REST** (novos/estendidos sob `/api/eventos/{slug}/torcida`):
  - `GET .../torcida/mvp/ranking` → `RankingMvpDto` (atalho de leitura; idêntico ao bloco `mvp` do estado).
  - **Admin**: `POST /api/eventos/{id}/torcida/mvp/encerrar` → congela votação e retorna eleito (gating: só
    o organizador; valida `Status in {AoVivo, Encerrado}`).

### Mobile (Arena Lages, MAUI)
- `InteractionViewModel`: bloco MVP vira **lista ordenada com barra de % e badge de posição** (1º/2º/3º).
  Atualiza pelo handler `MvpAtualizado` (payload `RankingMvpDto`); fallback polling 5s (igual base).
- Estado `Encerrado` com votação ainda aberta: tela em modo "resultado parcial" **com voto habilitado**;
  quando `votacaoAberta == false`, mostra o **eleito em destaque** (`accentLime`, coroa/medalha) e trava o voto.
- Visual conforme `docs/design-arena-lages.md` §4.6 (card "Jogador da Partida").

---

## 2. (4) Comentários — reações + moderação

### O que muda em relação à base
O feed simples (nome + horário, `MensagemTorcida`, push `NovaMensagem`/`MensagemRemovida`, limite 140 chars,
rate limit, filtro simples) **já está coberto** — não reescrever. Deltas: **reações nas mensagens** e
**reforço de moderação anti-spam/ofensivo**.

### Domain (entidades novas)
- `ReacaoMensagem` { Id, MensagemTorcidaId, TorcedorId, Tipo (enum), CriadoEm } — **único por
  `(MensagemTorcidaId, TorcedorId)`** (1 reação por torcedor por mensagem; trocar substitui).
- `TipoReacao` (enum): `0 Curtir`, `1 Forca`, `2 Aplauso`, `3 Riso` (curtida = reação padrão). Conjunto
  fechado e pequeno para caber no card; evolutível.

### Application
- `MensagemDto` estende com `reacoes: { tipo, total }[]` e `minhaReacao?: TipoReacao`.
- Comando `ReagirMensagemDto { tipo }` (e remoção = reagir com a mesma reação, toggle).
- DTO de push `ReacaoAtualizadaDto { mensagemId, reacoes:[{tipo,total}] }`.
- `ITorcidaService`: `ReagirMensagem(mensagemId, tipo)` (toggle idempotente).
- **Moderação** (estende `IModeracaoService`, já existe): formalizar `AvaliarTexto(texto) → ResultadoModeracao`
  { aprovado, motivo? } com (a) filtro de termos ofensivos (lista configurável), (b) **anti-spam**:
  rate limit já existente + detecção de mensagem **duplicada** (mesmo torcedor, mesmo texto em janela curta)
  e de flood de links. Mensagem reprovada → 422 com `motivo`; mensagem suspeita → entra `Removida=false` mas
  flagada para revisão (campo `MensagemTorcida.Sinalizada : bool`, delta no Domain).

### Infrastructure
- `HubDbContext`: `DbSet<ReacaoMensagem>` + índice único `(MensagemTorcidaId, TorcedorId)`; coluna
  `MensagemTorcida.Sinalizada`.
- `TorcidaService.ReagirMensagem`: upsert idempotente (índice único), recalcula totais por tipo, publica.
- `ModeracaoService`: implementa `AvaliarTexto` (filtro + anti-spam duplicata/flood); chamado no envio de
  mensagem **antes** de persistir.

### Web
- **SignalR**: novo evento `ReacaoAtualizada(ReacaoAtualizadaDto)` no grupo `evento-{id}`.
- **API REST**:
  - `POST /api/eventos/{slug}/torcida/mensagens/{mensagemId}/reacao` { tipo } → 200 (toggle) / 409 (fora de
    `AoVivo`) / 404. Gating: `AceitaInteracaoAoVivo`.
  - `DELETE` da mesma rota é equivalente ao toggle-off (opcional; o POST já alterna).
  - **Admin/moderação** (estende a base): `GET /api/eventos/{id}/torcida/mensagens?sinalizadas=true` (fila de
    revisão) e o `DELETE .../mensagens/{id}` já existente.

### Mobile (Arena Lages, MAUI)
- Card de mensagem ganha **linha de reações** (chips com emoji + contador); tap alterna a reação do torcedor
  (UI otimista, confirma via REST). Handler `ReacaoAtualizada` atualiza contadores ao vivo; fallback polling.
- Erro de moderação (422) → toast com `motivo` ("mensagem repetida"/"conteúdo não permitido"); não limpa o input.
- Visual: `accentVioletMid` (família mural/enquete) conforme §4.6.

---

## 3. (6) Enquetes de intervalo

### O que muda em relação à base
A base tem **uma** `Enquete` por evento (campo `Ativa`), voto único por `(EnqueteId, TorcedorId)`, push
`EnqueteAtualizada`. Os refinos: a enquete passa a ser **criada sob demanda no intervalo** (várias por jogo,
ciclo de vida próprio) e **múltipla escolha** continua sendo 1 voto por torcedor, mas com **resultado parcial
revelado só após votar**. Reaproveita a estrutura de enquete/opções/voto; adiciona o conceito de "intervalo".

### Domain
- Reusar `Enquete`/`OpcaoEnquete`/`VotoEnquete` da base, adicionando à `Enquete`:
  - `Tipo : TipoEnquete` (enum: `0 Padrao`, `1 Intervalo`) — distingue a enquete persistente da rápida de intervalo.
  - `AbertaEm? : DateTime?`, `FechadaEm? : DateTime?` — ciclo de vida (organizador abre no intervalo, fecha ao voltar).
  - `RevelaParcialAposVoto : bool` (default `true`) — gate de visibilidade do % (só vê quem votou).
  - Método `AceitaVoto(agora)` ⇒ `Ativa && AbertaEm != null && FechadaEm == null`.
- Regra: **N enquetes de intervalo por evento** (não mais 1). Apenas **uma `Intervalo` `Ativa` por vez** por
  evento (índice/validação) para não confundir a UI.

### Application
- DTO `EnqueteIntervaloDto { id, pergunta, tipo, ativa, opcoes:[{id, texto, votos?, percentual?}], minhaOpcaoId?, revelado }`
  onde `votos`/`percentual` vêm **null** enquanto o torcedor não votou e `revelado == false` (privacidade do
  resultado parcial). `TorcidaEstadoDto` ganha `enqueteIntervaloAtiva?: EnqueteIntervaloDto`.
- Comando reaproveita `VotarEnqueteDto { opcaoId }`.
- DTO admin `CriarEnqueteIntervaloDto { pergunta, opcoes:[texto], revelaParcialAposVoto? }`.
- DTO de push `EnqueteIntervaloAtualizadaDto { enqueteId, total, opcoes:[{id, votos, percentual}] }` (push só
  agrega totais; o cliente decide exibir conforme `revelado`).
- `ITorcidaService`: `VotarEnqueteIntervalo(enqueteId, opcaoId)` (ou reusar `VotarEnquete` com checagem de
  janela), e admin `CriarEnqueteIntervalo`, `EncerrarEnqueteIntervalo`.

### Infrastructure
- `HubDbContext`: colunas novas em `Enquete` (`Tipo`, `AbertaEm`, `FechadaEm`, `RevelaParcialAposVoto`); índice
  filtrado garantindo no máx. 1 `Intervalo` ativa por evento. Índice único de voto **já existe** na base.
- `TorcidaService`: voto valida `AceitaVoto(now)` (gating de intervalo); idempotência pelo índice único
  existente; recalcula percentuais; publica `EnqueteIntervaloAtualizada`. **Não revela** `votos/percentual`
  no estado para quem não votou (filtra no mapeamento).
- `DataSeeder`: para o evento AoVivo de exemplo, semear **uma enquete de intervalo** ("Quem foi melhor no 1º
  tempo?") já aberta, para demo.

### Web
- **SignalR**: `EnqueteIntervaloAberta(EnqueteIntervaloDto)`, `EnqueteIntervaloAtualizada(...)`,
  `EnqueteIntervaloEncerrada(enqueteId)` no grupo `evento-{id}`.
- **API REST**:
  - `GET  /api/eventos/{slug}/torcida/enquete-intervalo` → enquete de intervalo ativa (ou 204 se nenhuma).
  - `POST /api/eventos/{slug}/torcida/enquete-intervalo/{id}/voto` { opcaoId } → 200 com parcial revelado /
    409 (fora da janela) / 409 (já votou — idempotente). Gating: `Enquete.AceitaVoto`.
  - **Admin**: `POST /api/eventos/{id}/torcida/enquete-intervalo` (cria + abre), `POST .../{id}/encerrar`.
- **Gating** central: usa o helper de janela "enquete de intervalo" (AoVivo + Ativa + Aberta/não Fechada).

### Mobile (Arena Lages, MAUI)
- Novo bloco/sheet **"Enquete Rápida (Intervalo)"** que **aparece** quando chega `EnqueteIntervaloAberta`
  (ou ao carregar estado com `enqueteIntervaloAtiva`). Antes de votar: lista de opções sem %; após votar:
  barras de % parciais (handler `EnqueteIntervaloAtualizada`), voto travado.
- `EnqueteIntervaloEncerrada` → some/colapsa o bloco com resultado final. Fallback polling do endpoint GET.
- Visual conforme §4.6 (card "Enquete Rápida" — 3 opções → barras de %), `accentVioletMid`.

---

## Resumo de novidades (consolidado)

### Entidades / enums novos
- `ReacaoMensagem` (+ índice único `(MensagemTorcidaId, TorcedorId)`), enum `TipoReacao`.
- Enum `TipoEnquete` (Padrao/Intervalo) + campos em `Enquete` (`Tipo`, `AbertaEm`, `FechadaEm`, `RevelaParcialAposVoto`).
- Campos em `Evento` (`VotacaoMvpEncerradaEm`), `MensagemTorcida` (`Sinalizada`).
- (Opcional) `ResultadoMvp` para auditoria.

### Endpoints novos
- `GET  /api/eventos/{slug}/torcida/mvp/ranking`
- `POST /api/eventos/{id}/torcida/mvp/encerrar` (admin)
- `POST /api/eventos/{slug}/torcida/mensagens/{mensagemId}/reacao`
- `GET  /api/eventos/{id}/torcida/mensagens?sinalizadas=true` (admin/moderação)
- `GET  /api/eventos/{slug}/torcida/enquete-intervalo`
- `POST /api/eventos/{slug}/torcida/enquete-intervalo/{id}/voto`
- `POST /api/eventos/{id}/torcida/enquete-intervalo` (admin) e `POST .../{id}/encerrar` (admin)

### Eventos SignalR novos/estendidos (mesmo `TorcidaHub`, grupo `evento-{id}`)
- `MvpAtualizado` → payload estendido `RankingMvpDto`.
- `ReacaoAtualizada(ReacaoAtualizadaDto)`.
- `EnqueteIntervaloAberta` / `EnqueteIntervaloAtualizada` / `EnqueteIntervaloEncerrada`.

---

## Checklist de tarefas (deltas, em fases)

> "Done" = critérios da base atendidos + os deltas abaixo. Agente sugerido entre [].

### Fase R1 — MVP ranking em tempo real
- [ ] **Domain**: `Evento.VotacaoMvpEncerradaEm` + `AceitaVotoMvp`; (opcional) `ResultadoMvp`. [dev-backend]
- [ ] **Application**: estender `TorcidaEstadoDto.mvp` (posicao/percentual/votacaoAberta/eleito); `RankingMvpDto`;
      `EncerrarVotacaoMvp` em `ITorcidaService`. [dev-backend]
- [ ] **Infrastructure**: `TorcidaService` — gating `AceitaVotoMvp`, ranking ordenado, congelar/eleger, publicar
      `MvpAtualizado` estendido. [dev-backend]
- [ ] **Web/API**: `GET .../mvp/ranking`; `POST .../mvp/encerrar` (admin). Swagger. [dev-backend]
- [ ] **Mobile**: ranking ordenado com % e badge de posição; modo `Encerrado` com voto ainda aberto; destaque
      do eleito ao congelar; handler `MvpAtualizado` + polling. [dev-mobile / designer-ui]

### Fase R2 — Reações + moderação nos comentários
- [ ] **Domain**: `ReacaoMensagem` (índice único), `TipoReacao`, `MensagemTorcida.Sinalizada`. [dev-backend]
- [ ] **Application**: estender `MensagemDto` (reacoes/minhaReacao); `ReagirMensagemDto`; `ReacaoAtualizadaDto`;
      `ReagirMensagem` em `ITorcidaService`; `IModeracaoService.AvaliarTexto`. [dev-backend]
- [ ] **Infrastructure**: `DbSet<ReacaoMensagem>` + índice; `TorcidaService.ReagirMensagem` (toggle idempotente);
      `ModeracaoService.AvaliarTexto` (filtro ofensivo + anti-spam duplicata/flood) no envio. [dev-backend]
- [ ] **Web**: `POST .../mensagens/{id}/reacao`; `GET .../mensagens?sinalizadas=true`; push `ReacaoAtualizada`. [dev-backend]
- [ ] **Mobile**: chips de reação no card (toggle otimista); handler `ReacaoAtualizada`; toast de moderação (422). [dev-mobile / designer-ui]

### Fase R3 — Enquetes de intervalo
- [ ] **Domain**: `TipoEnquete`; campos `Tipo/AbertaEm/FechadaEm/RevelaParcialAposVoto` em `Enquete`;
      `Enquete.AceitaVoto`; regra "1 intervalo ativa por evento". [dev-backend]
- [ ] **Application**: `EnqueteIntervaloDto` (com privacidade do parcial), `CriarEnqueteIntervaloDto`,
      `EnqueteIntervaloAtualizadaDto`; métodos no `ITorcidaService`. [dev-backend]
- [ ] **Infrastructure**: colunas/índice em `Enquete`; voto com gating `AceitaVoto` + ocultar % de quem não
      votou; seed de enquete de intervalo demo; publicar pushes. [dev-backend]
- [ ] **Web/API**: `GET`/`POST voto`/admin (criar/encerrar) de enquete-intervalo; pushes
      `EnqueteIntervaloAberta/Atualizada/Encerrada`. [dev-backend]
- [ ] **Mobile**: bloco/sheet de enquete rápida que aparece no `Aberta`; parcial só após votar; encerra ao
      `Encerrada`; fallback polling. [dev-mobile / designer-ui]

### Fase R4 — Fechamento
- [ ] **Testes** dos deltas: voto MVP em `Encerrado` aceito e bloqueado após congelar; ranking ordenado/eleito;
      reação idempotente (1 por torcedor/mensagem); moderação reprova duplicata/ofensivo (422); enquete de
      intervalo — gating de janela, 1 voto, parcial oculto antes de votar. [dev-backend]
- [ ] **Spec e código revisados** (deltas não divergem da base). [revisor-codigo]
