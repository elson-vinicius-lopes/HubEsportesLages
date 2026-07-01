using HubEsportesLages.Application.DTOs;

namespace HubEsportesLages.Application.Interfaces;

/// <summary>
/// Direitos do titular (LGPD): acesso/portabilidade e exclusão dos dados de domínio
/// vinculados à conta (inscrições de alerta e ingressos). A conta Identity em si é
/// tratada pelo UserManager na camada Web. Spec: docs/specs/lgpd/.
/// </summary>
public interface ILgpdService
{
    /// <summary>Inscrições de alerta registradas com o e-mail do titular (mais recentes primeiro).</summary>
    Task<IReadOnlyList<InscricaoDto>> ListarInscricoesDoTitularAsync(string email, CancellationToken ct = default);

    /// <summary>Ingressos comprados pelo titular (mais recentes primeiro).</summary>
    Task<IReadOnlyList<IngressoDto>> ListarIngressosDoTitularAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Apaga os dados de domínio vinculados ao titular: remove as inscrições de alerta do
    /// e-mail e anonimiza os ingressos (nome e identificador do comprador), preservando os
    /// registros contábeis (valor, status e datas).
    /// </summary>
    Task<ExclusaoDadosDto> ApagarDadosVinculadosAsync(string email, CancellationToken ct = default);
}
