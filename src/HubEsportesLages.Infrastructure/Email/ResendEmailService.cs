using HubEsportesLages.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;

namespace HubEsportesLages.Infrastructure.Email;

/// <summary>Implementação de <see cref="IEmailService"/> usando a API do Resend.</summary>
public class ResendEmailService(
    IOptions<ResendSettings> options,
    ILogger<ResendEmailService> logger) : IEmailService
{
    private readonly ResendSettings _cfg = options.Value;

    public async Task EnviarAsync(string assunto, string corpoHtml, string? para = null, CancellationToken ct = default)
    {
        var destinatario = para ?? _cfg.DestinatarioPadrao;

        if (string.IsNullOrWhiteSpace(_cfg.ApiKey) || string.IsNullOrWhiteSpace(destinatario))
        {
            logger.LogWarning("Resend não configurado (ApiKey ou destinatário vazio). E-mail não enviado.");
            return;
        }

        try
        {
            var client = ResendClient.Create(_cfg.ApiKey);

            var remetente = string.IsNullOrWhiteSpace(_cfg.RemetenteNome)
                ? _cfg.RemetenteEmail
                : $"{_cfg.RemetenteNome} <{_cfg.RemetenteEmail}>";

            await client.EmailSendAsync(new EmailMessage
            {
                From = remetente,
                To = destinatario,
                Subject = assunto,
                HtmlBody = corpoHtml
            });

            logger.LogInformation("E-mail enviado via Resend para {Destinatario}: {Assunto}", destinatario, assunto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao enviar e-mail via Resend para {Destinatario}", destinatario);
        }
    }
}
