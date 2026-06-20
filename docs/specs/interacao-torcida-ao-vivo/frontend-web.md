# Addendum — Front-end Web da Interação da Torcida

> A spec base (`design.md`) cobriu o app **mobile (MAUI)**, que ainda não existe. Mas o **front-end
> ativo hoje é o site Razor** (`HubEsportesLages.Web`) — é o que se testa no navegador e no celular
> (`http://172.25.76.238:5210`). O backend (API Fase 1) está pronto, porém **não há botão na UI web**.
> Este addendum planeja o botão **"Interagir com a Torcida"** e a tela de interação NO SITE,
> consumindo a API existente. **Não há mudança de backend.**

## Princípio
O site web é um **cliente fino sobre a API REST já pronta** (`/api/eventos/{slug}/torcida`, `/api/favoritos/...`).
A identidade do torcedor no navegador é um **GUID em `localStorage`** enviado no header `X-Torcedor-Id`
(equivalente ao device-id do app). Página e API estão na **mesma origem** → sem CORS. Atualização ao vivo
por **polling** (~4s) até o SignalR (Fase 2 geral) substituir.

## Onde entra (arquivos)
1. **Botão de entrada** em `Views/Agenda/Evento.cshtml` (no `aside`, junto ao CTA "Avise-me"), com estados por `Model.Status`:
   - `AoVivo` → habilitado, **"🔴 Interagir com a Torcida"** → leva a `/Torcida/{slug}`.
   - `Agendado` → desabilitado + legenda "Disponível quando o jogo começar".
   - `Encerrado` → "Ver resultados da torcida" (abre a tela em modo leitura).
   - Só renderizar para `EhConfronto` (MVP/disputa fazem sentido em jogo entre equipes).
2. (Opcional) selo **"🔴 AO VIVO"** clicável nos cards de evento ao vivo (`_CardEvento.cshtml`) levando à mesma tela.
3. **Nova tela**: `Controllers/TorcidaController.cs` + `Views/Torcida/Index.cshtml`.
4. **JS**: `wwwroot/js/torcida.js` (cliente da API). **CSS**: novas classes em `wwwroot/css/site.css`.

## Backend (web MVC)
- `TorcidaController.Index(string slug)`: usa `IEventoService.ObterPorSlugAsync` para o **cabeçalho** do evento
  (título, confronto, status, local). Renderiza o "casco" das 4 seções; os dados de votação chegam via JS.
  Se o evento não existir → 404. **Nenhum** acesso a `ITorcidaService` aqui (a tela consome a API).
- Nada de identidade nova no server: a API já resolve `X-Torcedor-Id` (após o fix do middleware).

## Front-end (Razor + JS)
- `Views/Torcida/Index.cshtml` (Model = `EventoDetalheDto`): cabeçalho + 4 cartões com IDs/data-attributes
  para o JS preencher: **Favoritar equipe**, **Jogador da Partida (MVP)**, **Enquete**, **Mural**.
- `wwwroot/js/torcida.js`:
  1. `getTorcedorId()` — lê/gera GUID em `localStorage` (`hub.torcedorId`).
  2. `carregar()` — `GET /api/eventos/{slug}/torcida` (header `X-Torcedor-Id`) → renderiza candidatos+votos+`meuVoto`,
     enquete+`%`+`minhaOpcao`, `mensagens`, `favoritado`.
  3. Ações: votar MVP (`POST .../mvp`), votar enquete (`POST .../enquete/{id}/voto`), enviar mensagem
     (`POST .../mensagens`), favoritar/desfavoritar (`POST`/`DELETE /api/favoritos/equipes/{id}`).
  4. **Trava o voto** após registrado (espelha o protótipo `voted`/`poll`).
  5. **Polling** ~4s de estado + mensagens enquanto `AoVivo`; para quando `Encerrado`.
  6. Mapear erros HTTP em avisos amigáveis: `409`→"o jogo precisa estar ao vivo", `429`→"aguarde alguns
     segundos", `400`→(não deveria ocorrer; o JS sempre manda o header).
- **CSS** (em `site.css`): `.torcida-opcao` (opção de voto), `.torcida-barra` (barra de % da enquete),
  `.torcida-mural`/`.torcida-msg` (feed). Reusar a identidade do **site** (tema claro/sporty), não a do app dark.

## API consumida (já existente — sem alteração)
`GET /api/eventos/{slug}/torcida` · `POST .../torcida/mvp` · `POST .../torcida/enquete/{id}/voto` ·
`GET|POST .../torcida/mensagens` · `POST|DELETE /api/favoritos/equipes/{equipeId}`. JSON camelCase; header `X-Torcedor-Id`.

## Tarefas
### Fase W1 — botão + tela (consome a API atual)
- [ ] Botão "Interagir com a Torcida" gated por status em `Views/Agenda/Evento.cshtml`. [designer-ui]
- [ ] `TorcidaController` + `Views/Torcida/Index.cshtml` (casco das 4 seções, cabeçalho via `IEventoService`). [dev-backend / designer-ui]
- [ ] `wwwroot/js/torcida.js`: identidade localStorage, carregar estado, votar MVP/enquete, mural, favoritar, trava de 1 voto, tratamento de 409/429. [designer-ui]
- [ ] Classes CSS da torcida em `site.css`. [designer-ui]
- [ ] Smoke no navegador e no celular (evento AoVivo já semeado: `aabb-lages-x-serra-futsal-7`). [revisor-codigo]
### Fase W2 — ao vivo de verdade
- [ ] Trocar o polling por **SignalR** quando a Fase 2 (TorcidaHub) existir (cliente JS `@microsoft/signalr`). [dev-backend]

## Decisões, trade-offs e riscos
- **Identidade por navegador (localStorage)**: simples e suficiente; some se o usuário limpar o storage.
  É o mesmo conceito do device-id do app — sem login (lacuna #3).
- **Polling vs SignalR**: polling cobre a demo agora; SignalR entra na Fase 2 (compartilhada com o mobile).
- **Reuso total do backend**: zero mudança de API; o site vira o primeiro cliente real da Fase 1 — ótimo para
  **validar a API de ponta a ponta** antes do app MAUI.
- **Mesma origem** (site + API no mesmo host) → sem CORS, sem config extra.
