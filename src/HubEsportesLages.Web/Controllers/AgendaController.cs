using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HubEsportesLages.Web.Controllers;

public class AgendaController(IEventoService eventos, ICatalogoService catalogo) : Controller
{
    public async Task<IActionResult> Index([FromQuery] AgendaFiltro filtro, CancellationToken ct)
    {
        filtro ??= new AgendaFiltro();
        var lista = await eventos.ListarAgendaAsync(filtro, ct);

        var vm = new AgendaViewModel
        {
            Filtro = filtro,
            Eventos = lista,
            Modalidades = await catalogo.ListarModalidadesAsync(ct),
            Locais = await catalogo.ListarLocaisAsync(ct),
            Titulo = "Agenda esportiva de Lages",
            Subtitulo = "Todos os jogos, corridas e competições da cidade em um só lugar."
        };

        return View(vm);
    }

    [HttpGet("Agenda/Evento/{slug}")]
    public async Task<IActionResult> Evento(string slug, CancellationToken ct)
    {
        var evento = await eventos.ObterPorSlugAsync(slug, ct);
        if (evento is null)
            return NotFound();

        return View(evento);
    }
}
