using HubEsportesLages.Application.Common;
using HubEsportesLages.Application.DTOs;

namespace HubEsportesLages.Web.Models;

/// <summary>Dados da página inicial (hero, destaques, próximos jogos e feed).</summary>
public class HomeViewModel
{
    public IReadOnlyList<EventoResumoDto> Destaques { get; init; } = [];
    public IReadOnlyList<EventoResumoDto> AoVivo { get; init; } = [];
    public IReadOnlyList<EventoResumoDto> Proximos { get; init; } = [];
    public IReadOnlyList<ModalidadeDto> Modalidades { get; init; } = [];
    public IReadOnlyList<NotificacaoDto> Notificacoes { get; init; } = [];
    public int TotalInscritos { get; init; }
    public int TotalEventosFuturos { get; init; }
}

/// <summary>Página de agenda/resultados com filtros e paginação.</summary>
public class AgendaViewModel
{
    public AgendaFiltro Filtro { get; init; } = new();
    public PagedResult<EventoResumoDto> Eventos { get; init; } = new([], 1, 9, 0);
    public IReadOnlyList<ModalidadeDto> Modalidades { get; init; } = [];
    public IReadOnlyList<LocalDto> Locais { get; init; } = [];
    public string Titulo { get; init; } = "Agenda esportiva";
    public string Subtitulo { get; init; } = string.Empty;
    public bool ModoResultados { get; init; }
}

/// <summary>Página do feed de notificações + formulário de inscrição.</summary>
public class NotificacoesViewModel
{
    public IReadOnlyList<NotificacaoDto> Notificacoes { get; init; } = [];
    public IReadOnlyList<ModalidadeDto> Modalidades { get; init; } = [];
    public IReadOnlyList<EquipeDto> Equipes { get; init; } = [];
    public CriarInscricaoDto Inscricao { get; set; } = new();
    public int TotalInscritos { get; init; }
}

/// <summary>Área administrativa: criação de evento + últimos cadastrados.</summary>
public class AdminViewModel
{
    public CriarEventoDto Evento { get; set; } = new() { Inicio = DateTime.Today.AddDays(1).AddHours(20) };
    public IReadOnlyList<ModalidadeDto> Modalidades { get; init; } = [];
    public IReadOnlyList<LocalDto> Locais { get; init; } = [];
    public IReadOnlyList<EquipeDto> Equipes { get; init; } = [];
    public IReadOnlyList<EventoResumoDto> Proximos { get; init; } = [];
}
