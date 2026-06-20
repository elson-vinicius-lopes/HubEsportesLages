using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Infrastructure;
using HubEsportesLages.Web.BackgroundJobs;
using HubEsportesLages.Web.Identidade;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// MVC (site) + API REST (controllers com [ApiController]).
builder.Services.AddControllersWithViews();

// Camada de dados e serviços de aplicação (SQLite).
var connectionString = builder.Configuration.GetConnectionString("Default")
                       ?? "Data Source=hubesportes.db";
builder.Services.AddInfrastructure(connectionString);

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
