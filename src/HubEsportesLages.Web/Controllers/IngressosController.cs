using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HubEsportesLages.Web.Controllers;

/// <summary>
/// Telas web do ingresso digital (cliente do fluxo da Fase 1): compra/pagamento com QR
/// Pix simulado, "Meus ingressos" e o scanner de validação do admin.
/// </summary>
[Authorize]
public class IngressosController(IIngressoService ingressos, IEventoService eventos) : Controller
{
    /// <summary>Inicia a compra de um evento pago e mostra o QR Pix (simulado).</summary>
    [HttpGet("Ingressos/Comprar/{eventoId:int}")]
    public async Task<IActionResult> Comprar(int eventoId, CancellationToken ct)
    {
        var evento = await eventos.ObterPorIdAsync(eventoId, ct);
        if (evento is null)
            return NotFound();

        if (evento.Gratuito)
        {
            TempData["IngressoErro"] = "Este evento é de entrada gratuita — não há ingresso a comprar.";
            return RedirectToAction("Evento", "Agenda", new { slug = evento.Slug });
        }

        var comprador = User.Identity!.Name!;
        var pix = await ingressos.ComprarAsync(eventoId, comprador, comprador, ct);
        if (pix is null)
            return NotFound();

        var vm = new PagamentoIngressoViewModel
        {
            Pagamento = pix,
            EventoTitulo = evento.Titulo,
            EventoSlug = evento.Slug
        };
        return View("Pagamento", vm);
    }

    /// <summary>Confirma o pagamento (simulado) e exibe o ingresso emitido com o QR.</summary>
    [HttpPost("Ingressos/Confirmar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmar(int id, CancellationToken ct)
    {
        var comprador = User.Identity!.Name!;
        var emitido = await ingressos.ConfirmarPagamentoAsync(id, comprador, ct);
        if (emitido is null)
            return NotFound();

        return View("Emitido", emitido);
    }

    /// <summary>Lista os ingressos do torcedor logado.</summary>
    [HttpGet("Ingressos")]
    public async Task<IActionResult> Meus(CancellationToken ct)
    {
        var comprador = User.Identity!.Name!;
        var lista = await ingressos.ListarMeusAsync(comprador, ct);
        return View("Meus", lista);
    }

    /// <summary>Scanner de validação na entrada (somente Admin).</summary>
    [HttpGet("Ingressos/Scanner")]
    [Authorize(Roles = "Admin")]
    public IActionResult Scanner() => View("Scanner");
}
