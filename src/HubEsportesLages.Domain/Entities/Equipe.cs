namespace HubEsportesLages.Domain.Entities;

/// <summary>Equipe/associação esportiva local que disputa os eventos.</summary>
public class Equipe
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    /// <summary>Sigla curta usada no placar (ex.: "LAG").</summary>
    public string Sigla { get; set; } = string.Empty;

    public int ModalidadeId { get; set; }
    public Modalidade? Modalidade { get; set; }

    public string Cidade { get; set; } = "Lages";

    /// <summary>Emoji/escudo simbólico exibido no card do confronto.</summary>
    public string Escudo { get; set; } = string.Empty;

    public string CorPrimaria { get; set; } = "#1f6feb";
}
