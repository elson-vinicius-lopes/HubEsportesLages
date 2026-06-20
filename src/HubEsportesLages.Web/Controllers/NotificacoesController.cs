using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HubEsportesLages.Web.Controllers;

public class NotificacoesController(
    INotificacaoService notificacoes,
    ICatalogoService catalogo,
    IInscricaoService inscricoes) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = await MontarViewModelAsync(new CriarInscricaoDto(), ct);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inscrever(CriarInscricaoDto inscricao, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var vm = await MontarViewModelAsync(inscricao, ct);
            return View(nameof(Index), vm);
        }

        var registrada = await inscricoes.InscreverAsync(inscricao, ct);
        TempData["InscricaoOk"] =
            $"Pronto, {registrada.Nome.Split(' ')[0]}! Você receberá as notificações do Hub Esportes Lages.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarLidas(CancellationToken ct)
    {
        await notificacoes.MarcarTodasComoLidasAsync(ct);
        return RedirectToAction(nameof(Index));
    }

    private async Task<NotificacoesViewModel> MontarViewModelAsync(CriarInscricaoDto inscricao, CancellationToken ct) => new()
    {
        Notificacoes = await notificacoes.ListarRecentesAsync(30, ct),
        Modalidades = await catalogo.ListarModalidadesAsync(ct),
        Equipes = await catalogo.ListarEquipesAsync(ct: ct),
        Inscricao = inscricao,
        TotalInscritos = await inscricoes.ContarAtivasAsync(ct)
    };
}
