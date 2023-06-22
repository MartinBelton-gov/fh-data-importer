namespace FamilyHub.DataImporter.Web.Data;

public class DataImportApiService
{
    private static readonly ImportType[] ImportMappers = new[]
    {
        new ImportType{ Name = "Elmbridge Council", Supplier = "Placecube" },
        new ImportType{ Name = "Bristol City Council", Supplier = "Placecube" },
        new ImportType{ Name = "North Lincolnshire Council", Supplier = "Placecube" },
        new ImportType{ Name = "Pennine Lancashire", Supplier = "Placecube" },
        new ImportType{ Name = "London Borough of Hounslow", Supplier = "Ayup" },
        new ImportType{ Name = "London Borough of Sutton", Supplier = "Ayup" },
    };
    public Task<ImportType[]> GetDataImportsAsync()
    {
        return Task.FromResult(ImportMappers);
    }
}
