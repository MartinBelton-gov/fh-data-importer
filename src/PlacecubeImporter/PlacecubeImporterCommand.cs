using AutoMapper;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using PlacecubeImporter.Services;
using PluginBase;

namespace PlacecubeImporter;

public class PlacecubeImporterCommand : IDataInputCommand
{
    public string Name { get => "DataImporter"; }
    public string Description { get => "Imports Placecube Data."; }

    public async Task<int> Execute(string arg, string testOnly)
    {
        var myProfile = new AutoMappingProfiles();
        var configuration = new MapperConfiguration(
            cfg => cfg.AddProfile(myProfile)
        );
        var mapper = new Mapper(configuration);

        const OrganisationType organisationType = OrganisationType.LA;
        var elmbridgeCouncil = new OrganisationWithServicesDto
        {
            AdminAreaCode = "E10000030",
            OrganisationType = organisationType,
            Name = "Elmbridge Council",
            Description = "Elmbridge Council",
            Logo = default!,
            Uri = "https://www.elmbridge.gov.uk/",
            Url = "https://www.elmbridge.gov.uk/",
        };

        var northLincCouncil = new OrganisationWithServicesDto
        {
            AdminAreaCode = "E06000013",
            OrganisationType = organisationType,
            Name = "North Lincolnshire Council",
            Description = "North Lincolnshire Council",
            Logo = default!,
            Uri = "https://www.northlincs.gov.uk/",
            Url = "https://www.northlincs.gov.uk/",
        };

        var pennineLancashire = new OrganisationWithServicesDto
        {
            AdminAreaCode = "E10000017",
            OrganisationType = organisationType,
            Name = "Pennine Lancashire",
            Description = "Pennine Lancashire",
            Logo = default!,
            Uri = "https://healthierpenninelancashire.co.uk/",
            Url = "https://healthierpenninelancashire.co.uk/",
        };

        List<CommandItem> commandItems = new()
            {
                new CommandItem() { Name = pennineLancashire.Name, BaseUrl = "https://penninelancs.openplace.directory/o/ServiceDirectoryService/v2", AdminAreaCode = "E10000017", ParentOrganisation = pennineLancashire },
                new CommandItem() { Name = northLincCouncil.Name, BaseUrl = "https://northlincs.openplace.directory/o/ServiceDirectoryService/v2", AdminAreaCode = "E06000013", ParentOrganisation = northLincCouncil },
                new CommandItem() { Name = elmbridgeCouncil.Name, BaseUrl = "https://elmbridge.openplace.directory/o/ServiceDirectoryService/v2", AdminAreaCode = "E10000030", ParentOrganisation = elmbridgeCouncil }
            };

        foreach (var commandItem in commandItems)
        {
            if (!string.IsNullOrEmpty(testOnly) && testOnly != commandItem.Name)
            {
                continue;
            }

            Console.WriteLine($"Starting {commandItem.Name} Mapper");
#pragma warning disable S1075 // URIs should not be hardcoded
            IPlacecubeClientService placecubeClientService = new PlacecubeClientService(commandItem.BaseUrl);
            IOrganisationClientService organisationClientService = new OrganisationClientService(arg);


            PlacecubeMapper placecubeMapper = new PlacecubeMapper(placecubeClientService, organisationClientService, mapper, commandItem.AdminAreaCode, commandItem.Name, commandItem.ParentOrganisation);
#pragma warning restore S1075 // URIs should not be hardcoded
            await placecubeMapper.AddOrUpdateServices();
            Console.WriteLine($"Finished {commandItem.Name} Mapper");
        }

        return 0;
    }
}