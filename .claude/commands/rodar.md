---
description: Compila e executa o site/API do Hub Esportes Lages e confirma que subiu.
allowed-tools: Bash
---

Compile e rode a aplicação web do Hub Esportes Lages:

1. `dotnet build HubEsportesLages.slnx` — pare e mostre os erros se a compilação falhar.
2. Suba a aplicação em segundo plano fixando a porta:
   `dotnet run --project src/HubEsportesLages.Web --no-launch-profile --urls http://localhost:5210`
3. Aguarde até `http://localhost:5210/` responder HTTP 200 (faça polling com `curl`, sem `sleep` longo).
4. Confirme que o banco semeou: `curl -s http://localhost:5210/api/catalogo/modalidades`.
5. Informe ao usuário a URL do site (`http://localhost:5210`) e do Swagger (`/swagger`).

Não rode dois servidores na mesma porta (o DLL fica travado pelo processo em execução).
