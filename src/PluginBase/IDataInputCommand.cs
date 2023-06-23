using FamilyHubs.DataImporter.Infrastructure;
using static PluginBase.BaseMapper;

namespace PluginBase
{
    public interface IDataInputCommand
    {
        string Name { get; }
        string Description { get; }
        public CancellationTokenSource? CancellationTokenSource { get; set; }
        UpdateProgress? UpdateProgressDelegate { get; set; }
        public ApplicationDbContext? ApplicationDbContext { get; set; }

        Task<int> Execute(string arg, string testOnly);
    }
}
