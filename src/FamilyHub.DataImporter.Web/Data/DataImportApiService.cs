using FamilyHubs.DataImporter.Infrastructure;
using HounslowconnectImporter;
using Microsoft.AspNetCore.Components.Forms;
using PlacecubeImporter;
using static PluginBase.BaseMapper;

namespace FamilyHub.DataImporter.Web.Data;

public class DataImportApiService
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

    public DataImportTask? StartImport(string name, UpdateProgress updateProgress)
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
       
        task = new DataImportTask
        {
            ImportType = importType,
            CancellationTokenSource = new CancellationTokenSource()
        };

        task.ItemTask = importType.DataInputCommand.Execute(servicedirectoryBaseUrl, importType.Name)
            .ContinueWith(t =>
            {
                RunningTasks.Remove(task);
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
        task.CancellationTokenSource.Cancel();
        task.ItemTask.Wait();
        task.CancellationTokenSource.Dispose();
    }

    public bool IsTaskRunning(string name)
    {
        var runningTask = RunningTasks.FirstOrDefault(x => x.ImportType.Name == name);
        if (runningTask != null)
            return true;
        
        return false;
    }
}
