using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using PlanningPresenceBlazor.Data;
using PlanningPresenceBlazor.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuration de Serilog pour les logs
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/planning-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configuration des services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Services métier
builder.Services.AddScoped<PlanningService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<NotificationService>();

// Configuration de la base de données
builder.Services.AddDbContext<PlanningDbContext>(options =>
    options.UseSqlite("Data Source=planning.db"));

// Configuration CORS si nécessaire
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Création automatique de la base de données
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PlanningDbContext>();
    try
    {
        context.Database.EnsureCreated();
        Log.Information("Base de données initialisée avec succès");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Erreur lors de l'initialisation de la base de données");
    }
}

// Configuration du pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();

// Middleware de logging des requêtes
app.UseSerilogRequestLogging();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

try
{
    Log.Information("Démarrage de l'application Planning Présence");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Erreur fatale lors du démarrage de l'application");
}
finally
{
    Log.CloseAndFlush();
}