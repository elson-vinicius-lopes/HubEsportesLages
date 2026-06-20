# AGENTS.md — Hub Esportes Lages (acordo de trabalho canônico)

> Este é o **único** arquivo de regras de verdade do repositório. `CLAUDE.md`,
> `.github/copilot-instructions.md`, `.agents/rules/*` e `.gitlab/duo/chat-rules.md`
> são adaptadores finos que apontam para cá. **Mudou uma regra? Edite só este arquivo.**

## 0. Princípio: Spec-Driven Development (SDD)
A **fonte da verdade é a especificação em markdown versionada em `docs/`**, não o código,
nem qualquer ferramenta de IA. As IDEs de IA (Claude Code, GitHub Copilot, Google
Antigravity, GitLab Duo) são **consumidoras intercambiáveis** da mesma spec.

- Antes de implementar ou revisar, **leia a spec da feature** e siga-a.
- Em conflito entre código e spec, **a spec prevalece** — o código divergente é o bug.
- Mudança de comportamento começa pela spec: atualize o markdown, depois o código.

## 1. Onde estão as specs
- `docs/design-arena-lages.md` — **fonte da verdade** do app mobile Arena Lages
  (telas, design tokens, navegação, contrato com a API). LEIA antes de mexer no mobile.
- `docs/desafio.md` — contexto do produto (o problema que resolvemos).
- `docs/sdd-multi-ide.md` — como o SDD se integra entre as IDEs (este acordo em detalhe).
- `docs/specs/<feature>/{requisitos,design,tarefas}.md` — specs por feature:
  - `requisitos.md` — o quê/porquê (user stories, critérios de aceite), independente de stack.
  - `design.md` — o como (camadas .NET / telas MAUI, contrato de API).
  - `tarefas.md` — quebra acionável + checklist de "done".
- Para abrir uma feature nova: copie `docs/specs/_template/` para `docs/specs/<feature>/`.

## 2. Arquitetura do sistema
Monorepo, solução **`HubEsportesLages.slnx`** (.NET 10).

### Web — Hub Esportes Lages (Clean Architecture, 4 projetos)
- `src/HubEsportesLages.Domain` — entidades e enums, sem dependências
  (`Evento`, `Modalidade`, `Equipe`, `Local`, `Inscricao`, `Notificacao`).
- `src/HubEsportesLages.Application` — DTOs, interfaces de serviço, mapeamentos
  (`IEventoService`, `ICatalogoService`, `IInscricaoService`, `INotificacaoService`).
- `src/HubEsportesLages.Infrastructure` — EF Core 10 + SQLite, implementação dos
  serviços, `DataSeeder`, injeção de dependência.
- `src/HubEsportesLages.Web` — MVC (site) + API REST + Swagger + `NotificacaoLembreteWorker`.

Regra de dependência: Domain ← Application ← Infrastructure ← Web (nunca o inverso).
EF Core: usar `AsNoTracking`/`Include` conforme já estabelecido; registrar serviços no DI.

### Mobile — Arena Lages (.NET MAUI, MVVM) — a iniciar
- Projeto-alvo: `src/HubEsportesLages.Mobile` (Android + iOS, phone-first ~390px, **tema dark-only**).
- **Consome a API REST** do hub (JSON **camelCase**) — não acessa o banco diretamente.
- Spec visual e de telas: `docs/design-arena-lages.md` (fonte da verdade).
- Stack: `CommunityToolkit.Mvvm`, `HttpClient` tipado + `System.Text.Json`, Shell para navegação.

### Contrato da API (alimenta o mobile)
- `GET /api/eventos` (filtros: `Modalidade`, `LocalId`, `EquipeId`, `Busca`, `Periodo`,
  `ApenasGratuitos`, `Pagina`, `TamanhoPagina`) → lista paginada (`EventoResumoDto`).
- `GET /api/eventos/resultados` | `/destaques` | `/{slug}` → `EventoDetalheDto`.
- `GET /api/catalogo/modalidades` | `/locais` | `/equipes`.
- `GET /api/notificacoes`, `POST /api/notificacoes/gerar-lembretes`.
- `POST /api/inscricoes` (`CriarInscricaoDto`). Toda a API responde em **camelCase**.

## 3. Comandos de build / run (via HubEsportesLages.slnx)
Pré-requisito: **.NET SDK 10**. Use o `nuget.config` da raiz (isola o feed privado da máquina).

```bash
dotnet build HubEsportesLages.slnx                 # compila a solução inteira
dotnet run --project src/HubEsportesLages.Web      # sobe site + API + Swagger
dotnet run --project src/HubEsportesLages.Web --urls http://localhost:5210   # porta fixa
```
- Site na URL do console (ex.: `http://localhost:5210`); API/Swagger em `/swagger`.
- Banco SQLite (`hubesportes.db`) é criado e populado pelo seed na 1ª execução.
  Apague o `.db` para resetar. **Nunca** commitar `*.db`, `bin/`, `obj/` (ver `.gitignore`).

Mobile (após o scaffold):
```bash
dotnet build src/HubEsportesLages.Mobile
```
`BaseUrl` da API: `10.0.2.2:5210` no emulador Android, `localhost:5210` no Windows.

## 4. Convenções
- Código, identificadores de domínio e comentários em **português** (segue o existente).
- Respeite os padrões já estabelecidos; não introduza dependências de build de front-end no web.
- API sempre **camelCase**; os DTOs do mobile devem espelhar exatamente esse JSON.
- Ao terminar uma tarefa, marque o item correspondente em `docs/specs/<feature>/tarefas.md`.

## 5. MCP (configurado por ferramenta — não por este arquivo)
MCP é opcional e configurado em cada IDE separadamente, com servidores equivalentes:
`.vscode/mcp.json` (Copilot, chave `servers`), `.gitlab/duo/mcp.json` (Duo, `mcpServers`),
`.mcp.json` (Claude, `mcpServers`), e config global do Antigravity via UI ("View raw config").
Adote MCP só quando houver um servidor que traga valor real (ex.: issues do GitHub/GitLab).
Nunca commite tokens — use variáveis de ambiente nos campos `env`.
