using HubEsportesLages.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HubEsportesLages.Web.Controllers;

/// <summary>
/// Tela de interação da torcida (cliente fino sobre a API REST da Fase 1).
/// O controller só monta o cabeçalho do evento; os dados de votação chegam via JS
/// consumindo <c>/api/eventos/{slug}/torcida</c>.
/// </summary>
public class TorcidaController(IEventoService eventos) : Controller
{
    [HttpGet("Torcida/Evento/{slug}")]
    public async Task<IActionResult> Index(string slug, CancellationToken ct)
    {
        var evento = await eventos.ObterPorSlugAsync(slug, ct);
        if (evento is null)
            return NotFound();

        return View(evento);
    }
}
