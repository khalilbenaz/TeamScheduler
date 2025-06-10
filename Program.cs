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

// Services m�tier
builder.Services.AddScoped<PlanningService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<NotificationService>();

// Configuration de la base de donn�es
builder.Services.AddDbContext<PlanningDbContext>(options =>
    options.UseSqlite("Data Source=planning.db"));

// Configuration CORS si n�cessaire
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

// Cr�ation automatique de la base de donn�es
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PlanningDbContext>();
    try
    {
        context.Database.EnsureCreated();
        Log.Information("Base de donn�es initialis�e avec succ�s");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Erreur lors de l'initialisation de la base de donn�es");
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

// Middleware de logging des requ�tes
app.UseSerilogRequestLogging();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

try
{
    Log.Information("D�marrage de l'application Planning Pr�sence");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Erreur fatale lors du d�marrage de l'application");
}
finally
{
    Log.CloseAndFlush();
}