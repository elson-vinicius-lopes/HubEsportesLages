namespace HubEsportesLages.Application.Interfaces;

/// <summary>
/// Assinatura/validação do token de um ingresso (HMAC-SHA256 sobre o id + um segredo
/// de configuração, codificado em base64url). Anti-falsificação: não basta saber o id.
/// </summary>
public interface ITokenIngresso
{
    /// <summary>Gera o token assinado de um ingresso a partir do seu id.</summary>
    string Gerar(int ingressoId);

    /// <summary>Recomputa e confere a assinatura. Retorna o id do ingresso ou null se inválido.</summary>
    int? Validar(string token);
}
