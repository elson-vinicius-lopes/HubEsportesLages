using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HubEsportesLages.Web.Controllers.Api;

/// <summary>Inscrições de torcedores no hub de notificações.</summary>
[ApiController]
[Route("api/inscricoes")]
[Produces("application/json")]
[Tags("Inscrições")]
public class InscricoesApiController(IInscricaoService inscricoes) : ControllerBase
{
    /// <summary>Registra uma inscrição para receber notificações (geral, por modalidade ou por equipe).</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InscricaoDto>> Inscrever([FromBody] CriarInscricaoDto dto, CancellationToken ct)
    {
        var inscricao = await inscricoes.InscreverAsync(dto, ct);
        return CreatedAtAction(nameof(Inscrever), new { id = inscricao.Id }, inscricao);
    }
}
