using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace PluginBase
{
    public interface IDataInputCommand
    {
        string Name { get; }
        string Description { get; }
        
        IServiceScope? ServiceScope { get; set;  }

        Task<int> Execute(string arg, string testOnly);
    }
}
