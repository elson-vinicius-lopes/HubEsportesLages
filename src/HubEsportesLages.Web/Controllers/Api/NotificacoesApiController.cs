using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HubEsportesLages.Web.Controllers.Api;

/// <summary>Feed central de notificações do hub.</summary>
[ApiController]
[Route("api/notificacoes")]
[Produces("application/json")]
[Tags("Notificações")]
public class NotificacoesApiController(INotificacaoService notificacoes) : ControllerBase
{
    /// <summary>Lista as notificações mais recentes.</summary>
    [HttpGet]
    public async Task<IReadOnlyList<NotificacaoDto>> Listar([FromQuery] int quantidade = 20, CancellationToken ct = default) =>
        await notificacoes.ListarRecentesAsync(quantidade, ct);

    /// <summary>Quantidade de notificações ainda não lidas.</summary>
    [HttpGet("nao-lidas")]
    public async Task<object> NaoLidas(CancellationToken ct) =>
        new { naoLidas = await notificacoes.ContarNaoLidasAsync(ct) };

    /// <summary>Dispara a geração de lembretes para os eventos das próximas 24h.</summary>
    [HttpPost("gerar-lembretes")]
    public async Task<object> GerarLembretes(CancellationToken ct) =>
        new { gerados = await notificacoes.GerarLembretesAsync(ct) };
}
