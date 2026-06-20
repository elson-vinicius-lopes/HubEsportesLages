---
description: Smoke test dos endpoints da API do Hub Esportes Lages (assume a app rodando em :5210).
allowed-tools: Bash
argument-hint: "[url-base opcional, padrão http://localhost:5210]"
---

Faça um smoke test da API. Use a URL base de `$ARGUMENTS` ou `http://localhost:5210` por padrão.
Se a aplicação não estiver no ar, suba-a antes (ver `/rodar`).

Verifique status HTTP e um trecho da resposta de cada um:

- `GET  /api/catalogo/modalidades`
- `GET  /api/catalogo/locais`
- `GET  /api/eventos?TamanhoPagina=3`
- `GET  /api/eventos/destaques`
- `GET  /api/eventos/resultados`
- `GET  /api/notificacoes?quantidade=5`
- `POST /api/inscricoes` (corpo: nome/email ASCII) → espera 201
- `POST /api/eventos` (use **título sem acentos** no corpo JSON p/ evitar erro de encoding do shell) → espera 201
- `PUT  /api/eventos/{id}/resultado` com o id criado acima → espera 204
- Reconsulte `/api/notificacoes` e confirme que apareceram as notificações de "novo evento" e "resultado".

Ao final, apresente uma tabela: endpoint · status · OK/Falha. Aponte qualquer resposta inesperada.
