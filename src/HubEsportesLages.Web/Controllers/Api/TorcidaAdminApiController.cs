using HubEsportesLages.Application.Common;
using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HubEsportesLages.Web.Controllers.Api;

/// <summary>
/// Operações da organização sobre a interação da torcida: definição da escalação
/// (candidatos a MVP), cadastro de enquete e moderação do mural.
/// Restrito à role Admin (cookie do site ou JWT Bearer).
/// </summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/eventos/{eventoId:int}/torcida")]
[Produces("application/json")]
[Tags("Torcida (organização)")]
public class TorcidaAdminApiController(IModeracaoService moderacao) : ControllerBase
{
    /// <summary>Cria e ativa a enquete do evento (desativa a anterior).</summary>
    [HttpPost("enquete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CriarEnquete(int eventoId, [FromBody] CriarEnqueteDto dto, CancellationToken ct)
        => Mapear(await moderacao.CriarEnqueteAsync(eventoId, dto, ct));

    /// <summary>Define a escalação (candidatos a MVP) do evento, substituindo a anterior.</summary>
    [HttpPost("jogadores")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DefinirEscalacao(int eventoId, [FromBody] DefinirEscalacaoDto dto, CancellationToken ct)
        => Mapear(await moderacao.DefinirEscalacaoAsync(eventoId, dto, ct));

    /// <summary>Remove (oculta) uma mensagem do mural.</summary>
    [HttpDelete("mensagens/{mensagemId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverMensagem(int eventoId, int mensagemId, CancellationToken ct)
        => Mapear(await moderacao.RemoverMensagemAsync(eventoId, mensagemId, ct));

    private IActionResult Mapear(StatusInteracao status) => status switch
    {
        StatusInteracao.Ok => NoContent(),
        StatusInteracao.NaoEncontrado => NotFound(),
        StatusInteracao.Invalido => UnprocessableEntity(new { mensagem = "Dados inválidos." }),
        _ => UnprocessableEntity()
    };
}
