using Microsoft.EntityFrameworkCore;
using PortalCopa2026.Components;
using PortalCopa2026.Data;
using PortalCopa2026.Services.Grupos;
using PortalCopa2026.Services.Jogos;
using PortalCopa2026.Services.LandingPage;
using PortalCopa2026.Services.Ranking;
using PortalCopa2026.Services.Selecoes;
using PortalCopa2026.Services.Simulador;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Novos serviços (ex.: serviços de acesso a dados para landing page, jogos, grupos,
// seleções, ranking, simulador) devem ser registrados aqui via builder.Services,
// usando o DI nativo do ASP.NET Core (AddScoped/AddSingleton/AddTransient conforme o caso).
builder.Services.AddScoped<ILandingPageService, LandingPageService>();
builder.Services.AddScoped<IJogosService, JogosService>();
builder.Services.AddScoped<IGruposService, GruposService>();
builder.Services.AddScoped<ISimuladorService, SimuladorService>();
builder.Services.AddScoped<ISelecaoService, SelecaoService>();
builder.Services.AddScoped<IRankingService, RankingService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
    await SeedData.SeedAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
