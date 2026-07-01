using Microsoft.AspNetCore.Mvc;

namespace HubEsportesLages.Web.Controllers;

/// <summary>
/// Política de Privacidade do Bora pro Jogo (LGPD — Lei 13.709/2018).
/// Página pública e estática; a versão vigente está em LgpdConstantes.VersaoPoliticaAtual.
/// </summary>
public class PrivacidadeController : Controller
{
    [HttpGet("privacidade")]
    public IActionResult Index() => View();
}
