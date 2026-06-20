using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Domain.Enums;

namespace HubEsportesLages.Application.Interfaces;

/// <summary>Feed central de notificações do hub e geração de lembretes.</summary>
public interface INotificacaoService
{
    Task<IReadOnlyList<NotificacaoDto>> ListarRecentesAsync(int quantidade = 20, CancellationToken ct = default);

    Task<int> ContarNaoLidasAsync(CancellationToken ct = default);

    Task<NotificacaoDto> PublicarAsync(
        string titulo,
        string mensagem,
        TipoNotificacao tipo,
        int? eventoId = null,
        int? modalidadeId = null,
        bool importante = false,
        CancellationToken ct = default);

    Task MarcarTodasComoLidasAsync(CancellationToken ct = default);

    /// <summary>
    /// Varre os eventos próximos e cria lembretes ainda não publicados.
    /// Retorna quantos lembretes foram gerados (usado pelo serviço em background).
    /// </summary>
    Task<int> GerarLembretesAsync(CancellationToken ct = default);
}
