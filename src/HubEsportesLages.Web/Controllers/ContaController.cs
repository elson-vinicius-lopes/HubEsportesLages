using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HubEsportesLages.Web.Controllers;

/// <summary>
/// Autenticação do torcedor (usuário comum). Login, registro e logout.
/// </summary>
public class ContaController(
    IEmailService emailService,
    IEventoService eventos,
    ICatalogoService catalogo) : Controller
{
    // E-mails com acesso de administrador (hackathon). Mover para configuração/banco depois.
    private static readonly string[] AdminsConhecidos = { "elsouzalopes@gmail.com" };

    [HttpGet("conta")]
    [Authorize]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = new ContaViewModel
        {
            Modalidades = await catalogo.ListarModalidadesAsync(ct),
            Equipes = await catalogo.ListarEquipesAsync(ct: ct),
            Proximos = await eventos.ListarProximosAsync(8, ct),
            Metricas = await eventos.ObterMetricasDashboardAsync(ct),
            Inscricao = new CriarInscricaoDto
            {
                Nome = User.Identity?.Name ?? string.Empty,
                Email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty
            }
        };

        return View(vm);
    }

    [HttpGet("conta/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(Index));

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("conta/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        // TODO: Validar contra o banco quando a entidade Usuario for criada.
        // Por enquanto, aceita qualquer usuário com senha >= 6 caracteres para demonstração.
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            ModelState.AddModelError(string.Empty, "Usuário ou senha incorretos.");
            return View();
        }

        var alvo = username.Trim();
        var ehAdmin = Array.Exists(AdminsConhecidos, a => string.Equals(a, alvo, StringComparison.OrdinalIgnoreCase));

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, ehAdmin ? "Admin" : "Torcedor")
        };

        // Se o username parecer um e-mail, adicionamos a claim de e-mail
        if (username.Contains('@'))
        {
            claims.Add(new Claim(ClaimTypes.Email, username));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("conta/registrar")]
    public IActionResult Registrar()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(Index));

        return View();
    }

    [HttpPost("conta/registrar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(string nome, string email, string senha, string confirmarSenha, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(nome))
            ModelState.AddModelError(string.Empty, "Informe seu nome completo.");

        if (string.IsNullOrWhiteSpace(email))
            ModelState.AddModelError(string.Empty, "Informe um e-mail válido.");

        if (string.IsNullOrWhiteSpace(senha) || senha.Length < 6)
            ModelState.AddModelError(string.Empty, "A senha deve ter pelo menos 6 caracteres.");

        if (senha != confirmarSenha)
            ModelState.AddModelError(string.Empty, "As senhas não coincidem.");

        if (!ModelState.IsValid)
            return View();

        // TODO: Persistir o usuário no banco quando a entidade Usuario for criada.

        // Envia e-mail de boas-vindas.
        var corpoHtml = $"""
            <div style="font-family: 'Segoe UI', Arial, sans-serif; max-width: 520px; margin: 0 auto; background: #0b2545; color: #fff; border-radius: 12px; padding: 28px;">
                <h2 style="margin: 0 0 12px; color: #22c55e;">🎉 Bem-vindo(a), {nome}!</h2>
                <p style="font-size: 1rem; color: #cbd5e1; line-height: 1.6;">Sua conta no <strong>Bora pro Jogo</strong> foi criada com sucesso.</p>
                <p style="font-size: 1rem; color: #cbd5e1; line-height: 1.6;">Agora você pode acompanhar eventos, receber notificações e torcer pelas equipes de Lages/SC! 🏟️</p>
                <hr style="border: none; border-top: 1px solid rgba(255,255,255,0.15); margin: 20px 0;" />
                <p style="font-size: 0.82rem; color: #64748b;">Bora pro Jogo · Agenda esportiva de Lages/SC</p>
            </div>
            """;

        await emailService.EnviarAsync("🎉 Bem-vindo ao Bora pro Jogo!", corpoHtml, email, ct);

        TempData["RegistroOk"] = $"Conta criada com sucesso para {nome}! Faça login para continuar.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("conta/logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
