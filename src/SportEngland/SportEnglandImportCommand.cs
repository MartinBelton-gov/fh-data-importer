using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using SportEngland.Services;

namespace SportEngland;

public class SportEnglandImportCommand : IDataInputCommand
{
    public string Name { get => "DataImporter"; }
    public string Description { get => "Imports Sport England Data."; }
    public IServiceScope? ServiceScope { get; set; }

    public async Task<int> Execute(string arg, string testOnly)
    {
        const OrganisationType organisationType = OrganisationType.LA;
        OrganisationWithServicesDto sportEngland = new OrganisationWithServicesDto
        {
            AdminAreaCode = "E10000018",
            OrganisationType = organisationType,
            Name = "Sport England",
            Description = "Sport England",
            Logo = default!,
            Uri = "https://www.sportengland.org/",
            Url = "https://www.sportengland.org/",
        };

        if (!string.IsNullOrEmpty(testOnly) && testOnly != sportEngland.Name)
        {
            return 0;
        }

        Console.WriteLine($"Starting Sport England Mapper");
#pragma warning disable S1075 // URIs should not be hardcoded
        IPostcodeLocationClientService postcodeLocationClientService = new PostcodeLocationClientService("http://api.postcodes.io");
        ISportEnglandClientService sportEnglandClientService = new SportEnglandClientService("https://api.activeplacespower.com/api/v1.1/");
        IOrganisationClientService organisationClientService = new OrganisationClientService(arg);


        SportEnglandImportMapper sportEnglandImportMapper = new SportEnglandImportMapper(postcodeLocationClientService, sportEnglandClientService, organisationClientService, sportEngland.AdminAreaCode, sportEngland.Name, sportEngland);
#pragma warning restore S1075 // URIs should not be hardcoded
        await sportEnglandImportMapper.AddOrUpdateServices();
        Console.WriteLine($"Finished Sport England Mapper");



        return 0;
    }
}