using System.ComponentModel.DataAnnotations;

namespace HubEsportesLages.Application.DTOs;

/// <summary>Dados enviados pelo torcedor ao se inscrever para receber notificações.</summary>
public class CriarInscricaoDto
{
    [Required(ErrorMessage = "Informe seu nome.")]
    [StringLength(120, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe um e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Telefone inválido.")]
    public string? Telefone { get; set; }

    /// <summary>Opcional: inscrever-se apenas em uma modalidade.</summary>
    public int? ModalidadeId { get; set; }

    /// <summary>Opcional: inscrever-se apenas em uma equipe.</summary>
    public int? EquipeId { get; set; }

    public bool ReceberEmail { get; set; } = true;
    public bool ReceberPush { get; set; } = true;
}

/// <summary>Representação de uma inscrição já registrada.</summary>
public record InscricaoDto(
    int Id,
    string Nome,
    string Email,
    string? ModalidadeNome,
    string? EquipeNome,
    bool ReceberEmail,
    bool ReceberPush,
    DateTime CriadoEm);
