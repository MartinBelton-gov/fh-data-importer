﻿// See https://aka.ms/new-console-template for more information

using BuckingshireImporter;
using FamilyHubs.DataImporter.Infrastructure;
using HounslowconnectImporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenActiveImporter;
using PlacecubeImporter;
using PluginBase;
using PublicPartnershipImporter;
using SalfordImporter;
using SouthamptonImporter;
using SportEngland;
using System.Threading.Tasks;

//https://thecodeblogger.com/2022/09/16/net-dependency-injection-one-interface-and-multiple-implementations/

namespace FamilyHubs.DataImporter
{
    class Program
    {
        protected Program() { }

        public static IConfiguration Configuration { get; private set; } = default!;
        static async Task Main(string[] args)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IConfiguration>(Program.Configuration)
            .AddScoped<IDataInputCommand, BuckingshireImportCommand>()
            .AddScoped<IDataInputCommand, PlacecubeImporterCommand>()
            .AddScoped<IDataInputCommand, SouthamtonImportCommand>()
            .AddScoped<IDataInputCommand, SalfordImportCommand>()
            .AddScoped<IDataInputCommand, PublicPartnershipImportCommand>()
            .AddScoped<IDataInputCommand, SportEnglandImportCommand>()
            .AddScoped<IDataInputCommand, OpenActiveImportCommand>()
            .AddScoped<IDataInputCommand, ConnectImportCommand>()
            .RegisterAppDbContext(Configuration)
            .BuildServiceProvider();

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("NonHostConsoleApp.Program", LogLevel.Debug)
                    .AddConsole();
            });
            ILogger logger = loggerFactory.CreateLogger<Program>();

            logger.LogDebug("Starting application");

            using var scope = serviceProvider.CreateScope();

            try
            {
                // Init Database
                var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
                var shouldRestDatabaseOnRestart = Configuration.GetValue<bool>("ShouldClearDatabaseOnRestart");
                await initialiser.InitialiseAsync(shouldRestDatabaseOnRestart);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the DB. {exceptionMessage}", ex.Message);
            }

            IEnumerable<IDataInputCommand> services = serviceProvider.GetServices<IDataInputCommand>();
            string servicedirectoryBaseUrl = Configuration["ApplicationServiceApi:ServiceDirectoryUrl"] ?? default!;
            string importerToTest = Configuration["ImporterToTest"] ?? default!;
            if (Configuration["RunAsync"] == "True")
            {
                List<Task> taskList = new List<Task>();
                foreach (var service in services)
                {
                    service.ServiceScope = scope;
                    taskList.Add(service.Execute(servicedirectoryBaseUrl, importerToTest));
                }

                await Task.WhenAll(taskList);
            }
            else
            {
                foreach (var service in services)
                {
                    service.ServiceScope = scope;
                    await service.Execute(servicedirectoryBaseUrl, importerToTest);
                }
            }
            
        }
    }
}
