using BuckingshireImporter;
using FamilyHub.DataImporter.Web.Pages;
using FamilyHubs.DataImporter.Infrastructure;
using HounslowconnectImporter;
using OpenActiveImporter;
using PlacecubeImporter;
using PublicPartnershipImporter;
using SalfordImporter;
using SouthamptonImporter;
using SportEngland;
using static PluginBase.BaseMapper;

namespace FamilyHub.DataImporter.Web.Data;

public class DataImportApiService : IDataImportApiService
{
    private static List<DataImportTask> _runningTasks = new List<DataImportTask>();
    public static List<DataImportTask> RunningTasks { get { return _runningTasks; } }

    private static readonly ImportType[] ImportMappers = new[]
    {
        new ImportType{ Name = "Elmbridge Council", Supplier = "Placecube", DataInputCommand = new PlacecubeImporterCommand() },
        new ImportType{ Name = "Bristol City Council", Supplier = "Placecube", DataInputCommand = new PlacecubeImporterCommand() },
        new ImportType{ Name = "North Lincolnshire Council", Supplier = "Placecube", DataInputCommand = new PlacecubeImporterCommand() },
        new ImportType{ Name = "Pennine Lancashire", Supplier = "Placecube", DataInputCommand = new PlacecubeImporterCommand() },
        new ImportType{ Name = "London Borough of Hounslow", Supplier = "Ayup", DataInputCommand = new ConnectImportCommand() },
        new ImportType{ Name = "London Borough of Sutton", Supplier = "Ayup", DataInputCommand = new ConnectImportCommand() },
        new ImportType{ Name = "Buckingshire Council", Supplier = "FutureGov", DataInputCommand = new BuckingshireImportCommand() },
        new ImportType{ Name = "Salford City Council", Supplier = "Open Objects", DataInputCommand = new SalfordImportCommand() },
        new ImportType{ Name = "Southampton City Council", Supplier = "Etch UK", DataInputCommand = new SouthamtonImportCommand() },
        new ImportType{ Name = "Active Tameside", Supplier = "Open Active / Imin / ORUK", DataInputCommand = new OpenActiveImportCommand() },
        new ImportType{ Name = "LED Leisure Management Ltd", Supplier = "Open Active", DataInputCommand = new OpenActiveImportCommand() },
        new ImportType{ Name = "BwD Leisure", Supplier = "Open Active / Imin / ORUK", DataInputCommand = new OpenActiveImportCommand() },
        new ImportType{ Name = "Sport England", Supplier = "Open Active / Imin / ORUK", DataInputCommand = new SportEnglandImportCommand() },
        new ImportType{ Name = "Hull City Council", Supplier = "Public Partnership", DataInputCommand = new PublicPartnershipImportCommand() },
    };

    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _applicationDbContext;

    public DataImportApiService(IConfiguration configuration, ApplicationDbContext applicationDbContext)
    {
        _configuration = configuration;
        _applicationDbContext = applicationDbContext;
    }
    public Task<ImportType[]> GetDataImportsAsync()
    {
        return Task.FromResult(ImportMappers);
    }

    public DataImportTask? StartImport(string name, UpdateProgress updateProgress, Imports imports)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        ImportType? importType = ImportMappers.FirstOrDefault(x => x.Name == name);
        if (importType == null)
        {
            return null;
        }

        var task = RunningTasks.FirstOrDefault(x => x.ImportType.Name == name);
        if (task != null)
        {
            return null;
        }

        string servicedirectoryBaseUrl = _configuration["ApplicationServiceApi:ServiceDirectoryUrl"] ?? default!;

        importType.DataInputCommand.ApplicationDbContext = _applicationDbContext;
        importType.DataInputCommand.UpdateProgressDelegate = updateProgress;
        importType.DataInputCommand.CancellationTokenSource = new CancellationTokenSource();

        task = new DataImportTask
        {
            ImportType = importType,
        };

        task.ImportType.DataInputCommand.CancellationTokenSource = new CancellationTokenSource();

        task.ItemTask = importType.DataInputCommand.Execute(servicedirectoryBaseUrl, importType.Name)
            .ContinueWith(t =>
            {
                RunningTasks.Remove(task);
                task.ImportType.DataInputCommand.CancellationTokenSource.Dispose();
                imports.StatusChanged().ConfigureAwait(false);
            });

        RunningTasks.Add(task);

        return task;
    }

    public void StopImport(string name)
    {
        var task = RunningTasks.FirstOrDefault(x => x.ImportType.Name == name);
        if (task == null)
        {
            return;
        }

        // Cancel the task
        if (task.ImportType.DataInputCommand.CancellationTokenSource != null)
            task.ImportType.DataInputCommand.CancellationTokenSource.Cancel();
    }

    public bool IsTaskRunning(string name)
    {
        var runningTask = RunningTasks.FirstOrDefault(x => x.ImportType.Name == name);
        if (runningTask != null)
            return true;

        return false;
    }
}
