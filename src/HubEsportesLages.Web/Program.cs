using HubEsportesLages.Infrastructure;
using HubEsportesLages.Web.BackgroundJobs;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// MVC (site) + API REST (controllers com [ApiController]).
builder.Services.AddControllersWithViews();

// Camada de dados e serviços de aplicação (SQLite).
var connectionString = builder.Configuration.GetConnectionString("Default")
                       ?? "Data Source=hubesportes.db";
builder.Services.AddInfrastructure(connectionString);

// Geração automática de lembretes para os eventos próximos.
builder.Services.AddHostedService<NotificacaoLembreteWorker>();

// Documentação da API (Swagger / OpenAPI).
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hub Esportes Lages — API",
        Version = "v1",
        Description = "API pública do hub central de agenda e notificações dos esportes de Lages/SC."
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
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hub Esportes Lages — API v1");
    options.DocumentTitle = "Hub Esportes Lages — API";
});

app.MapStaticAssets();

// Rotas de API (atributos) + rota MVC padrão.
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
