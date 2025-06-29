using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using PlanningPresenceBlazor.Data;
using PlanningPresenceBlazor.Services;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configuration de Serilog pour le logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        "Logs/planning-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Configuration de la base de données SQLite
builder.Services.AddDbContext<PlanningDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=planning.db"));

// Anciens services (migration progressive)
builder.Services.AddScoped<TeamPlanningService>();
builder.Services.AddScoped<PlanningService>();
builder.Services.AddScoped<SchedulerService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<NotificationService>();

// Nouvelle architecture - Repositories
builder.Services.AddScoped<TeamScheduler.Core.Interfaces.ITeamRepository, TeamScheduler.Infrastructure.Repositories.TeamRepository>();
builder.Services.AddScoped<TeamScheduler.Core.Interfaces.IEmployeeRepository, TeamScheduler.Infrastructure.Repositories.EmployeeRepository>();
builder.Services.AddScoped<TeamScheduler.Core.Interfaces.ITeamClientAssignmentRepository, TeamScheduler.Infrastructure.Repositories.TeamClientAssignmentRepository>();
builder.Services.AddScoped<TeamScheduler.Core.Interfaces.ITeamProjectAssignmentRepository, TeamScheduler.Infrastructure.Repositories.TeamProjectAssignmentRepository>();

// Nouvelle architecture - Services
builder.Services.AddScoped<TeamScheduler.Application.Services.ITeamService, TeamScheduler.Application.Services.TeamService>();

// Configuration des options
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<SMSOptions>(builder.Configuration.GetSection("SMS"));
builder.Services.Configure<TeamsOptions>(builder.Configuration.GetSection("Teams"));
builder.Services.Configure<PlanningOptions>(builder.Configuration.GetSection("Planning"));

// HttpClient pour les notifications
builder.Services.AddHttpClient();

// Localisation française
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "fr-FR" };
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Localisation
app.UseRequestLocalization();

// Serilog request logging
app.UseSerilogRequestLogging();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Initialisation de la base de données
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PlanningDbContext>();
        
        // Créer la base de données si elle n'existe pas
        await dbContext.Database.EnsureCreatedAsync();
        
        // Appliquer les migrations
        if (dbContext.Database.GetPendingMigrations().Any())
        {
            await dbContext.Database.MigrateAsync();
        }
        
        // Initialiser les données de test en développement
        if (app.Environment.IsDevelopment())
        {
            await dbContext.InitializeTestDataAsync();
        }
        
        Log.Information("Base de données initialisée avec succès");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Erreur lors de l'initialisation de la base de données");
        throw;
    }
}

// Seed initial data for testing
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PlanningDbContext>();
    PlanningPresenceBlazor.DataTools.TestDataSeeder.SeedPresences(context);
}

Log.Information("Application démarrée");

app.Run();
