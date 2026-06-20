using HubEsportesLages.Application.Common;
using HubEsportesLages.Application.DTOs;

namespace HubEsportesLages.Application.Interfaces;

/// <summary>Operações sobre os eventos da agenda esportiva.</summary>
public interface IEventoService
{
    /// <summary>Lista paginada de eventos aplicando o filtro informado.</summary>
    Task<PagedResult<EventoResumoDto>> ListarAgendaAsync(AgendaFiltro filtro, CancellationToken ct = default);

    /// <summary>Eventos já encerrados com placar, do mais recente para o mais antigo.</summary>
    Task<PagedResult<EventoResumoDto>> ListarResultadosAsync(AgendaFiltro filtro, CancellationToken ct = default);

    /// <summary>Eventos marcados como destaque para o carrossel da home.</summary>
    Task<IReadOnlyList<EventoResumoDto>> ListarDestaquesAsync(int quantidade = 5, CancellationToken ct = default);

    /// <summary>Próximos eventos a partir de agora (widget da home).</summary>
    Task<IReadOnlyList<EventoResumoDto>> ListarProximosAsync(int quantidade = 6, CancellationToken ct = default);

    Task<EventoDetalheDto?> ObterPorSlugAsync(string slug, CancellationToken ct = default);

    Task<EventoDetalheDto?> ObterPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Cria um evento e dispara a notificação de "novo evento". Retorna o id gerado.</summary>
    Task<int> CriarAsync(CriarEventoDto dto, CancellationToken ct = default);

    /// <summary>Atualiza placar/encerramento e publica a notificação de resultado.</summary>
    Task<bool> AtualizarResultadoAsync(int id, AtualizarResultadoDto dto, CancellationToken ct = default);
}
