using HubEsportesLages.Application.DTOs;

namespace HubEsportesLages.Application.Interfaces;

/// <summary>Dados de apoio: modalidades, locais e equipes (usados em filtros e formulários).</summary>
public interface ICatalogoService
{
    Task<IReadOnlyList<ModalidadeDto>> ListarModalidadesAsync(CancellationToken ct = default);

    Task<IReadOnlyList<LocalDto>> ListarLocaisAsync(CancellationToken ct = default);

    Task<IReadOnlyList<EquipeDto>> ListarEquipesAsync(int? modalidadeId = null, CancellationToken ct = default);
}
