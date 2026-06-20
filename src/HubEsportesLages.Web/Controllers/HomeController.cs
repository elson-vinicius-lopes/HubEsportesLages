using System.Diagnostics;
using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Domain.Enums;
using HubEsportesLages.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HubEsportesLages.Web.Controllers;

public class HomeController(
    IEventoService eventos,
    ICatalogoService catalogo,
    INotificacaoService notificacoes,
    IInscricaoService inscricoes) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var destaques = await eventos.ListarDestaquesAsync(6, ct);
        var proximos = await eventos.ListarProximosAsync(6, ct);
        var modalidades = await catalogo.ListarModalidadesAsync(ct);
        var feed = await notificacoes.ListarRecentesAsync(6, ct);
        var totalInscritos = await inscricoes.ContarAtivasAsync(ct);

        var vm = new HomeViewModel
        {
            Destaques = destaques,
            AoVivo = proximos.Where(e => e.Status == StatusEvento.AoVivo).ToList(),
            Proximos = proximos,
            Modalidades = modalidades,
            Notificacoes = feed,
            TotalInscritos = totalInscritos,
            TotalEventosFuturos = modalidades.Sum(m => m.EventosFuturos)
        };

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
