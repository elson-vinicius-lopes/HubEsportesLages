# Design — Ingresso pago com QR-Code (Pix simulado)

> Segue a Clean Architecture do `AGENTS.md`. Pagamento é **simulado** (mock) com a interface pronta
> para trocar por um provedor Pix real depois, sem mudar o resto.

## Fluxo
```
Torcedor (logado) → [Comprar] → Ingresso(Pendente) + QR Pix SIMULADO (copia-e-cola fake + imagem)
        → [Confirmar pagamento (simulado)] → Ingresso(Pago) + token assinado + QR do ingresso
Admin no portão → [Scanner/Código] → POST /api/ingressos/validar → confere assinatura+Pago+não-utilizado
        → Ingresso(Utilizado) → ✅ / ❌(motivo)
```

## Domain (`HubEsportesLages.Domain`)
- `enum StatusIngresso { Pendente=0, Pago=1, Utilizado=2, Cancelado=3 }`
- `Ingresso` { Id, EventoId, Evento?, **CompradorId** (string = e-mail/login do usuário), CompradorNome,
  Preco (decimal), Status, **Token** (string? — só após pago), TxidPix (string — id do "pagamento" simulado),
  CriadoEm, PagoEm?, UtilizadoEm?, ValidadoPor? (admin que validou) }.
  - Índice em `CompradorId`; `Token` único quando presente.

## Application (`HubEsportesLages.Application`)
- DTOs (camelCase): `CriarIngressoDto { eventoId }`; `IngressoDto { id, eventoTitulo, eventoSlug, preco, status,
  criadoEm, pagoEm }`; `PagamentoPixDto { ingressoId, pixCopiaECola, qrPagamentoBase64, valor }`;
  `IngressoEmitidoDto { id, token, qrIngressoBase64, eventoTitulo }`; `ValidarIngressoDto { token }`;
  `ValidacaoResultadoDto { valido, status, mensagem, eventoTitulo?, compradorNome? }`.
- Interfaces:
  - `IPagamentoService` (mock): `GerarCobrancaPix(ingresso) → PagamentoPixDto`; `ConfirmarPagamento(txid) → bool`.
    Implementação `MockPixPagamentoService` (gera copia-e-cola fake `00020126...` + txid; confirma sempre true).
  - `IIngressoService`: `ComprarAsync(eventoId, compradorId, nome)`, `ConfirmarPagamentoAsync(id, compradorId)`,
    `ListarMeusAsync(compradorId)`, `ValidarAsync(token, adminId)`.
  - `ITokenIngresso`: `Gerar(ingressoId) → string` (HMAC-SHA256 sobre `id` + segredo de config, base64url);
    `Validar(token) → int? ingressoId` (recomputa e confere).

## Infrastructure (`HubEsportesLages.Infrastructure`)
- `HubDbContext`: `DbSet<Ingresso>` + Fluent (índices; `Preco` HasPrecision(10,2)).
- `IngressoService`: orquestra compra (cria Pendente + `IPagamentoService.GerarCobrancaPix`), confirmação
  (valida dono + status → Pago + `ITokenIngresso.Gerar` → Token), validação (decodifica token → carrega
  ingresso → checa Pago + não-utilizado → marca Utilizado + ValidadoPor; uso único idempotente).
- `MockPixPagamentoService`, `TokenIngresso` (lê segredo de `Ingressos:Segredo` em config),
  e geração de **QR via `QRCoder`** (PNG → base64) num helper `QrCodeGenerator`.
  - **Pacote novo:** `QRCoder` (server-side, sem dependência externa).
- Registrar tudo no `DependencyInjection`. (Opcional) seed: 1 ingresso pago de exemplo.

## Web (`HubEsportesLages.Web`)
- **API** `Controllers/Api/IngressosApiController.cs`:
  - `POST /api/ingressos` `[Authorize]` { eventoId } → `PagamentoPixDto` (Pendente + QR Pix). 400 se gratuito.
  - `POST /api/ingressos/{id}/confirmar-pagamento` `[Authorize]` → `IngressoEmitidoDto` (Pago + QR ingresso).
  - `GET  /api/ingressos/meus` `[Authorize]` → `IngressoDto[]`.
  - `POST /api/ingressos/validar` **`[Authorize(Roles="Admin")]`** { token } → `ValidacaoResultadoDto`.
- **MVC**:
  - Botão **"Comprar ingresso"** na página do evento (`Views/Agenda/Evento.cshtml`) quando `!Gratuito` (logado).
  - `IngressosController`: página de **compra/pagamento** (mostra QR Pix + botão "Já paguei (simulado)" → QR do ingresso)
    e **"Meus ingressos"**; usa `User.Identity.Name` como comprador.
  - **Admin scanner**: view com leitor de câmera (lib JS vendida `html5-qrcode`) + **campo manual** de fallback,
    chamando `POST /api/ingressos/validar`. Mostra ✅/❌ grande.
- **Segredo** do token em `appsettings` (`Ingressos:Segredo`) — não commitar segredo real; usar placeholder dev.

## Decisões, trade-offs e riscos
- **Pix simulado** atrás de `IPagamentoService` → trocar por Mercado Pago/Asaas/Efí depois sem tocar no resto.
- **Token assinado (HMAC)** = anti-falsificação sem guardar segredo no QR; **uso único** no banco evita reentrada.
- **Identidade do comprador** = usuário logado (e-mail). Quando houver `Usuario` em banco, vincular por id.
- Scanner web depende de **HTTPS/permissão de câmera**; o **campo manual** garante a demo mesmo sem câmera.
- QR server-side (`QRCoder`) → nada de dependência de build de front-end.
