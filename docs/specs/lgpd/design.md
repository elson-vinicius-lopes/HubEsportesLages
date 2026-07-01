# Design — Conformidade LGPD

> O "como". Referencie camadas e contratos concretos do repo.

## Backend (.NET 10)

### Domain
- Sem alterações — o titular é o usuário do Identity (`ApplicationUser`, que vive em
  Infrastructure por depender do ASP.NET Identity). `Inscricao` e `Ingresso` já existem.

### Application
- `Common/LgpdConstantes.cs` — versão vigente da política (`VersaoPoliticaAtual = "v1"`) e o
  texto de anonimização (`CompradorNomeAnonimizado = "Titular removido"`).
- `DTOs/LgpdDtos.cs`:
  - `ContaTitularDto(Nome, Email, ConsentimentoLgpdEm, ConsentimentoVersao)`;
  - `MeusDadosDto(GeradoEm, Conta, InscricoesAlerta, Ingressos, ObservacaoTorcida)` — payload
    do export (reusa `InscricaoDto` e `IngressoDto` existentes);
  - `ExclusaoDadosDto(InscricoesRemovidas, IngressosAnonimizados)`.
- `Interfaces/ILgpdService.cs`:
  - `ListarDadosVinculadosAsync(email)` → inscrições + ingressos do titular;
  - `ApagarDadosVinculadosAsync(email)` → apaga inscrições e anonimiza ingressos.

### Infrastructure
- `Identidade/ApplicationUser.cs` — novos campos `ConsentimentoLgpdEm` (`DateTime?`, UTC) e
  `ConsentimentoVersao` (`string?`, ex.: "v1").
- `Persistence/HubDbContext.cs` — Fluent API: `ConsentimentoVersao` com `HasMaxLength(16)`.
- `Services/LgpdService.cs` — implementação com `HubDbContext` (`AsNoTracking` + `Include`
  nas leituras). A exclusão usa `ExecuteDeleteAsync`/atualização em lote:
  - `Inscricoes` com `Email == email` (normalizado lower) → DELETE;
  - `Ingressos` com `CompradorId == email` → `CompradorNome = "Titular removido"` e
    `CompradorId = "removido:<guid>"` (um marcador por exclusão). **Decisão:** o CompradorId
    guarda o e-mail (dado pessoal), então também é anonimizado; o marcador preserva o
    agrupamento contábil dos registros sem identificar a pessoa e evita que um futuro
    cadastro com o mesmo e-mail herde os ingressos antigos.
- `DependencyInjection.cs` — `services.AddScoped<ILgpdService, LgpdService>()`.
- Migration `ConsentimentoLgpd` (duas colunas em `AspNetUsers`); aplicada pelo `MigrateAsync`
  já existente na inicialização.

### Web
- `Controllers/PrivacidadeController.cs` — `GET /privacidade` → view estática
  `Views/Privacidade/Index.cshtml` (política em pt-BR). Link no rodapé do `_Layout.cshtml`.
- `Controllers/ContaController.cs`:
  - `Registrar` (POST) ganha o parâmetro `bool aceitePrivacidade`; sem aceite → erro de
    validação; com aceite → grava `ConsentimentoLgpdEm = DateTime.UtcNow` e
    `ConsentimentoVersao = LgpdConstantes.VersaoPoliticaAtual`.
  - `GET /conta/meus-dados` (`[Authorize]`) — monta `MeusDadosDto` e devolve download JSON
    (`application/json`, camelCase indentado, nome `meus-dados-bora-pro-jogo.json`).
  - `POST /conta/excluir` (`[Authorize]` + antiforgery, parâmetro `senha`) — valida a senha
    (`CheckPasswordAsync`); bloqueia o último Admin (`GetUsersInRoleAsync("Admin").Count <= 1`);
    chama `ILgpdService.ApagarDadosVinculadosAsync`, `UserManager.DeleteAsync`, `SignOutAsync`
    e redireciona ao login com confirmação. Erros voltam ao painel via `TempData["ExclusaoErro"]`.
- `Controllers/Api/AuthApiController.cs` — `RegistrarRequisicaoDto` ganha
  `bool AceitePrivacidade`; `false` → 400; `true` → grava consentimento (igual ao site).
- Views:
  - `Views/Conta/Registrar.cshtml` — checkbox obrigatório com link para `/privacidade`;
  - `Views/Conta/Index.cshtml` — painel "Privacidade e meus dados" (exportar + excluir conta);
  - `Views/Notificacoes/Index.cshtml` — nota de privacidade no formulário de alertas.
- **Fix de segurança:** `TorcidaAdminApiController` recebe `[Authorize(Roles = "Admin")]`;
  `FavoritosApiController` recebe `[Authorize]` (política padrão aceita cookie do site e JWT —
  o `torcida.js` usa `fetch` same-origin, que envia o cookie da sessão logada).
- `Program.cs` — nos eventos do cookie, requisições `/api/...` não autenticadas passam a
  receber **401/403** em vez do redirect HTML para a tela de login (evita que o `fetch` do
  `torcida.js` interprete a página de login como sucesso). `torcida.js` traduz 401 para
  "entre na sua conta".

## API (contrato camelCase)
- `POST /api/auth/registrar` — request passa a exigir
  `{ "nome": "...", "email": "...", "senha": "...", "aceitePrivacidade": true }`;
  sem aceite → `400 { "mensagem": "É preciso aceitar a Política de Privacidade (/privacidade) para criar a conta." }`.
- `GET /conta/meus-dados` (site, cookie) — download JSON:
  `{ "geradoEm", "conta": { "nome", "email", "consentimentoLgpdEm", "consentimentoVersao" }, "inscricoesAlerta": [...], "ingressos": [...], "observacaoTorcida": "..." }`.
- `POST /conta/excluir` (site, cookie + antiforgery) — form `senha=...`.
- `POST|DELETE /api/favoritos/equipes/{equipeId}` — agora exige autenticação (cookie ou JWT).
- `POST /api/eventos/{eventoId}/torcida/enquete|jogadores`, `DELETE .../mensagens/{id}` —
  agora exigem role `Admin`.

## Mobile (Arena Lages, MAUI)
- A tela de cadastro do app deve exibir o aceite da política (link para `/privacidade`) e
  enviar `aceitePrivacidade: true` no `POST /api/auth/registrar` — sem isso a API recusa (400).

## Decisões e trade-offs / riscos
- **TorcedorId anônimo:** votos, mensagens do mural e equipes favoritas são gravados sob um
  GUID gerado no navegador (`X-Torcedor-Id`), sem vínculo com a conta. Por isso NÃO entram no
  export nem na exclusão — a política documenta essa característica (dado anônimo por construção).
- **Ingressos não são apagados:** são registro contábil da venda; anonimizamos comprador
  (nome e identificador) e mantemos valor/status/datas — base legal passa a ser interesse
  legítimo/obrigação de guarda contábil.
- **Versão do consentimento:** campo simples (`"v1"`); mudar a política implica atualizar
  `LgpdConstantes.VersaoPoliticaAtual` e a view. Sem histórico de versões (fora de escopo).
- **Último admin não se exclui:** evita perder o acesso administrativo do hub; a exclusão do
  admin exige antes promover outro usuário a Admin.
