using BuckingshireImporter;
using FamilyHub.DataImporter.Web.Data;
using HounslowconnectImporter;
using Microsoft.ApplicationInsights.Extensibility;
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
        services.AddSingleton<DataImportApiService>();
    }

    public static void RegisterImportComponents(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDataInputCommand, BuckingshireImportCommand>();
        services.AddScoped<IDataInputCommand, PlacecubeImporterCommand>();
        services.AddScoped<IDataInputCommand, SouthamtonImportCommand>();
        services.AddScoped<IDataInputCommand, SalfordImportCommand>();
        services.AddScoped<IDataInputCommand, PublicPartnershipImportCommand>();
        services.AddScoped<IDataInputCommand, SportEnglandImportCommand>();
        services.AddScoped<IDataInputCommand, OpenActiveImportCommand>();
        services.AddScoped<IDataInputCommand, ConnectImportCommand>();
    }

    public static async Task ConfigureWebApplication(this WebApplication app)
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
