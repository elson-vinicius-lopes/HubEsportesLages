using HubEsportesLages.Application.DTOs;

namespace HubEsportesLages.Application.Interfaces;

/// <summary>Gerencia as inscrições dos torcedores no hub de notificações.</summary>
public interface IInscricaoService
{
    /// <summary>Registra (ou reativa) uma inscrição. Retorna o registro persistido.</summary>
    Task<InscricaoDto> InscreverAsync(CriarInscricaoDto dto, CancellationToken ct = default);

    Task<int> ContarAtivasAsync(CancellationToken ct = default);
}
