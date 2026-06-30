using HubEsportesLages.Domain.Enums;

namespace HubEsportesLages.Domain.Entities;

/// <summary>
/// Ingresso digital de um evento pago: comprado por um torcedor logado, pago via Pix
/// (simulado), emitido com um token assinado e validado pelo admin na entrada (uso único).
/// </summary>
public class Ingresso
{
    public int Id { get; set; }

    public int EventoId { get; set; }
    public Evento? Evento { get; set; }

    /// <summary>Identidade do comprador (e-mail/login do usuário logado).</summary>
    public string CompradorId { get; set; } = string.Empty;

    public string CompradorNome { get; set; } = string.Empty;

    /// <summary>Valor pago pelo ingresso (preço do evento no momento da compra).</summary>
    public decimal Preco { get; set; }

    public StatusIngresso Status { get; set; } = StatusIngresso.Pendente;

    /// <summary>Token assinado do ingresso — preenchido somente após o pagamento (Pago).</summary>
    public string? Token { get; set; }

    /// <summary>Identificador do "pagamento" Pix simulado.</summary>
    public string TxidPix { get; set; } = string.Empty;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public DateTime? PagoEm { get; set; }

    public DateTime? UtilizadoEm { get; set; }

    /// <summary>Admin que validou o ingresso na entrada.</summary>
    public string? ValidadoPor { get; set; }
}
