// See https://aka.ms/new-console-template for more information

using BuckingshireImporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlacecubeImporter;
using PluginBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

            var placecude = services.FirstOrDefault(x => x.GetType() == typeof(PlacecubeImporterCommand));
            string servicedirectoryBaseUrl = Configuration["ApplicationServiceApi:ServiceDirectoryUrl"] ?? default!;
            if (placecude != null && !string.IsNullOrEmpty(servicedirectoryBaseUrl))
            {
                await placecude.Execute(servicedirectoryBaseUrl);
            }
        }
    }
}
