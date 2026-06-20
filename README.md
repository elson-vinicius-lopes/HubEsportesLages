# Hub Esportes Lages

Hub central de **agenda e notificações dos esportes de Lages/SC**. Reúne em um só lugar os jogos,
corridas e competições de todas as modalidades da cidade, com **resultados**, **feed de
notificações** e **inscrição** do torcedor para receber alertas da sua equipe.

Projeto desenvolvido para o **HackathOrion** (desafio *“Melhorar a experiência do torcedor nos
eventos esportivos de Lages”*), inspirado no padrão de portais como
[eventooficial.com.br](https://www.eventooficial.com.br/eventos) e
[esportecuritibanos.sc.gov.br](https://esportecuritibanos.sc.gov.br/).

---

## Funcionalidades

- **Agenda esportiva** com filtros por modalidade, local, período (hoje / semana / mês) e busca textual, além de paginação.
- **Página de evento** com confronto, placar, local + link para o Google Maps, ingresso e descrição.
- **Resultados** dos eventos já encerrados (placares e súmulas).
- **Feed central de notificações** — novos eventos, lembretes, alterações e resultados.
- **Inscrição do torcedor** para receber alertas (geral, por modalidade ou por equipe).
- **Área do organizador** para publicar novos eventos (dispara notificação automática).
- **Lembretes automáticos** gerados em segundo plano para eventos que começam nas próximas 24h.
- **API REST** documentada com **Swagger** (`/swagger`).
- **Jogos ao vivo** com destaque visual (status *AO VIVO* pulsante).

---

## Arquitetura

Solução em **.NET 10** seguindo *Clean Architecture* (4 projetos):

```
HubEsportesLages.slnx
└── src/
    ├── HubEsportesLages.Domain          # Entidades e enums (sem dependências)
    ├── HubEsportesLages.Application      # DTOs, interfaces de serviço, mapeamentos
    ├── HubEsportesLages.Infrastructure   # EF Core + SQLite, serviços, seed de dados
    └── HubEsportesLages.Web              # MVC (site) + API REST + Swagger + worker
```

| Camada | Responsabilidade |
|---|---|
| **Domain** | `Evento`, `Modalidade`, `Equipe`, `Local`, `Inscricao`, `Notificacao` e enums. |
| **Application** | Contratos (`IEventoService`, `ICatalogoService`, `IInscricaoService`, `INotificacaoService`), DTOs e filtros. |
| **Infrastructure** | `HubDbContext` (SQLite), implementação dos serviços, `DataSeeder` e injeção de dependência. |
| **Web** | Controllers MVC + API, views Razor, design system próprio (CSS) e `NotificacaoLembreteWorker`. |

**Stack:** ASP.NET Core MVC, Entity Framework Core 10 (SQLite), Swagger (Swashbuckle), Razor + CSS próprio (sem dependência de build de front-end).

---

## Como executar

Pré-requisito: **.NET SDK 10**.

```bash
dotnet run --project src/HubEsportesLages.Web
```

Na primeira execução o banco SQLite (`hubesportes.db`) é criado e populado automaticamente com um
cenário demonstrativo da cena esportiva de Lages (modalidades, locais, equipes e uma agenda com
eventos passados, ao vivo e futuros — com datas relativas ao dia da execução).

Acesse no navegador:

- **Site:** a URL exibida no console (ex.: `http://localhost:5210`)
- **API / Swagger:** `/swagger`

> Para fixar a porta: `dotnet run --project src/HubEsportesLages.Web --urls http://localhost:5210`

---

## API REST (principais endpoints)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/eventos` | Agenda (filtros: `Modalidade`, `LocalId`, `Busca`, `Periodo`, `Pagina`...) |
| `GET` | `/api/eventos/resultados` | Eventos encerrados com placar |
| `GET` | `/api/eventos/destaques` | Eventos em destaque |
| `GET` | `/api/eventos/{slug}` | Detalhe de um evento |
| `POST` | `/api/eventos` | Publica um novo evento |
| `PUT` | `/api/eventos/{id}/resultado` | Atualiza placar/encerramento |
| `GET` | `/api/notificacoes` | Feed de notificações |
| `POST` | `/api/notificacoes/gerar-lembretes` | Gera lembretes das próximas 24h |
| `POST` | `/api/inscricoes` | Inscreve um torcedor |
| `GET` | `/api/catalogo/modalidades` \| `/locais` \| `/equipes` | Dados de apoio |

Exemplo:

```bash
curl http://localhost:5210/api/eventos?Modalidade=futsal&Periodo=Todos
```

---

## Observações

- Os **locais, equipes e jogos** do seed são **ilustrativos**, montados para demonstrar o produto.
- O envio real de e-mail/push não está implementado — as inscrições e o feed simulam o canal de
  notificação (ponto natural de evolução, integrando provedor de e-mail/Web Push/WhatsApp).
- O banco é recriado a partir do seed apenas quando está vazio; apague `hubesportes.db` para resetar.

## Próximos passos

- Autenticação na área do organizador.
- Envio real de notificações (e-mail / Web Push / WhatsApp) a partir das inscrições.
- Check-in/ingresso digital com QR Code (entrada nos ginásios).
- App PWA com notificações no celular.
