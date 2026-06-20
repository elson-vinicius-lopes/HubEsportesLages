using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HubEsportesLages.Web.Controllers;

public class ResultadosController(IEventoService eventos, ICatalogoService catalogo) : Controller
{
    public async Task<IActionResult> Index([FromQuery] AgendaFiltro filtro, CancellationToken ct)
    {
        filtro ??= new AgendaFiltro();
        filtro.Periodo = PeriodoAgenda.Todos; // resultados não usam janela temporal futura
        var lista = await eventos.ListarResultadosAsync(filtro, ct);

        var vm = new AgendaViewModel
        {
            Filtro = filtro,
            Eventos = lista,
            Modalidades = await catalogo.ListarModalidadesAsync(ct),
            Locais = await catalogo.ListarLocaisAsync(ct),
            Titulo = "Resultados",
            Subtitulo = "Placares e súmulas dos eventos já realizados.",
            ModoResultados = true
        };

        return View("~/Views/Agenda/Index.cshtml", vm);
    }
}
