using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HubEsportesLages.Web.Controllers;

[Authorize]
public class NotificacoesController(
    INotificacaoService notificacoes,
    ICatalogoService catalogo,
    IInscricaoService inscricoes,
    IEmailService emailService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = await MontarViewModelAsync(new CriarInscricaoDto(), ct);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inscrever(CriarInscricaoDto inscricao, string? returnUrl = null, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            var vm = await MontarViewModelAsync(inscricao, ct);
            return View(nameof(Index), vm);
        }

        var registrada = await inscricoes.InscreverAsync(inscricao, ct);

        // Envia e-mail de confirmação de inscrição.
        var corpoHtml = $"""
            <div style="font-family: 'Segoe UI', Arial, sans-serif; max-width: 520px; margin: 0 auto; background: #0b2545; color: #fff; border-radius: 12px; padding: 28px;">
                <h2 style="margin: 0 0 12px; color: #22c55e;">🔔 Inscrição confirmada!</h2>
                <p style="font-size: 1rem; color: #cbd5e1; line-height: 1.6;">Olá, <strong>{registrada.Nome.Split(' ')[0]}</strong>! Você agora receberá as notificações do <strong>Bora pro Jogo</strong>.</p>
                <p style="font-size: 1rem; color: #cbd5e1; line-height: 1.6;">Fique ligado(a) nos próximos eventos esportivos de Lages/SC. 🏟️</p>
                <hr style="border: none; border-top: 1px solid rgba(255,255,255,0.15); margin: 20px 0;" />
                <p style="font-size: 0.82rem; color: #64748b;">Bora pro Jogo · Agenda esportiva de Lages/SC</p>
            </div>
            """;

        await emailService.EnviarAsync("🔔 Inscrição confirmada — Bora pro Jogo!", corpoHtml, registrada.Email, ct);

        TempData["InscricaoOk"] =
            $"Pronto, {registrada.Nome.Split(' ')[0]}! Você receberá as notificações do Bora pro Jogo.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

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
