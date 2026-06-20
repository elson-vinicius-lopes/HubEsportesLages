namespace HubEsportesLages.Application.Interfaces;

/// <summary>Contrato para envio de e-mails via provedor externo (Resend).</summary>
public interface IEmailService
{
    /// <summary>
    /// Envia um e-mail HTML para o destinatário informado.
    /// Se <paramref name="para"/> for null, usa o destinatário padrão configurado.
    /// </summary>
    Task EnviarAsync(string assunto, string corpoHtml, string? para = null, CancellationToken ct = default);
}
