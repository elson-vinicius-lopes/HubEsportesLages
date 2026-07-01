using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Infrastructure.Email;
using HubEsportesLages.Infrastructure.Identidade;
using HubEsportesLages.Infrastructure.Persistence;
using HubEsportesLages.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HubEsportesLages.Infrastructure;

/// <summary>Registra o acesso a dados e os serviços de aplicação no contêiner de DI.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, IConfiguration configuration)
    {
        services.AddDbContext<HubDbContext>(options => options.UseNpgsql(connectionString));

        // ASP.NET Core Identity sobre o HubDbContext (PostgreSQL), com roles persistidas.
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Política de senha forte obrigatória.
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;

                // Bloqueio por tentativas para mitigar força bruta.
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                // Login não exige confirmação de e-mail (cenário do Hub).
                options.SignIn.RequireConfirmedAccount = false;

                // E-mail único por conta.
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<HubDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IEventoService, EventoService>();
        services.AddScoped<ICatalogoService, CatalogoService>();
        services.AddScoped<IInscricaoService, InscricaoService>();
        services.AddScoped<INotificacaoService, NotificacaoService>();
        services.AddScoped<ITorcidaService, TorcidaService>();
        services.AddScoped<IModeracaoService, ModeracaoService>();

        // LGPD: export e exclusão dos dados vinculados ao titular (docs/specs/lgpd/).
        services.AddScoped<ILgpdService, LgpdService>();

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

    /// <summary>
    /// Aplica as migrations pendentes, garante a identidade (roles + admin) e
    /// popula o cenário demonstrativo.
    /// </summary>
    public static async Task InicializarBancoAsync(this IServiceProvider provider, CancellationToken ct = default)
    {
        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<HubDbContext>();
        await db.Database.MigrateAsync(ct);

        // Identidade: roles "Admin"/"Torcedor" e usuário administrador inicial.
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = sp.GetRequiredService<IConfiguration>();
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("IdentidadeSeeder");
        await IdentidadeSeeder.SeedAsync(userManager, roleManager, configuration, logger);

        await DataSeeder.SeedAsync(db, ct);
    }
}
