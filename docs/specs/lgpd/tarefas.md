# Tarefas — Conformidade LGPD

> Quebra acionável. Marque ao concluir. "Done" = critérios de aceite de `requisitos.md` atendidos.

- [x] T1 — Campos `ConsentimentoLgpdEm`/`ConsentimentoVersao` em `ApplicationUser` + Fluent API
      (`Infrastructure/Identidade/ApplicationUser.cs`, `Persistence/HubDbContext.cs`)
- [x] T2 — Migration `ConsentimentoLgpd` (`dotnet ef migrations add ConsentimentoLgpd -p src/HubEsportesLages.Infrastructure -s src/HubEsportesLages.Web`)
- [x] T3 — Constantes e DTOs LGPD (`Application/Common/LgpdConstantes.cs`, `Application/DTOs/LgpdDtos.cs`)
- [x] T4 — `ILgpdService` + `LgpdService` + registro no DI
      (`Application/Interfaces/ILgpdService.cs`, `Infrastructure/Services/LgpdService.cs`, `Infrastructure/DependencyInjection.cs`)
- [x] T5 — Consentimento no cadastro do site: checkbox obrigatório + persistência
      (`Web/Views/Conta/Registrar.cshtml`, `Web/Controllers/ContaController.cs`)
- [x] T6 — Consentimento no cadastro da API: `aceitePrivacidade` obrigatório
      (`Web/Controllers/Api/AuthApiController.cs`)
- [x] T7 — Página `/privacidade` + link no rodapé
      (`Web/Controllers/PrivacidadeController.cs`, `Web/Views/Privacidade/Index.cshtml`, `Web/Views/Shared/_Layout.cshtml`)
- [x] T8 — Exportar meus dados: `GET /conta/meus-dados` (download JSON camelCase)
      (`Web/Controllers/ContaController.cs`)
- [x] T9 — Excluir minha conta: `POST /conta/excluir` com senha, guarda do último admin,
      apaga inscrições, anonimiza ingressos, deleta usuário + sign-out; painel na view
      (`Web/Controllers/ContaController.cs`, `Web/Views/Conta/Index.cshtml`)
- [x] T10 — Minimização: nota de privacidade no formulário de alertas
      (`Web/Views/Notificacoes/Index.cshtml`)
- [x] T11 — Fix de segurança: `[Authorize(Roles="Admin")]` no `TorcidaAdminApiController` e
      `[Authorize]` no `FavoritosApiController`; `/api` não autenticado responde 401/403
      (`Web/Controllers/Api/TorcidaAdminApiController.cs`, `FavoritosApiController.cs`, `Web/Program.cs`, `wwwroot/js/torcida.js`)
- [ ] Testes cobrindo os critérios de aceite (não há projeto de testes no repo — validação manual)
- [x] Spec e código revisados (não divergem)
