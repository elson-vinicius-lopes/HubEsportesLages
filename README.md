# Bora pro Jogo (Hub Esportes Lages)

Central de **agenda, notificações e experiência do torcedor** dos esportes de Lages/SC.
Agenda com filtros, resultados, feed de notificações, **torcida ao vivo** (MVP, enquete, mural,
favoritar), **ingresso pago com QR-Code** (Pix simulado) validado pelo admin, painel administrativo,
autenticação **ASP.NET Identity** (senha forte + roles) e **API REST com JWT** para o app mobile.

Projeto do **HackathOrion** (desafio *"Melhorar a experiência do torcedor nos eventos esportivos
de Lages"*). Specs em `docs/` (Spec-Driven Development — ver `AGENTS.md`).

---

## Arquitetura

Solução **.NET 10** em *Clean Architecture* (`HubEsportesLages.slnx`):

| Projeto | Responsabilidade |
|---|---|
| `src/HubEsportesLages.Domain` | Entidades e enums (Evento, Equipe, Ingresso, votos da torcida…) |
| `src/HubEsportesLages.Application` | DTOs, interfaces de serviço, mapeamentos |
| `src/HubEsportesLages.Infrastructure` | **EF Core 10 + PostgreSQL (Npgsql)**, Identity, serviços, Migrations, seed |
| `src/HubEsportesLages.Web` | MVC (site) + API REST + Swagger + Serilog + worker de lembretes |

**Stack:** ASP.NET Core MVC · PostgreSQL 16 (WSL) · ASP.NET Identity (hash, lockout, roles) ·
JWT Bearer (API/mobile) · Serilog (console + `logs/`) · QRCoder · Razor + CSS próprio (sem build front-end).

---

## ▶️ Como executar (manual completo)

> ⚠️ **Regra do repositório:** agentes de IA **não** sobem a aplicação (ver `AGENTS.md` §6).
> Quem executa é você, seguindo este manual.

### Pré-requisitos (uma vez)
1. **.NET SDK 10** — `dotnet --version` deve mostrar `10.x`.
2. **WSL2 + Ubuntu 24.04** — `wsl --install -d Ubuntu-24.04 --no-launch` (se ainda não tiver).
3. **PostgreSQL no WSL** — o script do repo instala/configura tudo (db `hubesportes`, user `postgres`, senha `hub`):
   ```powershell
   wsl -d Ubuntu-24.04 -u root -- bash /mnt/c/Users/elson.lopes/source/repos/hubesporteslages/scripts/wsl-postgres-setup.sh
   ```

### Passo 1 — Subir o banco (a cada sessão)
O WSL **hiberna quando ocioso** e desliga o Postgres junto. Abra um terminal e **deixe este comando rodando**
(ele inicia o Postgres e mantém o WSL vivo):
```powershell
wsl -d Ubuntu-24.04 -u root -- bash -c "service postgresql start; echo POSTGRES_UP; tail -f /dev/null"
```

### Passo 2 — Subir a aplicação
Em **outro** terminal, na raiz do repositório:
```powershell
dotnet run --project src/HubEsportesLages.Web --no-launch-profile --urls http://0.0.0.0:5210
```
Na primeira execução, as **Migrations criam o schema** no Postgres e o **seed** popula o cenário
demo (modalidades, locais, equipes, agenda com jogo ao vivo, enquete, escalação).

### Passo 3 — Acessar
| O quê | Onde |
|---|---|
| Site (nesta máquina) | http://localhost:5210 |
| Site (celular na mesma Wi-Fi) | http://SEU-IP-WIFI:5210 (ex.: `ipconfig` → IPv4 do Wi-Fi) |
| API / Swagger | http://localhost:5210/swagger |
| **Login admin (dev)** | `elsouzalopes@gmail.com` / `Admin@Lages2026` |

Novos cadastros entram como **Torcedor** (senha forte obrigatória: 8+, maiúscula, minúscula, número, especial).

### API com JWT (app mobile)
```bash
curl -X POST http://localhost:5210/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"elsouzalopes@gmail.com","senha":"Admin@Lages2026"}'
# → { "token": "...", ... }  → use nas chamadas: Authorization: Bearer <token>
```

### Encerrar
`Ctrl+C` no terminal da aplicação e no do banco. Se algo ficar preso:
```powershell
Get-Process HubEsportesLages.Web -ErrorAction SilentlyContinue | Stop-Process -Force
```

---

## 🔧 Solução de problemas

| Sintoma | Causa | Correção |
|---|---|---|
| Build falha com `MSB3027/MSB3021 "file is locked"` | Instância da app ainda rodando | `Get-Process HubEsportesLages.Web \| Stop-Process -Force` e rebuild |
| `Connection refused` no Postgres ao subir a app | WSL hibernou e levou o Postgres | Rode o **Passo 1** de novo (e deixe o terminal aberto) |
| Porta 5210 ocupada | Outra instância na porta | Encerre-a (comando acima) — a porta 5210 é fixa (celular/QR) |
| Resetar o banco (re-seed) | — | `wsl -d Ubuntu-24.04 -u root -- sudo -u postgres psql -c "DROP DATABASE hubesportes;" && wsl -d Ubuntu-24.04 -u root -- sudo -u postgres createdb hubesportes` e suba a app |

### Variáveis de ambiente (produção)
Nunca commite segredos. Em produção, configure:
`ConnectionStrings__Default` · `Jwt__SecretKey` (≥32 chars) · `Ingressos__Segredo` ·
`Admin__SenhaInicial` · `Email__Provedor=Resend` + `Resend__ApiKey` (para e-mail real; padrão é log).

---

## API REST (principais endpoints)

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/auth/login` | Login → **JWT** (claims de role) |
| `GET` | `/api/eventos` · `/resultados` · `/destaques` · `/{slug}` | Agenda/resultados/detalhe |
| `GET/POST` | `/api/eventos/{slug}/torcida` (+ `/mvp`, `/enquete/{id}/voto`, `/mensagens`) | Torcida ao vivo |
| `POST/DELETE` | `/api/favoritos/equipes/{id}` | Favoritar equipe |
| `POST` | `/api/ingressos` · `/{id}/confirmar-pagamento` · `GET /meus` | Ingresso QR (Pix simulado) |
| `POST` | `/api/ingressos/validar` | **Check-in pelo admin** (uso único) |
| `GET` | `/api/catalogo/modalidades` · `/locais` · `/equipes` | Catálogo |
| `GET` | `/api/notificacoes` · `POST /api/inscricoes` | Feed / inscrição de alertas |

JSON sempre **camelCase**. Interação da torcida usa o header `X-Torcedor-Id` (GUID do dispositivo).

---

## Documentação (SDD)
- `AGENTS.md` — acordo de trabalho canônico (regras para humanos e agentes de IA).
- `docs/design-arena-lages.md` — spec do app mobile Arena Lages.
- `docs/specs/<feature>/` — requisitos/design/tarefas por feature.
- `docs/sdd-multi-ide.md` — SDD entre múltiplas IDEs de IA.
