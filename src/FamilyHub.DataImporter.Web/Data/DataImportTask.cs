namespace FamilyHub.DataImporter.Web.Data;

public class DataImportTask
{
    public required ImportType ImportType { get; set; }
    public Task ItemTask { get; set; } = default!;
}
