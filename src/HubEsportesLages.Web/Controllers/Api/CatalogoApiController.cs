using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HubEsportesLages.Web.Controllers.Api;

/// <summary>Dados de apoio: modalidades, locais e equipes.</summary>
[ApiController]
[Route("api/catalogo")]
[Produces("application/json")]
[Tags("Catálogo")]
public class CatalogoApiController(ICatalogoService catalogo) : ControllerBase
{
    [HttpGet("modalidades")]
    public async Task<IReadOnlyList<ModalidadeDto>> Modalidades(CancellationToken ct) =>
        await catalogo.ListarModalidadesAsync(ct);

    [HttpGet("locais")]
    public async Task<IReadOnlyList<LocalDto>> Locais(CancellationToken ct) =>
        await catalogo.ListarLocaisAsync(ct);

    [HttpGet("equipes")]
    public async Task<IReadOnlyList<EquipeDto>> Equipes([FromQuery] int? modalidadeId, CancellationToken ct) =>
        await catalogo.ListarEquipesAsync(modalidadeId, ct);
}
