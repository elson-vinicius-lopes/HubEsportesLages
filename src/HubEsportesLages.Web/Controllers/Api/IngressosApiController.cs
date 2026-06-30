using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HubEsportesLages.Web.Controllers.Api;

/// <summary>
/// Ingressos digitais com QR-Code e Pix simulado: compra, confirmação de pagamento,
/// "meus ingressos" e validação na entrada (somente Admin).
/// </summary>
[ApiController]
[Route("api/ingressos")]
[Produces("application/json")]
[Tags("Ingressos")]
public class IngressosApiController(IIngressoService ingressos) : ControllerBase
{
    /// <summary>Compra um ingresso de um evento pago. Retorna a cobrança Pix (simulada) com QR. 400 se gratuito.</summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagamentoPixDto>> Comprar([FromBody] CriarIngressoDto dto, CancellationToken ct)
    {
        var comprador = User.Identity!.Name!;
        var pix = await ingressos.ComprarAsync(dto.EventoId, comprador, comprador, ct);
        return pix is null
            ? BadRequest(new { mensagem = "Evento inexistente ou gratuito — não há ingresso a vender." })
            : Ok(pix);
    }

    /// <summary>Confirma o pagamento (simulado) de um ingresso do próprio comprador. Retorna o ingresso emitido com QR.</summary>
    [HttpPost("{id:int}/confirmar-pagamento")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IngressoEmitidoDto>> ConfirmarPagamento(int id, CancellationToken ct)
    {
        var comprador = User.Identity!.Name!;
        var emitido = await ingressos.ConfirmarPagamentoAsync(id, comprador, ct);
        return emitido is null ? NotFound() : Ok(emitido);
    }

    /// <summary>Lista os ingressos do torcedor logado (mais recentes primeiro).</summary>
    [HttpGet("meus")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<IngressoDto>>> Meus(CancellationToken ct)
    {
        var comprador = User.Identity!.Name!;
        return Ok(await ingressos.ListarMeusAsync(comprador, ct));
    }

    /// <summary>Valida um ingresso na entrada (check-in). Somente Admin. Uso único idempotente.</summary>
    [HttpPost("validar")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ValidacaoResultadoDto>> Validar([FromBody] ValidarIngressoDto dto, CancellationToken ct)
    {
        var admin = User.Identity!.Name!;
        var resultado = await ingressos.ValidarAsync(dto.Token, admin, ct);
        return Ok(resultado);
    }
}
