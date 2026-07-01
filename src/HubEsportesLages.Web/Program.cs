using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Infrastructure;
using HubEsportesLages.Infrastructure.Identidade;
using HubEsportesLages.Web.BackgroundJobs;
using HubEsportesLages.Web.Identidade;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Text;

// Npgsql: mantém o comportamento legado de timestamp (aceita DateTime Local/Unspecified
// em colunas timestamptz). Necessário porque o DataSeeder usa DateTime.Now/Today.
// Deve ficar no topo, antes de qualquer uso do provider (CreateBuilder/Serilog).
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Configura Serilog: console + arquivo diário na pasta logs/.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine("logs", "hub-esportes-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// MVC (site) + API REST (controllers com [ApiController]).
builder.Services.AddControllersWithViews();

// Camada de dados e serviços de aplicação (PostgreSQL). Registra também o ASP.NET Core
// Identity (usuários, roles persistidas e o cookie de aplicação).
var connectionString = builder.Configuration.GetConnectionString("Default")
                       ?? "Host=localhost;Port=5432;Database=hubesportes;Username=postgres;Password=hub";
builder.Services.AddInfrastructure(connectionString, builder.Configuration);

// Ajusta o cookie de autenticação do Identity: rotas de login/acesso negado e
// redirecionamento separado para a área do organizador (/admin).
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/conta/login";
    options.LogoutPath = "/conta/logout";
    options.AccessDeniedPath = "/conta/login";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    // Requisições com "Authorization: Bearer" são encaminhadas ao esquema JWT — assim os
    // [Authorize(Roles="Admin")] da API funcionam por cookie (site) E por JWT (mobile).
    options.ForwardDefaultSelector = context =>
        context.Request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? JwtBearerDefaults.AuthenticationScheme
            : null;
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            // Chamadas de API não autenticadas recebem 401 (JSON/fetch), nunca o HTML de login —
            // senão o fetch do torcida.js interpretaria a página de login (200) como sucesso.
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            var returnUrl = context.Request.Path + context.Request.QueryString;
            if (context.Request.Path.StartsWithSegments("/admin"))
            {
                context.Response.Redirect("/admin/login?returnUrl=" + Uri.EscapeDataString(returnUrl));
            }
            else
            {
                context.Response.Redirect("/conta/login?returnUrl=" + Uri.EscapeDataString(returnUrl));
            }
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            // Mesma regra para falta de permissão em /api: responde 403 em vez de redirecionar.
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            if (context.Request.Path.StartsWithSegments("/admin"))
            {
                context.Response.Redirect("/admin/login");
            }
            else
            {
                context.Response.Redirect("/conta/login");
            }
            return Task.CompletedTask;
        }
    };
});

// Autenticação JWT Bearer para a API REST (consumo pelo app mobile Arena Lages).
// ADICIONA o esquema JWT SEM trocar o padrão do Identity (cookie do site MVC).
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

// SecretKey: em produção deve vir de env 'Jwt__SecretKey'. Em Development, se vazia, usa um
// fallback longo SÓ em dev (com aviso). Em produção, a ausência de chave é um erro de configuração.
if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
{
    if (builder.Environment.IsDevelopment())
    {
        jwtSettings.SecretKey = "DEV-Jwt-SecretKey-BoraProJogo-troque-em-producao-via-env-Jwt__SecretKey";
        Log.Warning(
            "Jwt:SecretKey vazia — usando FALLBACK DE DEV (apenas em Development). Configure 'Jwt__SecretKey' (>= {Min} chars) via variável de ambiente em produção.",
            JwtSettings.TamanhoMinimoChave);
    }
    else
    {
        throw new InvalidOperationException(
            "Jwt:SecretKey não configurada. Defina a variável de ambiente 'Jwt__SecretKey' (>= 32 caracteres) em produção.");
    }
}

if (Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < JwtSettings.TamanhoMinimoChave)
{
    throw new InvalidOperationException(
        $"Jwt:SecretKey muito curta: são exigidos ao menos {JwtSettings.TamanhoMinimoChave} caracteres para HMAC-SHA256.");
}

// Disponibiliza as configs (já com a SecretKey resolvida) para o AuthApiController gerar o token.
builder.Services.AddSingleton(jwtSettings);

var chaveAssinatura = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = chaveAssinatura,
            // Tolerância de relógio curta (padrão do framework são 5 min).
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

// Política padrão dupla: os [Authorize] existentes na API passam a aceitar OS DOIS esquemas —
// cookie do Identity (site MVC) e JWT Bearer (app mobile). Roles seguem funcionando.
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(
            IdentityConstants.ApplicationScheme,
            JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});

// Identidade anônima do torcedor (cabeçalho X-Torcedor-Id) usada pela interação da torcida.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITorcedorContexto, TorcedorContexto>();

// Geração automática de lembretes para os eventos próximos.
builder.Services.AddHostedService<NotificacaoLembreteWorker>();

// Documentação da API (Swagger / OpenAPI).
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Bora pro Jogo — API",
        Version = "v1",
        Description = "API pública da central de agenda e notificações dos esportes de Lages/SC."
    });

    // Esquema Bearer: permite testar os endpoints protegidos com o token do POST /api/auth/login.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe apenas o token JWT (obtido em POST /api/auth/login). O 'Bearer ' é adicionado automaticamente."
    });
    options.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", documento),
            new List<string>()
        }
    });
});

var app = builder.Build();

// Cria e popula o banco de demonstração na inicialização.
await app.Services.InicializarBancoAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Swagger disponível em /swagger (inclusive em produção para a demonstração).
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Bora pro Jogo — API v1");
    options.DocumentTitle = "Bora pro Jogo — API";
});

app.MapStaticAssets();

// Resolve a identidade anônima do torcedor (X-Torcedor-Id) antes das rotas de API.
app.UseMiddleware<TorcedorIdentidadeMiddleware>();

// Rotas de API (atributos) + rota MVC padrão.
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação encerrada inesperadamente.");
}
finally
{
    Log.CloseAndFlush();
}
