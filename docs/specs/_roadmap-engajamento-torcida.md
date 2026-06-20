# Roadmap — Engajamento da Torcida (Arena Lages)

> Documento-âncora do arquiteto-chefe. Amarra as **7 features de engajamento** da torcida sobre a
> **infra compartilhada** já planejada na spec base `docs/specs/interacao-torcida-ao-vivo/`.
> Não substitui as specs por feature (cada uma tem `requisitos.md` / `design.md` / `tarefas.md`);
> este arquivo é o mapa de dependências, gating e sequência de build.
>
> Stack: monorepo .NET 10, Clean Architecture (`Domain ← Application ← Infrastructure ← Web`),
> EF Core 10 + SQLite, app MAUI `Arena Lages` (MVVM, dark-only) consumindo a API REST + SignalR.
> Spec visual: `docs/design-arena-lages.md`. Origem do design: `figma/src/app/App.tsx`.

---

## 1. Visão geral das 7 features

Todas compartilham **a mesma fundação** (definida e parcialmente implementada na base): identidade
anônima do torcedor, um único hub de tempo real, gating por status do evento e camada anti-abuso.
O objetivo é **engajar a torcida em todo o ciclo do jogo** — antes (hype), durante (ao vivo) e depois
(resultado) — reusando peças, nunca reinventando.

| # | Feature | Spec | Momento do jogo | Resumo |
|---|---|---|---|---|
| 0 | **Interação da torcida ao vivo** (base: MVP/Jogador da Partida, enquete, mural, favoritar) | `interacao-torcida-ao-vivo/` (Fase 1 em implementação) | Ao vivo (+ leitura pós) | Fundação de identidade, hub, gating e anti-abuso. Tudo depende dela. |
| 1 | **Palpite do jogo (placar)** | `palpite-placar/` | **Pré-jogo** | Torcedor crava o placar antes do apito; agregado de palpites em tempo real. |
| 2 | **Disputa entre torcidas (cabo de guerra)** | `disputa-torcidas/` | **Ao vivo** | Torcedor escolhe um lado; barra de força casa × visitante ao vivo. |
| 3 | **Foto com frame para redes sociais** | `foto-frame-social/` | **Qualquer** (exceto Cancelado) | Moldura de marca composta 100% no app; backend mínimo (catálogo + métrica). |
| 4 | **Esquenta da torcida (pontos de encontro)** | `esquenta-torcida/` | **Pré-jogo** | Pontos de encontro curados, rota no mapa, confirmação de presença. |
| 5 | **Refinos da interação base** (ranking MVP, comentários/reações + moderação, enquetes de intervalo) | `interacao-torcida-ao-vivo/refinamentos.md` | **Ao vivo** (+ pós no MVP) | Addendum que estende a base; não cria fundação nova. |

> Contagem das "7": a base (0) conta como **3 capacidades** já especificadas (MVP, enquete, mural) +
> as 4 features novas autônomas (1–4). A feature 5 é um **addendum** que aprofunda a própria base.
> Para efeito de portfólio: **5 specs novas planejadas agora + a base já existente**.

### Infra compartilhada (reutilizar, não reinventar)

- **Identidade do torcedor** — header `X-Torcedor-Id` (GUID anônimo persistido no app) →
  `ITorcedorContexto`. `TorcedorId` é `string` (máx 64). Idempotência de votos/apoios/presença via
  **índice único `(Escopo, TorcedorId)`** no banco — nunca confiar só na UI.
- **Tempo real** — **UM** hub SignalR `TorcidaHub` em `/hubs/torcida`, grupos por evento `evento-{id}`.
  O serviço de domínio publica via `IHubContext` após cada escrita (REST e hub usam o **mesmo serviço**
  → uma fonte de verdade). Cliente MAUI: `Microsoft.AspNetCore.SignalR.Client` com
  `WithAutomaticReconnect`; conecta ao abrir a tela, desconecta no `OnDisappearing`; **fallback polling**
  (`PeriodicTimer`) quando offline. **Nenhuma feature cria hub novo.**
- **Gating por `Evento.Status`** (enum: `0 Agendado`, `1 AoVivo`, `2 Encerrado`, `3 Adiado`, `4 Cancelado`).
  `Evento.AceitaInteracao => Status == AoVivo` já existe. Cada feature adiciona **seu próprio método de
  janela** (`AceitaPalpite`, `AceitaDisputa`, `AceitaEsquenta`, `PermiteFrame`) sem alterar o existente.
  Escrita fora da janela → **409**; violação de regra estrutural → **422**. Leitura quase sempre liberada.
- **Anti-abuso** — tamanho limitado, rate limit por torcedor, filtro simples, moderação manual
  (`IModeracaoService` já existe). API **camelCase**; endpoints sob `/api/eventos/{slug}/...`.

---

## 2. Matriz de gating (em qual `Status` cada feature aceita ESCRITA)

Leitura (`GET`) é liberada em qualquer status salvo indicação contrária — pós-jogo vira "resultado final".

| Feature | Agendado (pré) | AoVivo (durante) | Encerrado (pós) | Adiado / Cancelado | Método de domínio |
|---|:--:|:--:|:--:|:--:|---|
| **Palpite (placar)** | ✅ escreve | 409 | 409 | 409 | `Evento.AceitaPalpite => Status==Agendado` |
| **Disputa (cabo de guerra)** | 409 | ✅ escreve | 409 | 409 | `Evento.AceitaDisputa => AceitaInteracao && EhConfronto` |
| **Esquenta (pontos de encontro)** | ✅ escreve | 409 | 409 | 409 | `Evento.AceitaEsquenta => Status==Agendado` |
| **Foto-frame social** | ✅ | ✅ | ✅ | Cancelado→409 | `Evento.PermiteFrame => Status != Cancelado` |
| **MVP / Jogador da Partida** (base + refino) | | ✅ escreve | ✅ até congelar | | `AceitaVotoMvp` (AoVivo **ou** Encerrado, até `VotacaoMvpEncerradaEm`) |
| **Enquete (base)** | | ✅ escreve | (leitura) | | `Enquete.AceitaVoto` (Ativa) |
| **Enquete de intervalo** (refino) | | ✅ + janela aberta | | | `AceitaInteracaoAoVivo` + ciclo `Aberta` |
| **Mural / comentários + reações** (base + refino) | | ✅ escreve | (leitura) | | `AceitaInteracaoAoVivo` |
| **Favoritar equipe** (base) | ✅ qualquer | ✅ | ✅ | ✅ | sem gating (preferência do usuário) |

**Insight de design:** as janelas são **complementares ao longo do tempo**, não conflitantes.
`Palpite` e `Esquenta` são **pré-jogo** (espelho INVERSO do gating base); `Disputa`, `MVP`, `enquete`,
`mural` são **ao vivo**; `Foto-frame` é **transversal** (hype → "eu tô no jogo" → placar final);
`Favoritar` é atemporal. Isso garante que **sempre há algo para o torcedor fazer**, qualquer que seja o
status — sem sobrecarregar nenhuma janela única.

---

## 3. Feature → entidades novas → endpoints → tempo real → dependências

| Feature | Entidades / enums novos (Domain) | Endpoints (camelCase, `/api/eventos/{slug}/...`) | Tempo real? | Depende de |
|---|---|---|:--:|---|
| **Base — interação ao vivo** | `JogadorEvento`, `VotoMvp`, `Enquete`, `OpcaoEnquete`, `VotoEnquete`, `MensagemTorcida`, `EquipeFavorita`; `Evento.AceitaInteracao` | `GET /torcida`; `POST /torcida/mvp`; `POST /torcida/enquete/{id}/voto`; `GET·POST /torcida/mensagens`; `POST·DELETE /favoritos/equipes/{id}`; admin (enquete/jogadores/remover msg) | **Sim** — define o `TorcidaHub` (`MvpAtualizado`, `EnqueteAtualizada`, `NovaMensagem`, `MensagemRemovida`) | Identidade, anti-abuso. **Origem da infra.** |
| **Palpite (placar)** | `PalpitePlacar` { GolsCasa 0–99, GolsVisitante 0–99 } índice único `(EventoId, TorcedorId)`; `Evento.AceitaPalpite` | `GET /palpites`; `PUT /palpites/meu`; `DELETE /palpites/meu` (opcional) | **Sim** — reusa hub: `PalpitesAtualizados(PalpiteAgregadoDto)` | Identidade, hub, `Evento.EquipeCasa/Visitante`. Pontuação do acerto = **futuro** (gamificação). |
| **Disputa (cabo de guerra)** | enum `LadoTorcida {Casa,Visitante}`; `ApoioTorcida` índice único `(EventoId, TorcedorId)`; `Evento.AceitaDisputa` | `GET /torcida/disputa`; `POST /torcida/disputa/apoio` | **Sim** — reusa hub: `DisputaAtualizada(DisputaDto)` | Identidade, hub, **`Evento.EhConfronto`** (`EquipeCasaId`/`VisitanteId` preenchidos). Ranking histórico = futuro (gamificação). |
| **Foto-frame social** | `FrameTemplate` (único em `Slug`), `CompartilhamentoFrame` (sem índice único — é contagem), enum `EscopoFrame {Global,Evento,Equipe}`; `Evento.PermiteFrame` | `GET /frames` (catálogo + prefill); `POST /frames/compartilhamentos` (métrica); admin frames/métricas | **Opcional** — reusa hub só p/ re-prefill de placar; MVP sem realtime | Identidade, `EventoDetalheDto`/`Equipe` (escudo/`CorPrimaria`/placar). **Asset de marca (PNG do frame) bloqueia o mobile.** Composição 100% client-side (SkiaSharp). |
| **Esquenta (pontos de encontro)** | `PontoEncontro` (+ `MapaUrl` calc.), `PresencaPontoEncontro` índice único `(PontoEncontroId, TorcedorId)`; `Evento.AceitaEsquenta` | `GET /esquenta`; `POST·DELETE /esquenta/{pontoId}/presenca`; admin pontos | **Sim (Fase 2)** — reusa hub: `PresencaAtualizada(pontoId, total)` | Identidade, hub, `Local` do evento (Haversine p/ distância; `null` se sem lat/long). |
| **Refinos da base** | `ReacaoMensagem` único `(MensagemTorcidaId, TorcedorId)` + enum `TipoReacao`; enum `TipoEnquete {Padrao,Intervalo}` + campos em `Enquete`; `Evento.VotacaoMvpEncerradaEm`; `MensagemTorcida.Sinalizada` | `GET /torcida/mvp/ranking`; `POST /mvp/encerrar` (admin); `POST /mensagens/{id}/reacao`; `GET /mensagens?sinalizadas=true` (admin); `GET·POST /enquete-intervalo[...]` | **Sim** — reusa hub: `MvpAtualizado` estendido, `ReacaoAtualizada`, `EnqueteIntervalo*` | **Toda a base implementada** (`VotoMvp`/`Enquete`/`MensagemTorcida`/`IModeracaoService`). Só estende. |

---

## 4. Grafo de dependências

```
                    ┌──────────────────────────────────────────────────────────────┐
                    │   INFRA COMPARTILHADA (spec base: interacao-torcida-ao-vivo)   │
                    │                                                                │
          ┌─────────┤  [A] Identidade X-Torcedor-Id → ITorcedorContexto             │
          │         │  [B] TorcidaHub (/hubs/torcida, grupo evento-{id}, IHubContext)│
          │         │  [C] Gating por Evento.Status (+ métodos AceitaXxx)            │
          │         │  [D] Anti-abuso (rate limit, filtro, IModeracaoService)        │
          │         └──────────────┬───────────────────────────────┬───────────────┘
          │                        │                               │
          ▼                        ▼                               ▼
  [A] necessária por:      [B] necessária por:            depende de DADOS específicos:
   ├─ Palpite               ├─ Palpite (PalpitesAtualizados)   ├─ Disputa → Evento.EhConfronto
   ├─ Disputa               ├─ Disputa (DisputaAtualizada)     │           (EquipeCasaId/VisitanteId)
   ├─ Esquenta              ├─ Esquenta (PresencaAtualizada)   ├─ MVP/Refino → JogadorEvento (escalação)
   ├─ Foto-frame            ├─ Refinos (Mvp/Reacao/Enquete)    ├─ Esquenta → Local (lat/long, Haversine)
   └─ Refinos               └─ Foto-frame (opcional, prefill)  └─ Foto-frame → Equipe (escudo/cor/placar)

  GAMIFICAÇÃO / PONTOS (lacuna #4 — NÃO existe ainda) — bloqueia apenas evoluções FUTURAS:
   ├─ Palpite → pontuar acerto do placar (entidade já guarda os gols → aditivo)
   ├─ Disputa → ranking histórico de torcidas
   └─ Base → conceder pontos ao interagir (Fase 3 opcional)
```

**Leitura do grafo:**
- **[A] Identidade** é pré-requisito de **toda** escrita (todas as features atribuem ação a um torcedor).
- **[B] TorcidaHub** é pré-requisito do tempo real de **todas** — e **ainda não existe no código**
  (planejado na Fase 2 da base). Até existir, Palpite/Disputa/Esquenta/Refinos operam **só por REST + polling**.
- **[C]/[D]** são transversais e baratos de reusar (cada feature só pluga seu método `AceitaXxx`).
- **Escalação de jogadores** (`JogadorEvento`) só importa para MVP/Refinos.
- **Gamificação/pontos NÃO é dependência de MVP de ninguém** — é sempre evolução aditiva e fica fora do hackathon.
- **Foto-frame é o mais desacoplado**: precisa de [A] e de um asset de marca, mas a composição é client-side
  e o backend é mínimo — pode andar quase em paralelo com a base.

---

## 5. Sequência de build recomendada (hackathon)

Critério: **maior valor percebido / menor custo**, reusando ao máximo a base **já em implementação**,
e respeitando o grafo de dependências (identidade → hub → features).

### Onda 0 — Fundação (bloqueante, já em andamento) — **terminar primeiro**
Concluir a **Fase 1 da base** (`interacao-torcida-ao-vivo`): middleware `X-Torcedor-Id`/`ITorcedorContexto`,
`HubDbContext` + índices únicos, `TorcidaService`, MVP/enquete/mural via REST. **Tudo depende disso.**
Em seguida, a **Fase 2 da base** entrega o `TorcidaHub` — que destrava o tempo real de todas as outras.

### Onda 1 — MVP do hackathon (entregar AGORA)
1. **Disputa entre torcidas (cabo de guerra)** — *o maior "uau" por real investido.* Uma entidade
   (`ApoioTorcida` + enum), um `POST`, um agregado, e um push no hub que **já existe**. Visualmente forte
   (barra casa × visitante com os tokens reais do design), 100% ao vivo, share client-side sem backend.
   É o melhor cartão de visita do tempo real.
2. **Palpite do jogo (placar)** — preenche a janela **pré-jogo** (que a base deixa vazia) com custo mínimo:
   um `PUT` upsert idempotente, agregado simples, mesmo padrão de hub. Garante engajamento **antes** do apito.
3. **Refinos da base — reações no mural + ranking MVP** — alto valor incremental e **custo quase zero**
   (estende entidades que já vão existir). Reações deixam o mural vivo; ranking MVP melhora o pós-jogo.
   Enquete de intervalo entra **se sobrar tempo** (precisa de ciclo abrir/encerrar via admin).

### Onda 2 — Logo depois (alto valor, custo médio / dependência externa)
4. **Foto-frame social** — viralização orgânica (cada foto é mídia espontânea da Arena Lages). Backend é
   trivial, mas **depende de um asset de marca** (PNG do frame com alfa + gabarito) e de trabalho de
   composição SkiaSharp no app — por isso fica logo após o MVP, paralelizável com `[designer-ui]`.
5. **Esquenta da torcida (pontos de encontro)** — ótimo para comunidade, mas é o de **maior superfície**
   (mapa, geolocalização/Haversine, CRUD admin de pontos, curadoria de conteúdo) e o realtime depende do
   hub. Entrega muito valor fora do hackathon; no evento, pode ir só por REST.

### Fica para depois (pós-hackathon)
- **Gamificação / pontos** (lacuna #4): destrava pontuação de acerto de palpite, ranking histórico de
  torcidas, pontos por interação. **Aditivo** — as entidades já guardam os dados necessários.
- **Login real** substituindo a identidade anônima (lacuna #3) — sem quebrar o contrato `X-Torcedor-Id`.
- **Render server-side de imagem** para o share, **moderação automática por IA**, **Redis backplane**.

**Justificativa da ordem:** a base é pré-requisito de tudo, então é onda 0. Disputa e Palpite são as
features de **menor custo marginal** (reusam hub + padrão de voto idempotente já prontos) e cobrem as
janelas ao-vivo e pré-jogo — máximo valor demonstrável no menor tempo. Refinos são "de graça" porque só
estendem. Foto-frame e Esquenta têm dependências externas (asset / mapa / CRUD) que os tornam mais caros,
apesar do alto valor — daí a onda 2.

---

## 6. Riscos transversais e mitigação

| Risco | Impacto | Mitigação |
|---|---|---|
| **Escala do SignalR** | Backplane in-memory só atende **1 instância**; vários jogos simultâneos ou pico de torcida derrubam o realtime. | Grupos por `evento-{id}` isolam o tráfego; **payloads agregados** (não por-voto) reduzem fan-out; **fallback polling** (`PeriodicTimer`) garante funcionamento sem o canal. Escala horizontal = **Redis backplane** (pós-hackathon). |
| **Abuso / spam / flood** | Mural e mensagens são vetor de toxicidade; votos/apoios podem ser inflados. | **Índice único `(Escopo, TorcedorId)`** torna voto/apoio/presença idempotentes no banco; **rate limit + tamanho máximo + filtro de termos** (`IModeracaoService`); **moderação manual** + flag `Sinalizada`. Lista de termos configurável (fonte a definir). |
| **Identidade anônima por dispositivo** | `X-Torcedor-Id` é GUID local → reinstalar o app zera votos; um dispositivo = um "torcedor" (burlável trocando de aparelho). | Aceitável para o hackathon (barreira ≥ esforço de fraude casual). Persistir o GUID em storage seguro; **evoluir para login real** (lacuna #3) **sem quebrar o contrato** do header. Idempotência no banco limita o dano. |
| **Moderação manual não escala** | Em pico, organizador não dá conta de revisar mural. | Filtro automático simples na entrada (bloqueia o óbvio); **auto-flag** de duplicata/flood marca `Sinalizada`; fila de moderação (`GET /mensagens?sinalizadas=true`) prioriza o que ver. IA fica fora de escopo. |
| **Gatilho de transição de `Status` indefinido** | Quem vira `Agendado→AoVivo→Encerrado`? Define quando palpite/esquenta fecham e disputa/MVP abrem. **Lacuna comum a todas as specs.** | Curto prazo: **organizador via Admin**. Futuro: **job agendado** pela data do evento. Enquanto isso, **leitura sempre liberada** evita tela morta; escritas respeitam o `Status` atual. |
| **Asset de marca atrasa o Foto-frame** | Sem o PNG do frame (alfa + gabarito), o mobile não compõe. | Tratar como **Fase 0** da feature (`[designer-ui]`), paralelizável; backend e catálogo podem ficar prontos antes; usar frame placeholder para destravar o dev mobile. |
| **Divergência spec × código** | 6 specs evoluindo em paralelo sobre infra compartilhada podem contradizer-se. | Este roadmap é a **fonte única** de gating e dependências; `[revisor-codigo]` valida cada feature contra ele; **um só `TorcidaHub` / um só `ITorcedorContexto`** força a convergência. |

---

## Referências

- Base: `docs/specs/interacao-torcida-ao-vivo/{requisitos,design,tarefas}.md` + `refinamentos.md`
- Features: `docs/specs/{palpite-placar,disputa-torcidas,foto-frame-social,esquenta-torcida}/`
- Template SDD: `docs/specs/_template/` · Design visual: `docs/design-arena-lages.md` · Desafio: `docs/desafio.md`
