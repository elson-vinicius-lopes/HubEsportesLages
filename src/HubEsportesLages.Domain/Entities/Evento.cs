using HubEsportesLages.Domain.Enums;

namespace HubEsportesLages.Domain.Entities;

/// <summary>
/// Núcleo do hub: um evento da agenda esportiva de Lages (jogo, rodada, corrida,
/// torneio). Pode ou não ser um confronto entre duas equipes.
/// </summary>
public class Evento
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    /// <summary>Nome do campeonato/competição (ex.: "Campeonato Citadino de Futsal 2026").</summary>
    public string Campeonato { get; set; } = string.Empty;

    public int ModalidadeId { get; set; }
    public Modalidade? Modalidade { get; set; }

    public int LocalId { get; set; }
    public Local? Local { get; set; }

    // Confronto (opcional — eventos como corridas não têm mandante/visitante).
    public int? EquipeCasaId { get; set; }
    public Equipe? EquipeCasa { get; set; }

    public int? EquipeVisitanteId { get; set; }
    public Equipe? EquipeVisitante { get; set; }

    public DateTime Inicio { get; set; }
    public DateTime? Fim { get; set; }

    public StatusEvento Status { get; set; } = StatusEvento.Agendado;

    public int? PlacarCasa { get; set; }
    public int? PlacarVisitante { get; set; }

    public string? ImagemUrl { get; set; }

    public bool Gratuito { get; set; } = true;
    public decimal? PrecoIngresso { get; set; }

    /// <summary>Quando verdadeiro, o evento aparece em destaque na home.</summary>
    public bool Destaque { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    public ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();

    /// <summary>Indica se o evento é um confronto direto entre duas equipes.</summary>
    public bool EhConfronto => EquipeCasaId.HasValue && EquipeVisitanteId.HasValue;

    /// <summary>Resultado pronto para exibição (ex.: "3 x 1") ou null se ainda não houver placar.</summary>
    public string? Placar =>
        PlacarCasa.HasValue && PlacarVisitante.HasValue
            ? $"{PlacarCasa} x {PlacarVisitante}"
            : null;
}
