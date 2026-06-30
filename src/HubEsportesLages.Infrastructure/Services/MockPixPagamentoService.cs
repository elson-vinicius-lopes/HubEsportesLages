using System.Globalization;
using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Domain.Entities;
using HubEsportesLages.Infrastructure.Common;

namespace HubEsportesLages.Infrastructure.Services;

/// <summary>
/// Gateway Pix SIMULADO: gera um copia-e-cola fake (começa com "00020126...") + um txid e
/// confirma sempre. Trocar por um provedor real (Mercado Pago/Asaas/Efí) implementando
/// <see cref="IPagamentoService"/> — o resto do fluxo não muda.
/// </summary>
public class MockPixPagamentoService : IPagamentoService
{
    public PagamentoPixDto GerarCobrancaPix(Ingresso ingresso)
    {
        var pixCopiaECola = MontarCopiaECola(ingresso.TxidPix, ingresso.Preco);
        var qr = QrCodeGenerator.GerarPngBase64(pixCopiaECola);
        return new PagamentoPixDto(ingresso.Id, pixCopiaECola, qr, ingresso.Preco);
    }

    /// <summary>No mock o pagamento é sempre confirmado (não há provedor real).</summary>
    public bool ConfirmarPagamento(string txidPix) => !string.IsNullOrWhiteSpace(txidPix);

    /// <summary>Gera um txid no formato esperado por um provedor Pix (alfanumérico, 26 chars).</summary>
    public static string GerarTxid() =>
        Guid.NewGuid().ToString("N").ToUpperInvariant()[..26];

    // Monta um "BR Code" fictício no formato copia-e-cola (apenas para a demonstração).
    private static string MontarCopiaECola(string txid, decimal valor)
    {
        var valorFmt = valor.ToString("F2", CultureInfo.InvariantCulture);
        // Estrutura inspirada no padrão EMV/Pix: começa com "00020126" e embute txid/valor.
        var payload =
            "00020126" +
            "580014BR.GOV.BCB.PIX" +
            "0136bora-pro-jogo@hubesporteslages.sc" +
            "52040000" +
            "5303986" +
            "54" + valorFmt.Length.ToString("D2") + valorFmt +
            "5802BR" +
            "5913BoraProJogo" +
            "6008LAGES-SC" +
            "62" + (4 + txid.Length).ToString("D2") + "05" + txid.Length.ToString("D2") + txid +
            "6304SIMU";
        return payload;
    }
}
