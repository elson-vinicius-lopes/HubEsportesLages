# Guia Prático — Spec-Driven Development multi-IDE no Hub Esportes Lages

> Documento de arquitetura para o monorepo **Hub Esportes Lages** (`HubEsportesLages.slnx`): backend/web em **.NET 10** (Clean Architecture, MVC + API REST + Swagger) e o app mobile **Arena Lages** em **.NET MAUI** (a iniciar). Objetivo: garantir que **qualquer IDE de IA** (Claude Code, Google Antigravity, GitHub Copilot no VS Code, GitLab Duo) consuma a **mesma** spec e o mesmo acordo de trabalho, sem divergência.

---

## 1. Princípio central: a spec em markdown é a fonte da verdade neutra

A regra de ouro do time é simples e inegociável:

> **A especificação em markdown versionada no repositório é a fonte da verdade. As IDEs de IA são consumidoras intercambiáveis dessa spec — nenhuma ferramenta é dona do contexto.**

Consequências práticas:

- **O código serve à spec, não o contrário.** Em conflito, a spec prevalece; o código que diverge é o bug.
- **Nenhuma ferramenta vira "vendor lock-in".** Se amanhã trocarmos Copilot por Cursor, ou Antigravity por Gemini CLI, a spec e o acordo de trabalho continuam idênticos. Só troca o **adaptador fino** da ferramenta.
- **O artefato canônico é o markdown** (ex.: `docs/design-arena-lages.md`), não um arquivo proprietário de uma IDE, nem um servidor MCP, nem um "projeto" salvo em nuvem.
- **Já temos o embrião disso:** `docs/design-arena-lages.md` se declara explicitamente "fonte da verdade" do Arena Lages, e os subagentes em `.claude/` (ex.: `architect.md`) já tratam `docs/` e `figma/` como fonte da verdade. Este guia generaliza esse padrão para todas as IDEs.

---

## 2. Arquitetura de portabilidade em 3 camadas

```
┌──────────────────────────────────────────────────────────────────────┐
│ CAMADA A — SPECS (a verdade)                                           │
│   docs/specs/<feature>/{requisitos,design,tarefas}.md                  │
│   docs/design-arena-lages.md  (spec do app, já existente)              │
│   docs/desafio.md             (contexto do produto)                    │
└──────────────────────────────────────────────────────────────────────┘
            ▲ apontada por ▲                ▲ apontada por ▲
┌──────────────────────────────────────────────────────────────────────┐
│ CAMADA B — REGRAS (o acordo de trabalho)                               │
│   AGENTS.md   ◄── ÚNICO arquivo canônico (raiz)                        │
│      ▲            ▲                ▲                  ▲                  │
│   CLAUDE.md   .github/         .agents/rules/     .gitlab/duo/          │
│   (@import)   copilot-          arena-lages.md    chat-rules.md         │
│               instructions.md   (Antigravity)     (GitLab Duo)         │
│               (Copilot/VS Code)                                        │
│   → adaptadores FINOS que só apontam para AGENTS.md + docs/            │
└──────────────────────────────────────────────────────────────────────┘
            ▲ ferramentas compartilham ▲
┌──────────────────────────────────────────────────────────────────────┐
│ CAMADA C — MCP (ferramentas executáveis, opcional, por-ferramenta)     │
│   .vscode/mcp.json (Copilot)  •  .gitlab/duo/mcp.json (Duo)            │
│   .mcp.json (Claude)  •  ~/.gemini/.../mcp_config.json (Antigravity)   │
│   → NÃO é um arquivo único; cada IDE tem sua config. Conteúdo igual.   │
└──────────────────────────────────────────────────────────────────────┘
```

### Camada A — Specs em `docs/` (estrutura sugerida)

A spec é a verdade. Estrutura proposta, alinhada ao vocabulário de Spec-Driven Development (requisitos → design → tarefas):

```
docs/
├── desafio.md                          # contexto do produto (já existe)
├── design-arena-lages.md               # spec do app mobile (já existe — fonte da verdade)
└── specs/
    ├── _template/                      # modelo para copiar ao abrir uma feature
    │   ├── requisitos.md
    │   ├── design.md
    │   └── tarefas.md
    ├── checkin-qrcode/                 # exemplo: uma feature do Arena Lages
    │   ├── requisitos.md               # o "o quê / porquê" (user stories, critérios de aceite)
    │   ├── design.md                   # o "como" (camadas .NET / telas MAUI, contrato de API)
    │   └── tarefas.md                  # quebra acionável + checklist de done
    └── notificacoes-push/
        ├── requisitos.md
        ├── design.md
        └── tarefas.md
```

Convenção: **uma pasta por feature**, sempre os três arquivos. `requisitos.md` é independente de stack; `design.md` referencia as camadas concretas (`HubEsportesLages.Domain/Application/Infrastructure/Web` e o futuro `HubEsportesLages.Mobile`); `tarefas.md` é a lista que qualquer IDE executa.

### Camada B — UM arquivo canônico (`AGENTS.md`) + adaptadores finos

O risco número um do multi-IDE é **divergência de regras**: cada ferramenta lê um arquivo diferente, e os arquivos saem de sincronia. A defesa é ter **um único arquivo de regras de verdade** — `AGENTS.md` na raiz — e fazer todos os outros serem **ponteiros finos** (3 a 10 linhas) que não duplicam conteúdo:

| Adaptador fino | Para qual IDE | Estratégia |
|---|---|---|
| `CLAUDE.md` | Claude Code | `@import` do `AGENTS.md` (Claude não lê `AGENTS.md` nativamente hoje) |
| `.github/copilot-instructions.md` | GitHub Copilot / VS Code | texto curto: "leia `AGENTS.md` e a spec em `docs/`" |
| `.agents/rules/arena-lages.md` | Google Antigravity | regra curta apontando para `AGENTS.md` + `docs/` |
| `.gitlab/duo/chat-rules.md` | GitLab Duo | regra curta apontando para `AGENTS.md` + `docs/` |

`AGENTS.md` é um formato **aberto, Markdown puro, editor-agnóstico** (doado à Agentic AI Foundation sob a Linux Foundation em dez/2025). É lido **nativamente** por Copilot/VS Code, Antigravity e GitLab Duo — e por Claude Code via `@import`. Por isso é o denominador comum perfeito.

> **Regra de manutenção:** mudou o acordo de trabalho? Edite **só** o `AGENTS.md`. Os adaptadores nunca repetem regra — só apontam.

### Camada C — MCP como camada de ferramentas compartilhada

MCP (Model Context Protocol) dá às IDEs **ferramentas executáveis** (consultar issues, rodar queries, ler docs externas). Fato importante: **MCP não é configurado pelo `AGENTS.md`** — cada IDE tem seu próprio arquivo de config. Você não tem um arquivo MCP único; você tem **o mesmo conjunto de servidores** declarado em formatos por-ferramenta:

| IDE | Arquivo de config MCP | Chave de topo | Escopo |
|---|---|---|---|
| GitHub Copilot / VS Code | `.vscode/mcp.json` | `servers` | workspace (committável) |
| GitLab Duo | `.gitlab/duo/mcp.json` | `mcpServers` | workspace (committável) |
| Claude Code | `.mcp.json` | `mcpServers` | workspace (committável) |
| Google Antigravity | `~/.gemini/.../mcp_config.json` | `mcpServers` | **global por máquina** (não committável) |

Recomendação: adote MCP **só quando trouxer valor real** (ex.: um MCP server da própria GitLab/GitHub para issues). Mantenha os servidores equivalentes entre `.vscode/mcp.json`, `.gitlab/duo/mcp.json` e `.mcp.json`; para Antigravity, documente no `AGENTS.md` quais servidores configurar via UI ("View raw config"). **Atenção:** Antigravity não suporta OAuth no MCP, e o suporte pode estar limitado ao IDE (não à API/agente) — tratar como instável.

---

## 3. Tabela de referência — quem lê o quê, MCP e como consome a spec

| Ferramenta | Arquivo(s) de regras que lê | MCP? | Config MCP | Como consome a spec |
|---|---|---|---|---|
| **Claude Code** (já em uso) | `CLAUDE.md` nativo; `AGENTS.md` **só via** `@import` dentro do `CLAUDE.md` (não há leitura nativa — issue aberta #34235) | Sim | `.mcp.json` (`mcpServers`) | `CLAUDE.md` faz `@AGENTS.md` e `@docs/design-arena-lages.md`; subagentes em `.claude/agents/` já tratam `docs/`+`figma/` como fonte da verdade |
| **GitHub Copilot / VS Code** | `AGENTS.md` (raiz + aninhados; requer setting `chat.useAgentsMdFile`), `.github/copilot-instructions.md` (sempre ativo), `.github/instructions/*.instructions.md` (com `applyTo`). Também reconhece `CLAUDE.md`. | Sim (GA) | `.vscode/mcp.json` (chave **`servers`**) | `copilot-instructions.md` aponta para `AGENTS.md` + `docs/` via link relativo (`../docs/...`); `.instructions.md` com `applyTo` separando backend `**/*.cs` e MAUI `src/HubEsportesLages.Mobile/**` |
| **Google Antigravity** | `AGENTS.md` na raiz do **workspace aberto** (suporte desde v1.20.3, 2026-03-05), `.agents/rules/*.md` (≤ ~12k chars/arquivo), global `~/.gemini/GEMINI.md`. Em conflito, `GEMINI.md` específico sobrepõe `AGENTS.md`. | Sim (no IDE) | `~/.gemini/.../mcp_config.json` (global; **sem OAuth**) — *caminho exato varia por versão* | `AGENTS.md` (raiz) + uma regra curta em `.agents/rules/` apontam para `docs/design-arena-lages.md`. Em monorepo, lê o `AGENTS.md` da pasta aberta — útil ter um por subprojeto. |
| **GitLab Duo** | `.gitlab/duo/chat-rules.md` (regras de chat/agente), `.gitlab/duo/mr-review-instructions.yaml` (review de MR, com `fileFilters`). *Também lê `AGENTS.md` nativamente conforme docs da GitLab — confirmar na versão da instância.* | Sim (cliente **e** servidor) | `.gitlab/duo/mcp.json` (`mcpServers`) | `chat-rules.md` aponta para `AGENTS.md` + `docs/`; review por MR usa `fileFilters` por área (backend `**/*.cs`, MAUI) validando contra a spec |

**Incertezas declaradas (não inventar certeza):**
- **Precedência exata** entre `AGENTS.md` × `copilot-instructions.md` × `CLAUDE.md` no Copilot **não é documentada** pela Microsoft — trate-os como **cumulativos/somados**.
- **Antigravity + MCP**: suportado no IDE; **possivelmente não na API/agente**. Caminho do `mcp_config.json` diverge entre fontes (`~/.gemini/antigravity/` vs `~/.gemini/config/`) — confirmar via UI "View raw config".
- **Claude Code + `AGENTS.md` nativo**: **não suportado** ainda; usar `@import`.
- **GitLab Duo + `AGENTS.md`**: a doc da GitLab cita `AGENTS.md`, mas o mecanismo nativo estável e documentado é `.gitlab/duo/chat-rules.md` — por isso o adaptador abaixo é em `chat-rules.md` (e ele aponta para `AGENTS.md`, então funciona nos dois casos).

---

## 4. Vale adotar o GitHub Spec Kit (`specify`) neste repo?

**Recomendação: NÃO adotar agora; adotar de forma seletiva no futuro, se a equipe sentir falta de orquestração de slash-commands.** Já temos os dois pilares do SDD — specs em `docs/` + Claude Code com `.claude/commands` e `.claude/agents` — funcionando.

**O que é:** toolkit open-source da GitHub (`github/spec-kit`) com o CLI Python `specify`, que faz scaffolding e instala slash-commands (`/speckit.specify`, `/speckit.plan`, `/speckit.tasks`, `/speckit.implement`...) por agente.

**Prós para este repo:**
- Padroniza o fluxo requisitos → plano → tarefas → implementação com comandos prontos.
- Suporta oficialmente **Claude Code** (`claude`), **GitHub Copilot** (`copilot`) e **Gemini CLI** (`gemini`) — `specify init . --integration <agente>` instala os comandos lado a lado, todos lendo os **mesmos** markdowns de spec.
- A "constituição" (`.specify/memory/constitution.md`) é um bom lugar para princípios não-negociáveis (mas nós já fazemos isso no `AGENTS.md`).

**Contras para este repo (decisivos):**
- **Layout opinativo e conflitante:** o Spec Kit espera specs em `specs/<NNN-feature>/spec.md` e constituição em `.specify/memory/`. **Não há flag oficial estável** para apontar a raiz para `docs/`. Já investimos em `docs/` como fonte da verdade — adotar o Spec Kit forçaria a duplicar/migrar ou conviver com dois locais de spec.
- **Sem suporte oficial a Google Antigravity nem GitLab Duo** — dois dos nossos alvos. Sobraria a integração `generic`, que você adapta à mão (ou seja, o trabalho manual que este guia já resolve com `AGENTS.md`).
- **MCP nativo: não existe** (issue #99 fechada como "not planned"). Só servidores comunitários, instáveis.
- **Duplicação de comandos por agente:** cada integração recebe sua própria cópia dos comandos. Já temos `.claude/commands` desenhados sob medida (`rodar`, `testar-api`, `nova-modalidade`, `scaffold-mobile`).
- Prefixos de comando e lista de integrations **mudam por versão** — custo de manutenção.

**Veredito:** o `AGENTS.md` + `docs/specs/` deste guia entrega o objetivo central (uma spec, várias IDEs) **sem** o acoplamento de layout do Spec Kit e cobrindo Antigravity e GitLab Duo. Se no futuro a equipe quiser os slash-commands prontos, dá para adicionar `specify` **apenas para Claude/Copilot/Gemini**, fazendo o `spec.md` gerado **referenciar** (link relativo) os arquivos em `docs/` — sem migrar a fonte da verdade.

---

## 5. Conteúdo PRONTO PARA COMMITAR

### `AGENTS.md`

```markdown
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
Pré-requisito: **.NET SDK 10**. Use o `nuget.config` da raiz.

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
```

### `CLAUDE.md`

```markdown
# CLAUDE.md

As regras de trabalho deste repositório são canônicas no AGENTS.md. Importe-as:

@AGENTS.md

Fonte da verdade do app mobile Arena Lages:

@docs/design-arena-lages.md

Não duplique regras aqui. Para mudar o acordo de trabalho, edite o AGENTS.md.
Comandos e subagentes específicos do Claude Code ficam em `.claude/commands/` e `.claude/agents/`.
```

### `.github/copilot-instructions.md`

```markdown
# Instruções do Copilot — Hub Esportes Lages

O acordo de trabalho canônico está em [`AGENTS.md`](../AGENTS.md) na raiz.
**Leia e siga o AGENTS.md** — ele descreve o princípio de Spec-Driven Development,
a arquitetura (.NET 10 Clean Architecture + app MAUI Arena Lages), os comandos de
build/run via `HubEsportesLages.slnx` e onde ficam as specs.

A fonte da verdade muda conforme a área:
- App mobile Arena Lages: [`docs/design-arena-lages.md`](../docs/design-arena-lages.md).
- Features: `docs/specs/<feature>/{requisitos,design,tarefas}.md`.

Em conflito entre código e spec, **a spec prevalece**.

Dica: habilite a leitura do AGENTS.md em Settings → `chat.useAgentsMdFile`.
Para regras por área, veja `.github/instructions/*.instructions.md`.
```

### `.github/instructions/backend.instructions.md`

```markdown
---
applyTo: "src/HubEsportesLages.Domain/**,src/HubEsportesLages.Application/**,src/HubEsportesLages.Infrastructure/**,src/HubEsportesLages.Web/**"
---
Backend .NET 10 (Clean Architecture). Respeite a direção de dependência
Domain ← Application ← Infrastructure ← Web. Use EF Core com `AsNoTracking`/`Include`
e registre serviços no DI. Regras completas em [`AGENTS.md`](../../AGENTS.md).
```

### `.github/instructions/mobile.instructions.md`

```markdown
---
applyTo: "src/HubEsportesLages.Mobile/**"
---
App mobile Arena Lages (.NET MAUI, MVVM, tema dark-only). Consuma a API REST do hub
(JSON camelCase) — não acesse o banco. Siga a spec visual/de telas em
[`docs/design-arena-lages.md`](../../docs/design-arena-lages.md) e o
[`AGENTS.md`](../../AGENTS.md).
```

### `.agents/rules/hub-esportes-lages.md`  *(Google Antigravity)*

```markdown
# Regras — Hub Esportes Lages (Antigravity)

O acordo de trabalho canônico está em `AGENTS.md` na raiz do workspace — leia e siga-o.
Ele cobre o princípio de Spec-Driven Development, a arquitetura (.NET 10 Clean Architecture
+ app MAUI Arena Lages), os comandos de build/run via `HubEsportesLages.slnx` e a localização
das specs em `docs/`.

Fonte da verdade do app mobile: `docs/design-arena-lages.md`.
Specs por feature: `docs/specs/<feature>/{requisitos,design,tarefas}.md`.

Em conflito entre código e spec, a spec prevalece. Mantenha esta regra curta;
não duplique o conteúdo do AGENTS.md aqui.
```

> Nota Antigravity: ele lê o `AGENTS.md` da **raiz do workspace aberto**. Se você abrir só a pasta do `src/HubEsportesLages.Mobile`, crie um `AGENTS.md` ali que aponte para o da raiz (`../../AGENTS.md`). MCP é configurado fora do repo, na UI ("View raw config").

### `.gitlab/duo/chat-rules.md`  *(GitLab Duo)*

```markdown
# Regras do GitLab Duo — Hub Esportes Lages

O acordo de trabalho canônico está em `AGENTS.md` na raiz do repositório — leia e siga-o.
Cobre o princípio de Spec-Driven Development, a arquitetura (.NET 10 Clean Architecture +
app MAUI Arena Lages), os comandos de build/run via `HubEsportesLages.slnx` e a localização
das specs em `docs/`.

- Fonte da verdade do app mobile: `docs/design-arena-lages.md`.
- Specs por feature: `docs/specs/<feature>/{requisitos,design,tarefas}.md`.
- Ao revisar ou implementar, valide o código contra os critérios de aceite da spec.

Em conflito entre código e spec, a spec prevalece.
```

### `docs/specs/_template/requisitos.md`

```markdown
# Requisitos — <Feature>

> O "o quê" e o "porquê". Independente de stack. Esta seção é a fonte da verdade da feature.

## Contexto
Por que esta feature existe? Qual problema do torcedor/organizador ela resolve?

## User stories
- Como **<persona>**, quero **<ação>**, para **<benefício>**.

## Critérios de aceite (testáveis)
- [ ] Dado <contexto>, quando <ação>, então <resultado observável>.
- [ ] ...

## Fora de escopo
- ...
```

### `docs/specs/_template/design.md`

```markdown
# Design — <Feature>

> O "como". Referencie camadas e contratos concretos do repo.

## Backend (.NET 10)
- Domain: entidades/enums afetados.
- Application: DTOs, interfaces de serviço, mapeamentos.
- Infrastructure: EF Core / seed / DI.
- Web: controllers MVC/API, endpoints, Swagger.

## API (contrato camelCase)
- Método/rota, request, response.

## Mobile (Arena Lages, MAUI)
- Telas/ViewModels/serviços. Tokens e identidade conforme `docs/design-arena-lages.md`.

## Decisões e trade-offs / riscos
- ...
```

### `docs/specs/_template/tarefas.md`

```markdown
# Tarefas — <Feature>

> Quebra acionável. Marque ao concluir. "Done" = critérios de aceite de `requisitos.md` atendidos.

- [ ] T1 — <descrição> (camada/arquivo)
- [ ] T2 — ...
- [ ] Testes cobrindo os critérios de aceite
- [ ] Spec e código revisados (não divergem)
```

---

## 6. Passo a passo de adoção e validação em CI

### Adoção no repositório (ordem sugerida)

1. **Crie a Camada B.** Commit dos arquivos da seção 5: `AGENTS.md`, `CLAUDE.md`, `.github/copilot-instructions.md` (+ `.github/instructions/*`), `.agents/rules/hub-esportes-lages.md`, `.gitlab/duo/chat-rules.md`.
2. **Crie a Camada A.** Pasta `docs/specs/_template/` com os três arquivos; mantenha `docs/design-arena-lages.md` e `docs/desafio.md`.
3. **Ative o Copilot/VS Code:** em Settings, ligue `chat.useAgentsMdFile` (e, para monorepo, `chat.useNestedAgentsMdFiles` se quiser `AGENTS.md` por subprojeto).
4. **Valide cada IDE com um teste de fumaça:** abra a ferramenta e pergunte *"Qual é a fonte da verdade deste projeto e como rodo o site?"*. A resposta deve citar `docs/` e `dotnet run --project src/HubEsportesLages.Web`. Se não citar, o adaptador não está sendo lido.
5. **(Opcional) Camada C / MCP:** só se houver um MCP server útil (issues GitLab/GitHub). Adicione `.vscode/mcp.json` e `.gitlab/duo/mcp.json` com servidores equivalentes; documente o Antigravity no `AGENTS.md`.
6. **Atualize `.gitignore`** se adotar MCP com segredos: nunca commitar tokens (use variáveis de ambiente nos campos `env`).

### Validar em CI que o código segue a spec

Não existe gate nativo "código vs spec.md" em nenhuma das ferramentas. A validação é um **job de CI custom** com três checagens objetivas e determinísticas:

**a) Estrutura das specs (toda feature tem os 3 arquivos):**

```bash
# scripts/check-specs.sh
set -euo pipefail
fail=0
for dir in docs/specs/*/; do
  [ "$dir" = "docs/specs/_template/" ] && continue
  for f in requisitos.md design.md tarefas.md; do
    if [ ! -f "$dir$f" ]; then echo "FALTA: $dir$f"; fail=1; fi
  done
done
exit $fail
```

**b) Os adaptadores não divergiram** (todos ainda apontam para `AGENTS.md`/`docs/`) e o build da solução passa:

```bash
test -f AGENTS.md
grep -q "@AGENTS.md" CLAUDE.md
grep -q "AGENTS.md" .github/copilot-instructions.md
dotnet build HubEsportesLages.slnx -warnaserror
```

**c) O contrato testável da spec é coberto por testes** — a regra cultural: todo critério de aceite em `requisitos.md` vira um teste. O CI roda `dotnet test HubEsportesLages.slnx` (quando houver projeto de testes) e falha se cair abaixo do esperado.

**GitHub Actions — `.github/workflows/spec-ci.yml`:**

```yaml
name: spec-ci
on: [push, pull_request]
jobs:
  build-and-specs:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '10.0.x' }
      - name: Validar estrutura das specs
        run: bash scripts/check-specs.sh
      - name: Validar adaptadores de regras
        run: |
          test -f AGENTS.md
          grep -q "@AGENTS.md" CLAUDE.md
          grep -q "AGENTS.md" .github/copilot-instructions.md
      - name: Build da solução
        run: dotnet build HubEsportesLages.slnx -warnaserror
      # - name: Testes (quando houver projeto de testes)
      #   run: dotnet test HubEsportesLages.slnx
```

**GitLab CI — `.gitlab-ci.yml`** (equivalente, e o **Duo Code Review** já valida MRs contra a spec via `chat-rules.md`/`mr-review-instructions.yaml`):

```yaml
spec-ci:
  image: mcr.microsoft.com/dotnet/sdk:10.0
  script:
    - bash scripts/check-specs.sh
    - test -f AGENTS.md && grep -q "@AGENTS.md" CLAUDE.md
    - dotnet build HubEsportesLages.slnx -warnaserror
    # - dotnet test HubEsportesLages.slnx
```

> A validação "semântica" (o código faz o que a spec pede) continua sendo trabalho de **revisão** — humana e assistida por IA. O CI garante o que é determinístico: specs bem-formadas, adaptadores sincronizados, build verde e testes dos critérios de aceite passando.

---

### Resumo de uma frase

Uma spec em `docs/` como verdade, um `AGENTS.md` como acordo de trabalho, adaptadores finos por IDE que só apontam para eles, MCP opcional por ferramenta — e um job de CI que recusa specs malformadas, adaptadores divergentes e builds quebrados. Assim Claude Code, Copilot, Antigravity e GitLab Duo trabalham sobre o mesmo contexto, sem vendor lock-in.

---

Arquivos a commitar (caminhos absolutos neste repo):
- `C:\Users\elson.lopes\source\repos\hubesporteslages\AGENTS.md`
- `C:\Users\elson.lopes\source\repos\hubesporteslages\CLAUDE.md`
- `C:\Users\elson.lopes\source\repos\hubesporteslages\.github\copilot-instructions.md`
- `C:\Users\elson.lopes\source\repos\hubesporteslages\.github\instructions\backend.instructions.md`
- `C:\Users\elson.lopes\source\repos\hubesporteslages\.github\instructions\mobile.instructions.md`
- `C:\Users\elson.lopes\source\repos\hubesporteslages\.agents\rules\hub-esportes-lages.md`
- `C:\Users\elson.lopes\source\repos\hubesporteslages\.gitlab\duo\chat-rules.md`
- `C:\Users\elson.lopes\source\repos\hubesporteslages\docs\specs\_template\{requisitos,design,tarefas}.md`
- `C:\Users\elson.lopes\source\repos\hubesporteslages\scripts\check-specs.sh`
- `C:\Users\elson.lopes\source\repos\hubesporteslages\.github\workflows\spec-ci.yml` e/ou `.gitlab-ci.yml`

Fontes já existentes referenciadas: `docs\design-arena-lages.md` (spec do Arena Lages, declarada fonte da verdade), `docs\desafio.md`, `HubEsportesLages.slnx`, `.claude\agents\architect.md` e `.claude\commands\scaffold-mobile.md`.
