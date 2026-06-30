using System.Security.Cryptography;
using System.Text;
using HubEsportesLages.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HubEsportesLages.Infrastructure.Services;

/// <summary>
/// Token assinado de um ingresso. Formato base64url de <c>"{id}.{assinatura}"</c>, onde a
/// assinatura é um HMAC-SHA256 sobre o id usando o segredo de <c>Ingressos:Segredo</c>.
/// A validação recomputa o HMAC e compara — não basta conhecer o id para forjar.
/// </summary>
public class TokenIngresso(IConfiguration configuration) : ITokenIngresso
{
    private readonly byte[] _segredo = Encoding.UTF8.GetBytes(
        configuration["Ingressos:Segredo"]
        ?? throw new InvalidOperationException("Configure 'Ingressos:Segredo'."));

    public string Gerar(int ingressoId)
    {
        var assinatura = Assinar(ingressoId);
        return Base64UrlEncode(Encoding.UTF8.GetBytes($"{ingressoId}.{assinatura}"));
    }

    public int? Validar(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        string conteudo;
        try
        {
            conteudo = Encoding.UTF8.GetString(Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            return null;
        }

        var partes = conteudo.Split('.', 2);
        if (partes.Length != 2 || !int.TryParse(partes[0], out var id))
            return null;

        var esperada = Assinar(id);
        // Comparação em tempo constante para evitar timing attacks.
        var iguais = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(partes[1]),
            Encoding.UTF8.GetBytes(esperada));

        return iguais ? id : null;
    }

    private string Assinar(int ingressoId)
    {
        using var hmac = new HMACSHA256(_segredo);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"ingresso:{ingressoId}"));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string texto)
    {
        var s = texto.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }
}
