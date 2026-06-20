using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HubEsportesLages.Web.Controllers;

/// <summary>
/// Área de gestão (organizadores/Fundação Municipal de Esportes) para publicar
/// novos eventos na agenda. Em produção ficaria atrás de autenticação.
/// </summary>
public class AdminController(IEventoService eventos, ICatalogoService catalogo) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = await MontarViewModelAsync(new CriarEventoDto
        {
            Inicio = DateTime.Today.AddDays(1).AddHours(20)
        }, ct);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarEventoDto evento, CancellationToken ct)
    {
        if (evento.EquipeCasaId.HasValue && evento.EquipeCasaId == evento.EquipeVisitanteId)
            ModelState.AddModelError(nameof(evento.EquipeVisitanteId), "As equipes mandante e visitante devem ser diferentes.");

        if (!ModelState.IsValid)
        {
            var vm = await MontarViewModelAsync(evento, ct);
            return View(nameof(Index), vm);
        }

        var id = await eventos.CriarAsync(evento, ct);
        TempData["EventoOk"] = $"Evento publicado com sucesso na agenda (#{id}).";
        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminViewModel> MontarViewModelAsync(CriarEventoDto evento, CancellationToken ct) => new()
    {
        Evento = evento,
        Modalidades = await catalogo.ListarModalidadesAsync(ct),
        Locais = await catalogo.ListarLocaisAsync(ct),
        Equipes = await catalogo.ListarEquipesAsync(ct: ct),
        Proximos = await eventos.ListarProximosAsync(8, ct)
    };
}
