using HubEsportesLages.Application.Common;
using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HubEsportesLages.Web.Controllers.Api;

/// <summary>Endpoints públicos da agenda esportiva de Lages.</summary>
[ApiController]
[Route("api/eventos")]
[Produces("application/json")]
[Tags("Eventos")]
public class EventosApiController(IEventoService eventos) : ControllerBase
{
    /// <summary>Lista a agenda de eventos (próximos por padrão), com filtros e paginação.</summary>
    [HttpGet]
    public Task<PagedResult<EventoResumoDto>> Listar([FromQuery] AgendaFiltro filtro, CancellationToken ct) =>
        eventos.ListarAgendaAsync(filtro ?? new AgendaFiltro(), ct);

    /// <summary>Lista os resultados de eventos já encerrados.</summary>
    [HttpGet("resultados")]
    public Task<PagedResult<EventoResumoDto>> Resultados([FromQuery] AgendaFiltro filtro, CancellationToken ct)
    {
        filtro ??= new AgendaFiltro();
        filtro.Periodo = PeriodoAgenda.Todos;
        return eventos.ListarResultadosAsync(filtro, ct);
    }

    /// <summary>Eventos em destaque para a home.</summary>
    [HttpGet("destaques")]
    public async Task<IReadOnlyList<EventoResumoDto>> Destaques(CancellationToken ct) =>
        await eventos.ListarDestaquesAsync(6, ct);

    /// <summary>Detalhe de um evento pelo seu slug.</summary>
    [HttpGet("{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventoDetalheDto>> Obter(string slug, CancellationToken ct)
    {
        var evento = await eventos.ObterPorSlugAsync(slug, ct);
        return evento is null ? NotFound() : Ok(evento);
    }

    /// <summary>Publica um novo evento na agenda.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarEventoDto dto, CancellationToken ct)
    {
        var id = await eventos.CriarAsync(dto, ct);
        var criado = await eventos.ObterPorIdAsync(id, ct);
        return CreatedAtAction(nameof(Obter), new { slug = criado!.Slug }, criado);
    }

    /// <summary>Atualiza o placar/encerramento de um evento.</summary>
    [HttpPut("{id:int}/resultado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarResultado(int id, [FromBody] AtualizarResultadoDto dto, CancellationToken ct)
    {
        var ok = await eventos.AtualizarResultadoAsync(id, dto, ct);
        return ok ? NoContent() : NotFound();
    }
}
