using HubEsportesLages.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HubEsportesLages.Infrastructure.Email;

/// <summary>
/// Implementação de <see cref="IEmailService"/> para desenvolvimento: em vez de enviar,
/// registra o e-mail no log (destinatário, assunto e tamanho do corpo). Assim os e-mails
/// ficam visíveis no console — sem depender de provedor externo nem expor segredos.
/// Em produção, troque por <c>Email:Provedor=Resend</c> (ou um adaptador SMTP).
/// </summary>
public class LogEmailService(ILogger<LogEmailService> logger) : IEmailService
{
    public Task EnviarAsync(string assunto, string corpoHtml, string? para = null, CancellationToken ct = default)
    {
        logger.LogInformation(
            "[E-MAIL/log] Para: {Destino} | Assunto: {Assunto} | Corpo: {Tamanho} chars (provedor 'Log' — não enviado de verdade)",
            string.IsNullOrWhiteSpace(para) ? "(destinatário padrão)" : para,
            assunto,
            corpoHtml?.Length ?? 0);

        return Task.CompletedTask;
    }
}
