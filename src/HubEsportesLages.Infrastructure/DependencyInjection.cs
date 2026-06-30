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

        // Ingressos (QR + Pix simulado): token assinado, gateway mock e orquestração.
        services.AddSingleton<ITokenIngresso, TokenIngresso>();
        services.AddSingleton<IPagamentoService, MockPixPagamentoService>();
        services.AddScoped<IIngressoService, IngressoService>();

        // E-mail: por padrão registra no log (dev/visível, sem segredo). Use Resend só quando
        // Email:Provedor=Resend E a key estiver configurada (via variável de ambiente / user-secrets).
        services.Configure<ResendSettings>(configuration.GetSection(ResendSettings.SectionName));
        var provedorEmail = configuration["Email:Provedor"] ?? "Log";
        if (string.Equals(provedorEmail, "Resend", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(configuration["Resend:ApiKey"]))
        {
            services.AddScoped<IEmailService, ResendEmailService>();
        }
        else
        {
            services.AddScoped<IEmailService, LogEmailService>();
        }

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
