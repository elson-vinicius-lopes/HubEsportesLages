using HubEsportesLages.Application.Interfaces;

namespace HubEsportesLages.Web.BackgroundJobs;

/// <summary>
/// Serviço em segundo plano que, periodicamente, gera lembretes para os eventos
/// que começam nas próximas 24h — alimentando o feed central de notificações.
/// </summary>
public class NotificacaoLembreteWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<NotificacaoLembreteWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Pequena espera para o banco já estar inicializado/populado.
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        using var timer = new PeriodicTimer(Intervalo);
        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var notificacoes = scope.ServiceProvider.GetRequiredService<INotificacaoService>();
                var gerados = await notificacoes.GerarLembretesAsync(stoppingToken);
                if (gerados > 0)
                    logger.LogInformation("Lembretes gerados: {Quantidade}", gerados);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao gerar lembretes de eventos.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
