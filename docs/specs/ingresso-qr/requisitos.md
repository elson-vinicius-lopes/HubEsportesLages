# Requisitos — Ingresso pago com QR-Code (Pix simulado)

> Feature-diferencial para a apresentação à Secretaria: vender ingresso, gerar QR de pagamento
> (**Pix simulado**, sem provedor real agora), emitir **ingresso digital com QR** e o **admin
> validar/check-in** lendo o QR no portão. Gera modelo de receita para associações/Fundação.

## Contexto
Hoje a entrada nos ginásios é manual e não há receita digital. Esta feature digitaliza a bilheteria:
compra → pagamento (Pix simulado) → ingresso com QR único → validação pelo admin na entrada (uso único).

## Personas
- **Torcedor (logado)** — compra o ingresso e apresenta o QR na entrada.
- **Admin** — escaneia/valida o QR no portão e faz o check-in.

## User stories
- Como **torcedor logado**, quero **comprar um ingresso** de um evento pago e **pagar via Pix (simulado)**,
  para garantir minha entrada.
- Como **torcedor**, quero ver **"Meus ingressos"** com o **QR** para apresentar na entrada.
- Como **admin**, quero **escanear o QR** (ou digitar o código) e **validar** o ingresso, para liberar a entrada.
- Como **admin**, quero que um ingresso **só possa ser usado uma vez** (anti-reentrada/fraude).

## Critérios de aceite (testáveis)
- [ ] Comprar só é permitido para **usuário logado** e evento **não gratuito** (`Gratuito == false`).
- [ ] Ao comprar, cria-se `Ingresso(Pendente)` e retorna-se um **QR de pagamento Pix simulado**
      (string copia-e-cola fake + imagem do QR).
- [ ] Ao **confirmar pagamento (simulado)**, o ingresso vira **`Pago`** e ganha um **token assinado**
      e o **QR do ingresso**.
- [ ] **Validação (admin)**: ler o token → se válido + `Pago` + **não utilizado** + evento correto →
      marca **`Utilizado`** e retorna ✅; senão retorna ❌ com motivo (inválido / já utilizado / não pago).
- [ ] Um ingresso **`Utilizado`** não valida de novo (uso único, idempotente).
- [ ] Validação é **`[Authorize(Roles="Admin")]`** — torcedor não valida.
- [ ] Token do ingresso é **assinado** (não falsificável só sabendo o id).

## Fora de escopo (agora)
- Integração com provedor Pix real (webhook, conciliação) — virá depois trocando a implementação do serviço.
- Lotes/meia-entrada/reembolso; emissão fiscal.

## Dependências
- Autenticação (já existe): comprador = usuário logado (`ContaController`). Admin = role Admin.
- Evento já tem `Gratuito`/`PrecoIngresso`.
