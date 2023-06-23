using static PluginBase.BaseMapper;

namespace PluginBase
{
    public interface IServiceDirectoryMapper
    {
        Task AddOrUpdateServices();
        public UpdateProgress? UpdateProgressDelegate { get; set; }
    }
}
