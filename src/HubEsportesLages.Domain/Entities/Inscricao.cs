namespace HubEsportesLages.Domain.Entities;

/// <summary>
/// Inscrição de um torcedor para receber notificações do hub.
/// A inscrição pode ser geral (toda a agenda), por modalidade ou por equipe.
/// </summary>
public class Inscricao
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Telefone { get; set; }

    /// <summary>Quando preenchido, restringe as notificações a uma modalidade.</summary>
    public int? ModalidadeId { get; set; }
    public Modalidade? Modalidade { get; set; }

    /// <summary>Quando preenchido, restringe as notificações a uma equipe.</summary>
    public int? EquipeId { get; set; }
    public Equipe? Equipe { get; set; }

    public bool ReceberEmail { get; set; } = true;
    public bool ReceberPush { get; set; } = true;

    public bool Ativa { get; set; } = true;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
