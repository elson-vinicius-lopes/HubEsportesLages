namespace HubEsportesLages.Application.DTOs;

/// <summary>Janela temporal usada para filtrar a agenda.</summary>
public enum PeriodoAgenda
{
    Proximos = 0,
    Hoje = 1,
    Semana = 2,
    Mes = 3,
    Todos = 4
}

/// <summary>Critérios de filtragem/paginação da agenda esportiva.</summary>
public class AgendaFiltro
{
    /// <summary>Filtra por modalidade (slug, ex.: "futsal"). Vazio = todas.</summary>
    public string? Modalidade { get; set; }

    public int? EquipeId { get; set; }

    public int? LocalId { get; set; }

    /// <summary>Busca textual por título, campeonato ou equipe.</summary>
    public string? Busca { get; set; }

    public PeriodoAgenda Periodo { get; set; } = PeriodoAgenda.Proximos;

    public bool ApenasGratuitos { get; set; }

    public int Pagina { get; set; } = 1;

    public int TamanhoPagina { get; set; } = 9;

    public int PaginaNormalizada => Pagina < 1 ? 1 : Pagina;

    public int TamanhoNormalizado => TamanhoPagina is < 1 or > 48 ? 9 : TamanhoPagina;
}
