# Requisitos — Foto com Frame para Redes Sociais

> O "o quê" e o "porquê". Independente de stack. Fonte da verdade da feature.
> Spec visual: `docs/design-arena-lages.md` (tokens dark-only, `Accent` lima `#C2EF4E`).
> **Estende** — não duplica — a base `docs/specs/interacao-torcida-ao-vivo/` (identidade
> `X-Torcedor-Id`, gating por `Evento.Status`, contrato camelCase, `TorcidaHub`).

## Contexto
O torcedor quer **mostrar que está no jogo**. Hoje, para postar nas redes, ele tira uma foto
qualquer — sem identidade visual da Arena Lages, do evento ou da sua equipe. Esta feature dá ao
torcedor uma forma de **compor uma imagem personalizada** com a cara do app: pega/captura uma foto,
aplica uma **moldura (frame)** com a marca Arena Lages + logo, e pode sobrepor **nome do time,
placar e uma frase** (ex.: "Eu tô no jogo"). Depois **baixa ou compartilha** direto nas redes.

Isso transforma cada torcedor em divulgador orgânico do evento e da comunidade esportiva de Lages,
ampliando o alcance sem custo de mídia.

A composição é **predominantemente client-side** (no app MAUI): captura, montagem e exportação
acontecem no dispositivo. O **backend é mínimo**: serve um **catálogo de frames** (templates) por
evento/equipe e dados de pré-preenchimento (título, placar, equipes). Opcionalmente registra uma
**métrica de compartilhamento** para o organizador.

Diferente da base (interação só faz sentido **durante** o jogo), a foto-frame é útil em uma
**janela maior**: antes (hype), durante ("Eu tô no jogo") e depois (frame com placar final). O
gating, portanto, **não** é `AoVivo`-only — ver Critérios de aceite.

## Personas
- **Torcedor** — compõe e compartilha a imagem; não precisa estar logado (identidade anônima por dispositivo).
- **Organizador / Fundação** — (futuro) cadastra frames por evento/equipe e acompanha métricas de compartilhamento.

## User stories
- Como **torcedor**, quero **escolher/capturar uma foto** e aplicar uma **moldura da Arena Lages**, para postar com a identidade do app.
- Como **torcedor**, quero **escolher um frame do evento/da minha equipe**, para a imagem ficar contextual ao jogo.
- Como **torcedor**, quero **adicionar nome do time, placar e uma frase** (ex.: "Eu tô no jogo"), para personalizar.
- Como **torcedor**, quero **baixar** a imagem na galeria **e compartilhar** pelo seletor nativo, para postar onde eu quiser.
- Como **torcedor sem foto à mão**, quero gerar um **card só com o frame + dados do evento**, sem precisar de foto.
- Como **organizador**, quero (futuro) **cadastrar frames** por evento/equipe e **ver quantos compartilhamentos** rolaram.

## Critérios de aceite (testáveis)
- [ ] O botão **"Foto com frame"** aparece no detalhe do evento e está **habilitado em qualquer
      status exceto `Cancelado` (4)**. Em `Cancelado`, fica oculto/desabilitado.
- [ ] O **catálogo de frames** é carregado por evento (`GET .../frames`); sempre há **pelo menos
      o frame padrão da Arena Lages** (fallback embutido no app se a API falhar/offline).
- [ ] Dado um evento, quando abro a tela, então os campos **nome do time / placar / frase** vêm
      **pré-preenchidos** com os dados do evento (equipes, `placar` se houver) e a frase default "Eu tô no jogo".
- [ ] Posso **trocar a foto** (galeria via `MediaPicker.PickPhotoAsync` ou câmera via `CapturePhotoAsync`)
      e a **pré-visualização** atualiza ao vivo (foto + frame + textos sobrepostos).
- [ ] Posso **gerar sem foto**: a composição usa um fundo do frame e os dados do evento.
- [ ] **Baixar** salva a imagem (PNG/JPEG) na galeria/arquivos do dispositivo; **Compartilhar** abre
      o `Share` nativo com o arquivo. A imagem exportada tem resolução adequada para redes (≥ 1080×1080).
- [ ] A imagem composta **nunca é enviada ao backend** (composição 100% local); o backend só serve frames
      e (opcional) recebe um **ping de métrica** sem a imagem.
- [ ] **Offline / API indisponível**: a tela ainda funciona com o **frame padrão embutido** e dados já
      carregados; o ping de métrica é best-effort (falha silenciosa, não bloqueia o compartilhamento).
- [ ] O **placar** só é oferecido como sobreposição quando o evento **tem placar** (`Status` `AoVivo`/`Encerrado`
      com `PlacarCasa`/`PlacarVisitante`); em `Agendado` o campo placar fica oculto/vazio.
- [ ] (Opcional) O **ping de métrica** (`POST .../frames/compartilhamentos`) é aceito em **qualquer status
      exceto `Cancelado`** e é **idempotente-tolerante** (não há voto único; conta eventos).

## Fora de escopo
- Editor avançado de imagem (stickers livres, recorte manual, filtros, brush, multi-camadas arrastáveis).
- Upload/armazenamento da imagem final no servidor ou galeria pública no app.
- Vídeo/boomerang/stories animados.
- Moderação da foto do torcedor (a imagem não trafega pelo backend; nada a moderar server-side).
- Cadastro de frames pelo organizador via UI rica (Fase futura; MVP usa seed).

## Dependências (specs relacionadas / lacunas)
- **Base `interacao-torcida-ao-vivo`**: reutiliza identidade `X-Torcedor-Id` → `ITorcedorContexto`,
  o contrato `/api/eventos/{slug}/...` camelCase e o padrão `ResultadoInteracao`/`StatusInteracao`.
  **Não** reusa votos/mural/enquete — esta feature não escreve interação ao vivo.
- **Dados do evento**: título, equipes (nome/sigla/escudo/cor) e placar vêm do `EventoDetalheDto` já existente.
- **Assets de marca**: logo Arena Lages e frame padrão precisam existir como asset embutido no app MAUI
  (`Resources/Images`) — **lacuna**: confirmar o arquivo de logo/PNG do frame com design.
- **Tempo real (opcional)**: se o placar mudar enquanto a tela está aberta, atualizar o pré-preenchimento
  via `TorcidaHub` (grupo `evento-{id}`), reusando o hub da base. Não é requisito do MVP.
- **Frames por equipe**: depende de `Equipe.CorPrimaria`/`Escudo` (já existem) para gerar frames temáticos.
