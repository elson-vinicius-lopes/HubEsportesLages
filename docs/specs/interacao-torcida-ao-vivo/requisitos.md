# Requisitos — Interação da Torcida Ao Vivo

> O "o quê" e o "porquê". Independente de stack. Fonte da verdade da feature.
> Origem do design: `figma/src/app/App.tsx` → `InteractionScreen` (`App.tsx:823`), aberta pelo
> botão `BtnInverted` **"Interagir com a Torcida"** (`App.tsx:809`) e pelo `BtnGhost` **"Interagir"**
> (`App.tsx:716`). Spec visual: `docs/design-arena-lages.md` (§4.6) — lacunas de API #5 e #6.

## Contexto
Durante os jogos, o torcedor quer participar em tempo real. No protótipo, a tela **TORCIDA** existe,
mas é **100% mock** (estado local React, sem back-end): votar no Jogador da Partida (MVP), responder
enquete, mandar mensagem no mural e favoritar a equipe. Esta feature transforma isso em interação
**real, ao vivo e plugada na API do hub**, fortalecendo o engajamento da comunidade esportiva de Lages.

A interação é **contextual ao jogo**: só faz sentido **durante** a partida (evento `AoVivo`).

## Personas
- **Torcedor** — vota, responde enquete, comenta no mural, favorita a equipe.
- **Organizador / Fundação** — configura a enquete e a lista de jogadores (escalação) do evento; modera mensagens.

## User stories
- Como **torcedor**, durante um jogo **ao vivo**, quero **votar no Jogador da Partida**, para participar da escolha.
- Como **torcedor**, quero **responder à enquete** e ver o **percentual atualizar em tempo real**.
- Como **torcedor**, quero **mandar uma mensagem no mural** e ver as dos outros **em tempo real**.
- Como **torcedor**, quero **favoritar minha equipe**, para acompanhar todos os jogos dela.
- Como **organizador**, quero **cadastrar a enquete e a escalação** de um evento.
- Como **organizador**, quero **remover mensagens impróprias** do mural.

## Critérios de aceite (testáveis)
- [ ] O botão **"Interagir"** só fica **habilitado quando o evento está `AoVivo`** (status=1).
      `Agendado` → botão com legenda "Disponível no início do jogo"; `Encerrado` → "Ver resultados" (modo leitura).
- [ ] Voto de **MVP**: no máximo **1 por torcedor por evento** (idempotente). Reabrir a tela mostra o voto já registrado.
- [ ] Voto na **enquete**: no máximo **1 por torcedor por enquete**; o **%** reflete o total e **atualiza em ≤ 2s**
      para quem está na tela (tempo real).
- [ ] **Mensagem** enviada aparece para os outros torcedores conectados em **≤ 2s**, com nome/handle e horário.
      Mensagem vazia é rejeitada; tamanho limitado (≤ 140 chars); rate limit (ex.: 1 msg / 3s por torcedor).
- [ ] **Favoritar/desfavoritar** equipe persiste e aparece no Perfil.
- [ ] **Degradação graciosa**: sem o canal de tempo real, a tela ainda funciona via REST
      (carrega o estado atual, envia por POST, atualiza por refresh periódico).
- [ ] Escritas (voto/mensagem) em evento que **não está `AoVivo`** são **recusadas** pelo backend (HTTP 409/422).

## Fora de escopo
- Chat privado / DM entre torcedores.
- Moderação automática por IA (apenas filtro simples + moderação manual).
- Transmissão de vídeo/áudio.

## Dependências (specs relacionadas / lacunas da API)
- **Identidade do torcedor** (lacuna #3 do design): votos/mensagens precisam ser atribuídos a alguém.
  Fallback para o hackathon: identidade **anônima por dispositivo** (`X-Torcedor-Id` = GUID gerado e
  persistido no app). Decisão a confirmar com a spec de usuário.
- **Escalação / jogadores do evento** (lacuna #6): candidatos a MVP. Cadastrados pelo organizador.
- **Pontos / gamificação** (lacuna #4): dar pontos ao interagir é **opcional** (Fase 3).
