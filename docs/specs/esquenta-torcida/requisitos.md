# Requisitos — Esquenta da Torcida (Pontos de Encontro)

> O "o quê" e o "porquê". Independente de stack. Fonte da verdade da feature.
> Spec **irmã** de `docs/specs/interacao-torcida-ao-vivo/` (interação **durante** o jogo). Esta feature cobre
> o momento **antes** do jogo (pré-jogo). Reutiliza a infra compartilhada (identidade `X-Torcedor-Id`,
> `TorcidaHub`, gating por `Evento.Status`, anti-abuso/moderação) descrita lá — **não duplicar**, referenciar.
> Spec visual: `docs/design-arena-lages.md` (§4.5 EventDetailPage, card Local / "Ver Rota" / `mapaUrl`; §2 tokens).

## Contexto
Antes de um jogo, a torcida quer **se reunir**: combinar um bar, uma roda de samba, um ponto na frente do
estádio. Hoje o app só mostra o **local oficial do evento** (`Local.mapaUrl`); não há onde divulgar nem
descobrir os **pontos de encontro** da torcida ("Esquenta da torcida — Bar do Centro 17h, a 800 m do
estádio"). Esta feature cria uma **área de pré-jogo** no detalhe do evento com uma **lista curada de pontos
de encontro** (local, horário, endereço, distância do estádio, descrição, regras, mapa) e permite ao
torcedor **confirmar presença** em um ponto, fortalecendo o encontro presencial da comunidade.

A diferença essencial em relação à spec irmã é a **janela temporal**: o esquenta é **contextual ao
pré-jogo** — só faz sentido **enquanto o evento ainda não começou** (`Agendado`). Quando o jogo fica
`AoVivo`, a tela passa a **leitura** (já estão lá) e a energia migra para a interação ao vivo.

## Personas
- **Torcedor** — vê os pontos de encontro, abre a rota no mapa e confirma/cancela presença em um ponto.
- **Organizador / Fundação** — cadastra, edita e remove os pontos de encontro de um evento; modera a lista.

## User stories
- Como **torcedor**, no pré-jogo, quero **ver os pontos de encontro recomendados** (com horário, endereço,
  distância do estádio e descrição), para escolher onde me reunir antes da partida.
- Como **torcedor**, quero **abrir a rota no mapa** de um ponto de encontro, para chegar lá.
- Como **torcedor**, quero **confirmar minha presença** em um ponto e **ver quantos já confirmaram**, para
  saber onde a galera vai estar.
- Como **torcedor**, quero **cancelar minha presença**, caso mude de planos.
- Como **organizador**, quero **cadastrar/editar/remover pontos de encontro** de um evento, para curar a lista.

## Critérios de aceite (testáveis)
- [ ] O detalhe de um evento **`Agendado`** mostra a seção **"Esquenta da torcida"** com a lista de pontos de
      encontro ordenada por **horário** (e, como desempate, por **distância** ao local do evento).
- [ ] Cada ponto exibe: **nome**, **horário** (`HH:mm`), **endereço**, **distância** formatada (ex.: "a 800 m"
      / "a 1,2 km"), **descrição**, **regras** (quando houver) e o total de **presenças confirmadas**.
- [ ] O botão **"Ver Rota"** abre o **`mapaUrl`** do ponto (mesmo padrão do `Local` do evento — coordenadas →
      Google Maps, com fallback por endereço). Nenhum ponto fica sem link de mapa.
- [ ] **Confirmar presença**: no máximo **1 confirmação por torcedor por ponto** (idempotente). Reabrir a tela
      mostra a presença já registrada (estado "Confirmado") e o contador correto.
- [ ] **Cancelar presença** remove a confirmação e decrementa o contador; é idempotente (cancelar de novo é no-op).
- [ ] **Gating de janela**: confirmar/cancelar presença só é aceito enquanto o evento está **`Agendado`**
      (pré-jogo). Em `AoVivo`/`Encerrado`/`Adiado`/`Cancelado` o backend **recusa as escritas** (HTTP 409),
      e a **leitura** da lista continua liberada em qualquer status.
- [ ] A **identidade** do torcedor usa o cabeçalho `X-Torcedor-Id` (mesma infra da spec irmã). Sem o header,
      a escrita retorna **400** com mensagem orientando a enviá-lo.
- [ ] **Tempo real (opcional/Fase 2)**: ao confirmar/cancelar, o contador de presenças do ponto atualiza para
      os torcedores na tela em **≤ 2 s**; sem o canal, a tela funciona por **REST + refresh periódico**.
- [ ] Escritas com **dados inválidos** (ponto que não pertence ao evento) retornam **422/404**.
- [ ] **Admin**: criar um ponto com nome/endereço/horário válidos persiste e ele passa a aparecer na lista;
      remover um ponto o tira da lista e zera a contagem associada.

## Fora de escopo
- Mensagens/chat dentro do ponto de encontro (o **mural** é a interação ao vivo — outra spec).
- Pontos de encontro **criados por torcedores** (UGC) — nesta versão a lista é **curada pelo organizador**.
- Roteamento/navegação turn-by-turn dentro do app (apenas abre o `mapaUrl` externo).
- Cálculo de distância "até o torcedor" por GPS do dispositivo — a distância é **ponto → local do evento**,
  pré-calculada (ver Dependências).
- Notificação push de lembrete do esquenta (pode reusar `Notificacao` numa fase futura).

## Dependências (specs relacionadas / lacunas)
- **Spec irmã** `docs/specs/interacao-torcida-ao-vivo/` — reutiliza, **sem duplicar**:
  - Identidade anônima `X-Torcedor-Id` → `ITorcedorContexto` (`TorcedorContexto`).
  - `TorcidaHub` em `/hubs/torcida`, grupos `evento-{id}` (Fase 2 desta feature publica nele).
  - Padrão `ResultadoInteracao<T>` + `StatusInteracao` → tradução para HTTP no controller.
  - Gating por `Evento.Status`; aqui a janela é **`Agendado`** (e **não** `AoVivo`).
- **Local / `mapaUrl`** (`HubEsportesLages.Domain.Entities.Local`): reaproveitar a regra de `MapaUrl`
  (coordenadas → Google Maps; fallback por nome/endereço) na nova entidade de ponto de encontro.
- **Distância ponto → estádio**: calculada no servidor a partir das coordenadas do ponto e do `Local` do
  evento (Haversine). Lacuna: se o `Local` do evento não tiver lat/long, a distância fica `null` (UI omite).
- **Status do evento**: quem vira `Agendado → AoVivo`? Mesma decisão da spec irmã (organizador via Admin / job
  futuro). Até lá, a leitura do esquenta fica sempre disponível.
