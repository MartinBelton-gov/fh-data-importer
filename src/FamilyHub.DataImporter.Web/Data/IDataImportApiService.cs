using FamilyHub.DataImporter.Web.Pages;
using PluginBase;

namespace FamilyHub.DataImporter.Web.Data
{
    public interface IDataImportApiService
    {
        Task<ImportType[]> GetDataImportsAsync();
        bool IsTaskRunning(string name);
        DataImportTask? StartImport(string name, BaseMapper.UpdateProgress updateProgress, Imports imports);
        void StopImport(string name);
    }
}