using BuckingshireImporter;
using FamilyHub.DataImporter.Web.Data;
using FamilyHubs.DataImporter.Infrastructure;
using HounslowconnectImporter;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenActiveImporter;
using PlacecubeImporter;
using PluginBase;
using PublicPartnershipImporter;
using SalfordImporter;
using Serilog;
using Serilog.Events;
using SouthamptonImporter;
using SportEngland;

namespace FamilyHub.DataImporter.Web;

public static class StartupExtensions
{
    public static void ConfigureHost(this WebApplicationBuilder builder)
    {
        // ApplicationInsights
        builder.Host.UseSerilog((_, services, loggerConfiguration) =>
        {
            var logLevelString = builder.Configuration["LogLevel"];

            var parsed = Enum.TryParse<LogEventLevel>(logLevelString, out var logLevel);

            loggerConfiguration.WriteTo.Console(
                parsed ? logLevel : LogEventLevel.Warning);
        });
    }

    public static void RegisterApplicationComponents(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddSingleton<WeatherForecastService>();

        services.RegisterAppDbContext(configuration);

        services.AddTransient<DataImportApiService>();
    }

    public static IServiceCollection RegisterAppDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<ApplicationDbContextInitialiser>();
        var connectionString = configuration.GetConnectionString("PostCodeConnection");
        ArgumentException.ThrowIfNullOrEmpty(connectionString);

        var connection = new SqliteConnectionStringBuilder(connectionString).ToString();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite(connection, mg =>
                    mg.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.ToString()));
        });

        return services;
    }

    public static async Task ConfigureDb(this WebApplication app, IConfiguration configuration, Serilog.ILogger logger)
    {
        using var scope = app.Services.CreateScope();

        try
        {
            
            // Init Database
            var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
            var shouldRestDatabaseOnRestart = configuration.GetValue<bool>("ShouldClearDatabaseOnRestart");
            await initialiser.InitialiseAsync(shouldRestDatabaseOnRestart);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An error occurred seeding the DB. {exceptionMessage}", ex.Message);
        }
    }

    public static void ConfigureWebApplication(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");
    }
}
