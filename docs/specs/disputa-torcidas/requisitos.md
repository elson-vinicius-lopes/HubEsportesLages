# Requisitos — Disputa entre Torcidas (Cabo de Guerra)

> O "o quê" e o "porquê". Independente de stack. Fonte da verdade da feature.
> **Estende** a base `docs/specs/interacao-torcida-ao-vivo/` (mesma tela TORCIDA, mesma identidade
> anônima `X-Torcedor-Id`, mesmo hub `TorcidaHub`). Esta spec adiciona **um novo card** à tela de
> interação — não duplica MVP/enquete/mural/favoritar, apenas referencia.
> Origem visual: `figma/src/app/App.tsx` → `InteractionScreen`; tokens em `docs/design-arena-lages.md` (§2, §4.6).

## Contexto
Quando um evento é um **confronto entre duas equipes** (`Evento.EhConfronto`), a maior emoção ao vivo é
a rivalidade das torcidas. Hoje a tela TORCIDA cobre escolha individual (MVP), opinião (enquete) e
conversa (mural), mas **não mede a força coletiva de cada lado**. A "Disputa entre Torcidas" é uma
dinâmica visual de **cabo de guerra**: cada torcedor declara **uma vez** o apoio à sua torcida e vê,
em tempo real, a barra pender para um lado conforme o placar de apoios muda — ex.: **"Torcida Time A
62% × 38% Torcida Time B"**. É leve, viciante e gera compartilhamento ("minha torcida está ganhando o
cabo de guerra"), reforçando o engajamento da comunidade esportiva de Lages.

A dinâmica é **contextual ao jogo**: só faz sentido **durante** a partida (`Evento.Status == AoVivo`)
e só existe para eventos que **são confronto** (têm `EquipeCasaId` e `EquipeVisitanteId`).

## Personas
- **Torcedor** — declara apoio a um dos dois lados (1 vez por evento) e acompanha a disputa ao vivo.
- **Organizador / Fundação** — apenas garante que o evento está marcado como confronto e `AoVivo`
  (sem cadastro adicional específico desta feature).

## User stories
- Como **torcedor**, durante um jogo **ao vivo**, quero **declarar apoio à minha torcida**, para somar à força do meu lado.
- Como **torcedor**, quero ver o **cabo de guerra com o % de cada torcida atualizando em tempo real**, para sentir a disputa.
- Como **torcedor**, quero **compartilhar o resultado parcial** ("Time A 62% × 38% Time B"), para chamar a galera.
- Como **torcedor que já apoiou**, quero ver **meu lado destacado** e a contagem travada (sem poder votar de novo).
- Como **torcedor**, quando o jogo **encerra**, quero ver o **resultado final do cabo de guerra** em modo leitura.

## Critérios de aceite (testáveis)
- [ ] O card "Disputa entre Torcidas" **só aparece** quando o evento **é confronto** (`EhConfronto == true`).
      Para eventos sem duas equipes (corrida, torneio aberto), o card **não é renderizado**.
- [ ] **Apoio**: no máximo **1 por torcedor por evento** (idempotente, garantido por índice único no banco).
      Reabrir a tela mostra o lado já apoiado e a barra travada; segunda tentativa retorna o estado atual (não cria voto novo).
- [ ] O **percentual** de cada lado reflete o total de apoios e **atualiza em ≤ 2s** para quem está na tela (tempo real via `TorcidaHub`).
- [ ] Empate/zero apoios é exibido como **50% × 50%** (estado inicial), sem divisão por zero.
- [ ] Declarar apoio só é permitido com o evento **`AoVivo`**. Fora dessa janela a escrita é **recusada** (HTTP 409).
      `Agendado` → card com legenda "Disponível no início do jogo"; `Encerrado` → card em **modo leitura** (resultado final).
- [ ] **Compartilhar** gera um texto/imagem com "{TimeA} {pctA}% × {pctB}% {TimeB}" + nome do evento (share nativo do MAUI).
- [ ] **Degradação graciosa**: sem o canal de tempo real, o card ainda funciona via REST
      (carrega o placar atual, envia o apoio por POST, atualiza por refresh periódico).
- [ ] A identidade usada é a **mesma** `X-Torcedor-Id` da base (um torcedor anônimo = um apoio).

## Fora de escopo
- Apoio **trocar de lado** depois de declarado (o voto é definitivo no evento; ver Decisões no design).
- Apoio em eventos **não-confronto** (corridas, torneios com >2 equipes).
- Ranking histórico de "torcida mais forte" entre eventos / temporada (futuro, depende de gamificação — lacuna #4).
- Geração de imagem server-side para o share (o share monta o texto/figura no app).
- Anti-bot avançado / verificação de presença física (check-in real no estádio) — usa só a identidade anônima + rate limit.

## Dependências (specs relacionadas / lacunas da API)
- **Base obrigatória**: `docs/specs/interacao-torcida-ao-vivo/` — reusa `ITorcedorContexto` (`X-Torcedor-Id`),
  o `TorcidaHub` (`/hubs/torcida`, grupos `evento-{id}`), o gating por `Evento.AceitaInteracao`, e a tela `InteractionPage`.
- **Confronto no evento**: requer `Evento.EquipeCasaId` e `Evento.EquipeVisitanteId` preenchidos (já existem no Domain).
- **Identidade do torcedor** (lacuna #3): apoio anônimo por dispositivo; evoluir para login sem quebrar o contrato.
- **Compartilhamento**: usa `Share` do MAUI Essentials (sem dependência de backend).
