# Tarefas — Interação da Torcida Ao Vivo

> Marque ao concluir. "Done" = critérios de aceite de `requisitos.md` atendidos. Agente sugerido entre [].

## Fase 1 — REST persistido (sem tempo real)
- [ ] **Domain**: entidades `JogadorEvento`, `VotoMvp`, `Enquete`, `OpcaoEnquete`, `VotoEnquete`,
      `MensagemTorcida`, `EquipeFavorita`; método `Evento.AceitaInteracao`. [dev-backend]
- [ ] **Infrastructure**: `HubDbContext` (DbSets + Fluent API + **índices únicos** de 1 voto); `EnsureCreated`/seed
      de enquete + escalação para o evento AoVivo. [dev-backend]
- [ ] **Application**: DTOs (`TorcidaEstadoDto`, `MensagemDto`, comandos), `ITorcidaService`,
      `ITorcedorContexto`, `IModeracaoService`, mapeamentos. [dev-backend]
- [ ] **Infrastructure**: `TorcidaService` (votos idempotentes, tally agregado, mensagens c/ limite+rate,
      favoritos toggle) + registro no `DependencyInjection`. [dev-backend]
- [ ] **Web/Identidade**: middleware `X-Torcedor-Id` → `ITorcedorContexto` (fallback anônimo). [dev-backend]
- [ ] **Web/API**: `TorcidaApiController` (GET estado; POST mvp; POST enquete/voto; GET/POST mensagens;
      POST/DELETE favoritos) com **gating AoVivo** nas escritas (409); endpoints admin (enquete, jogadores,
      remover mensagem). Swagger ok. [dev-backend]
- [ ] **Mobile**: botão "Interagir" com os 3 estados por status (AoVivo/Agendado/Encerrado) no
      EventDetailPage e na CheckInPage. [dev-mobile / designer-ui]
- [ ] **Mobile**: `InteractionPage` + `InteractionViewModel` via REST (carrega estado; vota MVP/enquete com
      trava de 1 voto; envia mensagem; favorita) + estados loading/erro/vazio. [dev-mobile]

## Fase 2 — Tempo real (SignalR)
- [ ] **Web**: `TorcidaHub` (`/hubs/torcida`) + `AddSignalR`/`MapHub`; `TorcidaService` publica
      `MvpAtualizado`/`EnqueteAtualizada`/`NovaMensagem`/`MensagemRemovida` no grupo `evento-{id}`. [dev-backend]
- [ ] **Mobile**: cliente `SignalR.Client` (conectar ao grupo do evento AoVivo, handlers, `WithAutomaticReconnect`,
      desconectar no `OnDisappearing`); fallback polling quando offline. [dev-mobile]

## Fase 3 — Robustez e extras
- [ ] **Anti-abuso/moderação**: rate limit + tamanho + filtro simples; tela/admin de moderação. [dev-backend]
- [ ] **(Opcional) Gamificação**: conceder pontos ao interagir (depende da lacuna #4). [dev-backend]
- [ ] **Testes** cobrindo os critérios de aceite: 1 voto por evento/enquete, gating por status (409 fora de
      AoVivo), mensagem inválida/rate limit, atualização em tempo real. [dev-backend]
- [ ] **Spec e código revisados** (não divergem). [revisor-codigo]
