using PluginBase;
using System;

namespace BuckingshireImporter
{
    public class BuckingshireImportCommand : IDataInputCommand
    {
        public string Name { get => "DataImporter"; }
        public string Description { get => "Imports Buckinghamshire Data."; }

        public async Task<int> Execute(string arg)
        {
            await Task.Run(() =>
            {
                int result = 1 + 2;
            });

            return 0;
        }
    }
}
