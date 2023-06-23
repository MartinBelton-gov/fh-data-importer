using FamilyHubs.DataImporter.Infrastructure;

namespace PluginBase
{
    public interface IDataInputCommand
    {
        string Name { get; }
        string Description { get; }
        string Progress { get; set; }
        IServiceDirectoryMapper? ServiceDirectoryMapper { get; set; }
        public ApplicationDbContext? ApplicationDbContext { get; set; }

        Task<int> Execute(string arg, string testOnly);
    }
}
