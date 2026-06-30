# Tarefas — Ingresso pago com QR-Code (Pix simulado)

> "Done" = critérios de aceite de `requisitos.md`. Agente sugerido entre [].

## Fase 1 — Backend (compra → pagamento simulado → emissão → validação)
- [ ] **Domain**: `StatusIngresso` + entidade `Ingresso`. [dev-backend]
- [ ] **Infra**: `HubDbContext` (DbSet + índices + precisão); pacote **`QRCoder`**; `QrCodeGenerator` (PNG→base64). [dev-backend]
- [ ] **Application**: DTOs + `IPagamentoService` + `IIngressoService` + `ITokenIngresso`. [dev-backend]
- [ ] **Infra**: `MockPixPagamentoService`, `TokenIngresso` (HMAC + segredo de config), `IngressoService`
      (comprar/confirmar/listar/validar com **uso único**) + registro no DI. [dev-backend]
- [ ] **Web API**: `IngressosApiController` (POST comprar `[Authorize]`; POST confirmar-pagamento; GET meus;
      **POST validar `[Authorize(Roles="Admin")]`**) + `Ingressos:Segredo` no appsettings. [dev-backend]

## Fase 2 — Front-end web (demo)
- [ ] Botão **"Comprar ingresso"** em `Views/Agenda/Evento.cshtml` quando `!Gratuito` e logado. [designer-ui]
- [ ] `IngressosController` + views: **compra/pagamento** (QR Pix + "Já paguei (simulado)" → QR do ingresso)
      e **"Meus ingressos"** (lista + QR). [dev-backend / designer-ui]
- [ ] **Admin scanner**: view com `html5-qrcode` (vendida em `wwwroot/lib`) + campo manual de fallback →
      `POST /api/ingressos/validar`; resultado ✅/❌ grande. [designer-ui]
- [ ] CSS dos cartões de ingresso/QR em `site.css`. [designer-ui]

## Fase 3 — Validação
- [ ] Build verde (`dotnet build HubEsportesLages.slnx`) + smoke: comprar → confirmar → validar (✅) →
      validar de novo (❌ já utilizado) → validar como torcedor (403). [revisor-codigo]
- [ ] Revisão: token assinado, uso único idempotente, gating de role, sem entidade vazando, camelCase. [revisor-codigo]

## Futuro (pós-apresentação)
- [ ] Trocar `MockPixPagamentoService` por provedor real (Mercado Pago/Asaas/Efí) + webhook de confirmação.
- [ ] Scanner no app **MAUI** (`ZXing.Net.Maui`) para o perfil admin.
