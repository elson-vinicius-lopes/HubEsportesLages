namespace HubEsportesLages.Domain.Entities;

/// <summary>
/// Modalidade esportiva (Futebol, Futsal, Basquete, Vôlei, etc.).
/// É a principal dimensão de filtro/inscrição do hub.
/// </summary>
public class Modalidade
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    /// <summary>Identificador amigável usado na URL (ex.: "futsal").</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Emoji/ícone exibido nos cards e filtros.</summary>
    public string Icone { get; set; } = string.Empty;

    /// <summary>Cor de destaque (hex) usada na identidade visual da modalidade.</summary>
    public string CorHex { get; set; } = "#1f6feb";

    public string Descricao { get; set; } = string.Empty;

    public ICollection<Evento> Eventos { get; set; } = new List<Evento>();
    public ICollection<Equipe> Equipes { get; set; } = new List<Equipe>();
}
