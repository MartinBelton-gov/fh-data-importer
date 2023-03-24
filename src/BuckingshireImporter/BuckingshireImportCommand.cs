using BuckingshireImporter.Services;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using PluginBase;

namespace BuckingshireImporter
{
    public class BuckingshireImportCommand : IDataInputCommand
    {
        public string Name { get => "DataImporter"; }
        public string Description { get => "Imports Buckinghamshire Data."; }

        public async Task<int> Execute(string arg, string testOnly)
        {
            

            const OrganisationType organisationType = OrganisationType.LA;
            OrganisationWithServicesDto buckinghamshireCouncil = new OrganisationWithServicesDto
            {
                AdminAreaCode = "E06000060",
                OrganisationType = organisationType,
                Name = "Buckingshire Council",
                Description = "Buckingshire Council",
                Logo = default!,
                Uri = "https://www.buckinghamshire.gov.uk/",
                Url = "https://www.buckinghamshire.gov.uk/",
            };

            if (!string.IsNullOrEmpty(testOnly) && testOnly != buckinghamshireCouncil.Name)
            {
                return 0;
            }

            Console.WriteLine($"Starting Buckinghamshire Mapper");
#pragma warning disable S1075 // URIs should not be hardcoded
            IBuckinghamshireClientService buckinghamshireClientService = new BuckinghamshireClientService("https://api.familyinfo.buckinghamshire.gov.uk/api/v1/");
            IOrganisationClientService organisationClientService = new OrganisationClientService(arg);


            BuckinghamshireMapper buckinghamshireMapper = new BuckinghamshireMapper(buckinghamshireClientService, organisationClientService, buckinghamshireCouncil.AdminAreaCode, buckinghamshireCouncil.Name, buckinghamshireCouncil);
#pragma warning restore S1075 // URIs should not be hardcoded
            await buckinghamshireMapper.AddOrUpdateServices();
            Console.WriteLine($"Finished Buckinghamshire Mapper");

            

            return 0;
        }
    }
}
