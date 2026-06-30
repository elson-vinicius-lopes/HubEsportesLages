using QRCoder;

namespace HubEsportesLages.Infrastructure.Common;

/// <summary>
/// Geração de QR-Code server-side via QRCoder (headless, sem System.Drawing).
/// Retorna um PNG já pronto para embutir em <c>&lt;img src="data:image/png;base64,..."&gt;</c>.
/// </summary>
public static class QrCodeGenerator
{
    /// <summary>Gera o QR do conteúdo e devolve a imagem PNG em base64 (sem o prefixo data:).</summary>
    public static string GerarPngBase64(string conteudo, int pixelsPorModulo = 8)
    {
        using var generator = new QRCodeGenerator();
        using var dados = generator.CreateQrCode(conteudo, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(dados);
        var bytes = png.GetGraphic(pixelsPorModulo);
        return Convert.ToBase64String(bytes);
    }
}
