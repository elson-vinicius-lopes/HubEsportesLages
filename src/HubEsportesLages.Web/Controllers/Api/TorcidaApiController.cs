using HubEsportesLages.Application.Common;
using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HubEsportesLages.Web.Controllers.Api;

/// <summary>
/// Interação da torcida durante um evento ao vivo: estado agregado, votação de MVP,
/// enquete e mural. As escritas exigem o cabeçalho <c>X-Torcedor-Id</c> e o evento <c>AoVivo</c>.
/// </summary>
[ApiController]
[Route("api/eventos/{slug}/torcida")]
[Produces("application/json")]
[Tags("Torcida")]
public class TorcidaApiController(ITorcidaService torcida) : ControllerBase
{
    /// <summary>Estado agregado da torcida (MVP, enquete, mural e favorito). Leitura liberada em qualquer status.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TorcidaEstadoDto>> Estado(string slug, CancellationToken ct)
    {
        var estado = await torcida.ObterEstadoAsync(slug, ct);
        return estado is null ? NotFound() : Ok(estado);
    }

    /// <summary>Vota no Jogador da Partida (1 por torcedor por evento). Exige evento ao vivo.</summary>
    [HttpPost("mvp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> VotarMvp(string slug, [FromBody] VotarMvpDto dto, CancellationToken ct)
        => Estado(await torcida.VotarMvpAsync(slug, dto, ct));

    /// <summary>Vota em uma opção da enquete (1 por torcedor por enquete). Exige evento ao vivo.</summary>
    [HttpPost("enquete/{enqueteId:int}/voto")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> VotarEnquete(string slug, int enqueteId, [FromBody] VotarEnqueteDto dto, CancellationToken ct)
        => Estado(await torcida.VotarEnqueteAsync(slug, enqueteId, dto, ct));

    /// <summary>Lista as mensagens do mural (mais recentes primeiro).</summary>
    [HttpGet("mensagens")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<MensagemDto>>> Mensagens(string slug, [FromQuery] DateTime? desde, CancellationToken ct)
    {
        var mensagens = await torcida.ListarMensagensAsync(slug, desde, ct);
        return mensagens is null ? NotFound() : Ok(mensagens);
    }

    /// <summary>Publica uma mensagem no mural. Exige evento ao vivo; aplica rate limit por torcedor.</summary>
    [HttpPost("mensagens")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Enviar(string slug, [FromBody] EnviarMensagemDto dto, CancellationToken ct)
    {
        var r = await torcida.EnviarMensagemAsync(slug, dto, ct);
        return r.Status switch
        {
            StatusInteracao.Ok => CreatedAtAction(nameof(Mensagens), new { slug }, r.Dados),
            StatusInteracao.NaoEncontrado => NotFound(),
            StatusInteracao.NaoAoVivo => Conflict(Erro("A interação só está disponível com o jogo ao vivo.")),
            StatusInteracao.SemTorcedor => BadRequest(Erro("Informe o cabeçalho X-Torcedor-Id.")),
            StatusInteracao.LimiteExcedido => StatusCode(StatusCodes.Status429TooManyRequests, Erro("Aguarde alguns segundos antes de enviar outra mensagem.")),
            _ => UnprocessableEntity(Erro("Mensagem inválida."))
        };
    }

    private IActionResult Estado(ResultadoInteracao<TorcidaEstadoDto> r) => r.Status switch
    {
        StatusInteracao.Ok => Ok(r.Dados),
        StatusInteracao.NaoEncontrado => NotFound(),
        StatusInteracao.NaoAoVivo => Conflict(Erro("A interação só está disponível com o jogo ao vivo.")),
        StatusInteracao.SemTorcedor => BadRequest(Erro("Informe o cabeçalho X-Torcedor-Id.")),
        _ => UnprocessableEntity(Erro("Dados inválidos."))
    };

    private static object Erro(string mensagem) => new { mensagem };
}
