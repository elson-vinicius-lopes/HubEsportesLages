namespace HubEsportesLages.Application.Common;

/// <summary>
/// Constantes da conformidade LGPD (spec: docs/specs/lgpd/).
/// </summary>
public static class LgpdConstantes
{
    /// <summary>
    /// Versão vigente da Política de Privacidade (/privacidade). Gravada no usuário
    /// no momento do aceite; atualize ao publicar uma nova versão da política.
    /// </summary>
    public const string VersaoPoliticaAtual = "v1";

    /// <summary>Nome gravado nos ingressos ao anonimizar o comprador na exclusão da conta.</summary>
    public const string CompradorNomeAnonimizado = "Titular removido";
}
