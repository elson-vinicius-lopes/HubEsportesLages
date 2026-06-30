using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Domain.Entities;

namespace HubEsportesLages.Application.Interfaces;

/// <summary>
/// Gateway de pagamento Pix. A implementação atual é simulada (mock); a interface
/// está pronta para ser trocada por um provedor real (Mercado Pago/Asaas/Efí) depois.
/// </summary>
public interface IPagamentoService
{
    /// <summary>Gera a cobrança Pix de um ingresso — copia-e-cola + QR de pagamento + valor.</summary>
    PagamentoPixDto GerarCobrancaPix(Ingresso ingresso);

    /// <summary>Confirma o pagamento de um txid. No mock retorna sempre verdadeiro.</summary>
    bool ConfirmarPagamento(string txidPix);
}
