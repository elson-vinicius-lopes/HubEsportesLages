namespace HubEsportesLages.Domain.Enums;

/// <summary>Ciclo de vida de um ingresso digital (compra → pagamento → uso na entrada).</summary>
public enum StatusIngresso
{
    /// <summary>Comprado, aguardando confirmação de pagamento (Pix).</summary>
    Pendente = 0,

    /// <summary>Pagamento confirmado; ingresso emitido com token e QR.</summary>
    Pago = 1,

    /// <summary>Já apresentado e validado na entrada (uso único).</summary>
    Utilizado = 2,

    /// <summary>Cancelado/estornado.</summary>
    Cancelado = 3
}
