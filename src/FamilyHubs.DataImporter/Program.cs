// See https://aka.ms/new-console-template for more information

using BuckingshireImporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlacecubeImporter;
using PluginBase;
using SalfordImporter;
using SouthamptonImporter;

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

            IEnumerable<IDataInputCommand> services = serviceProvider.GetServices<IDataInputCommand>();
            string servicedirectoryBaseUrl = Configuration["ApplicationServiceApi:ServiceDirectoryUrl"] ?? default!;
            string importerToTest = Configuration["ImporterToTest"] ?? default!;
            foreach (var service in services)
            {
                await service.Execute(servicedirectoryBaseUrl, importerToTest);
            }
        }
    }
}
