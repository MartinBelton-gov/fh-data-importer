using System.Threading.Tasks;

namespace PluginBase
{
    public interface IDataInputCommand
    {
        string Name { get; }
        string Description { get; }

        Task<int> Execute(string arg);
    }
}
