---
description: Revisa as mudanças atuais do Hub Esportes Lages usando o agente revisor-codigo.
---

Acione o subagente **revisor-codigo** para revisar o estado atual do código (foque no que mudou
recentemente / no escopo em andamento). Peça a ele que:

1. Compile a solução (`dotnet build HubEsportesLages.slnx`) e reporte erros/avisos relevantes.
2. Verifique aderência à Clean Architecture, padrões de EF Core, registro de serviços em DI,
   publicação de notificações, convenções de API e o design system das views.
3. Liste os achados priorizados como **[Bug] / [Risco] / [Arquitetura] / [Melhoria]** com
   `arquivo:linha` e correção sugerida.

Ao receber o resultado, resuma os pontos críticos e pergunte se devo aplicar as correções.
