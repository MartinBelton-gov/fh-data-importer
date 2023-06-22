using PluginBase;

namespace FamilyHub.DataImporter.Web.Data;

public class ImportType
{
    public required string Name { get; set; } 
    public required string Supplier { get; set; }
    public required IDataInputCommand DataInputCommand { get; set; }
}
