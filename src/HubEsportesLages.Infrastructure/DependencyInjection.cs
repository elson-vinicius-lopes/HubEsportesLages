using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Infrastructure.Email;
using HubEsportesLages.Infrastructure.Persistence;
using HubEsportesLages.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HubEsportesLages.Infrastructure;

/// <summary>Registra o acesso a dados e os serviços de aplicação no contêiner de DI.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, IConfiguration configuration)
    {
        services.AddDbContext<HubDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IEventoService, EventoService>();
        services.AddScoped<ICatalogoService, CatalogoService>();
        services.AddScoped<IInscricaoService, InscricaoService>();
        services.AddScoped<INotificacaoService, NotificacaoService>();
        services.AddScoped<ITorcidaService, TorcidaService>();
        services.AddScoped<IModeracaoService, ModeracaoService>();

        // Resend — envio de e-mails nas notificações.
        services.Configure<ResendSettings>(configuration.GetSection(ResendSettings.SectionName));
        services.AddScoped<IEmailService, ResendEmailService>();

        return services;
    }

    /// <summary>Cria o banco (se necessário) e popula com o cenário demonstrativo.</summary>
    public static async Task InicializarBancoAsync(this IServiceProvider provider, CancellationToken ct = default)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        await db.Database.EnsureCreatedAsync(ct);
        await DataSeeder.SeedAsync(db, ct);
    }
}
