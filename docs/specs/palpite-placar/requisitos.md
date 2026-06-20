# Requisitos — Palpite do Jogo (placar)

> O "o quê" e o "porquê". Independente de stack. Fonte da verdade da feature.
> Origem do design: `figma/src/app/App.tsx` → `InteractionScreen` (tela **TORCIDA**), card de palpite
> ("Qual vai ser o placar de hoje?"). Spec visual: `docs/design-arena-lages.md` (§2 tokens, §4.6 TORCIDA).
> **Estende** — não duplica — `docs/specs/interacao-torcida-ao-vivo/` (infra compartilhada: identidade
> `X-Torcedor-Id`, `TorcidaHub` em `/hubs/torcida`, gating por `Evento.Status`, anti-abuso, REST camelCase).

## Contexto
Antes da bola rolar, o torcedor quer "cravar" o placar final do jogo. Esta feature deixa o torcedor
**registrar um palpite de placar** (ex.: `Time A 2 x 1 Time B`) e ver, de forma agregada, **o que a
torcida acha** — distribuição dos palpites e a comparação entre os dois times (quem a torcida acredita
que vence, e por quanto).

Diferente da Interação Ao Vivo (que acontece **durante** o jogo), o palpite é **pré-jogo**: faz sentido
**enquanto o evento está `Agendado`** e **trava no apito inicial** (quando vira `AoVivo`/`Encerrado`).
O **gating é INVERSO** ao da spec base: lá a escrita exige `AoVivo`; aqui a escrita exige `Agendado`.

## Personas
- **Torcedor** — registra/edita o palpite enquanto o jogo não começou; depois acompanha o consenso da torcida.
- **Organizador / Fundação** — não cadastra nada novo: o palpite reaproveita os times já definidos no evento.

## User stories
- Como **torcedor**, antes do jogo (`Agendado`), quero **registrar meu palpite de placar** (gols casa × gols
  visitante), para participar e me comprometer com um resultado.
- Como **torcedor**, quero **editar meu palpite** enquanto o jogo **não começou**, para mudar de ideia.
- Como **torcedor**, quero ver o **percentual dos palpites da torcida** (placares mais cravados) e a
  **comparação entre os dois times** (% que crava vitória do mandante / empate / visitante), para sentir o clima.
- Como **torcedor**, quero que, **iniciada a partida**, o palpite **trave** (vire leitura), para que ninguém
  palpite "sabendo" do andamento do jogo.

## Critérios de aceite (testáveis)
- [ ] O card **"Qual vai ser o placar de hoje?"** só permite **registrar/editar** quando o evento está
      **`Agendado`** (status=0). Em `AoVivo` (1) ou `Encerrado` (2) o card fica em **modo leitura**
      (mostra meu palpite + agregados, sem inputs).
- [ ] Palpite é **único por (EventoId, TorcedorId)** (idempotente por índice no banco): registrar de novo
      **atualiza** o palpite existente (upsert) enquanto `Agendado`; não cria duplicado.
- [ ] Os campos `golsCasa`/`golsVisitante` são inteiros **0–99** (validação); valores fora da faixa → 422.
- [ ] **Escritas fora da janela**: `POST`/`PUT` de palpite em evento que **não está `Agendado`** são
      **recusadas** pelo backend com **409** (`Conflict`) e mensagem clara ("palpites encerrados").
- [ ] O agregado retorna: **distribuição dos placares** (top N + "outros"), **% de palpites em cada placar**,
      e **comparação dos times** (`percentualCasa`, `percentualEmpate`, `percentualVisitante`) + `totalPalpites`.
- [ ] **Atualização do agregado**: enquanto o evento está `Agendado` e a tela aberta, novos palpites de outros
      torcedores refletem no agregado em **≤ 2s** (tempo real via `TorcidaHub`); com o canal indisponível, a tela
      ainda funciona via REST (carrega estado e atualiza por refresh periódico — **degradação graciosa**).
- [ ] Reabrir a tela mostra **meu palpite já registrado** preenchido (e travado se o jogo já começou).
- [ ] O botão/CTA de palpite reflete o estado por status: `Agendado` = "Palpitar placar"; `AoVivo`/`Encerrado`
      = "Ver palpites da torcida" (leitura). Em `Adiado`/`Cancelado` o card fica oculto ou desabilitado.

## Fora de escopo
- **Acerto/pontuação** do palpite após o resultado real (gamificação, "quem cravou"): depende da lacuna #4
  (pontos) e do placar final consolidado. Fica para fase futura (ver Dependências).
- Palpite de outros mercados (artilheiro, primeiro a marcar, total de gols como over/under). Apenas **placar exato**.
- Aposta com valor / dinheiro. É **engajamento**, não aposta.
- Edição do palpite após o início do jogo (intencional — trava por gating).

## Dependências (specs relacionadas / lacunas da API)
- **Spec base** `docs/specs/interacao-torcida-ao-vivo/` — reusa: identidade anônima `X-Torcedor-Id` →
  `ITorcedorContexto`; `TorcidaHub` (`/hubs/torcida`, grupo `evento-{id}`); padrão de DTO/serviço/gating;
  índice único para idempotência. **Não recriar** essas peças.
- **Times do evento** (`Evento.EquipeCasa` / `Evento.EquipeVisitante`) já existentes — usados nos rótulos do card.
- **Quem vira o status** Agendado→AoVivo: organizador via Admin (ou job futuro). É o que **fecha** os palpites.
- **(Futuro) Acerto/pontuação**: depende do placar final (`placarCasa`/`placarVisitante` no Encerrado) e da
  lacuna #4 (gamificação). Não bloqueia esta entrega.
