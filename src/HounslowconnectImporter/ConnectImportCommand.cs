using FamilyHubs.DataImporter.Infrastructure;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using HounslowconnectImporter.Services;
using PluginBase;

namespace HounslowconnectImporter;


public class ConnectImportCommand : IDataInputCommand
{
    public string Name { get => "DataImporter"; }
    public string Description { get => "Imports Data."; }

    public ApplicationDbContext? ApplicationDbContext { get; set; }
    public string Progress { get; set; } = default!;

    public async Task<int> Execute(string arg, string testOnly)
    {
        const OrganisationType organisationType = OrganisationType.LA;
        OrganisationWithServicesDto hounslow = new OrganisationWithServicesDto
        {
            AdminAreaCode = "E09000018",
            OrganisationType = organisationType,
            Name = "London Borough of Hounslow",
            Description = "London Borough of Hounslow",
            Logo = default!,
            Uri = "https://www.hounslow.gov.uk/",
            Url = "https://www.hounslow.gov.uk/",
        };

        OrganisationWithServicesDto sutton = new OrganisationWithServicesDto
        {
            AdminAreaCode = "E09000018",
            OrganisationType = organisationType,
            Name = "London Borough of Sutton",
            Description = "London Borough of Sutton",
            Logo = default!,
            Uri = "https://www.sutton.gov.uk/",
            Url = "https://www.sutton.gov.uk/",
        };

        List<CommandItem> commandItems = new()
        {
            new CommandItem() { Name = hounslow.Name, BaseUrl = "https://api.hounslowconnect.com/core/v1/", AdminAreaCode = hounslow.AdminAreaCode, ParentOrganisation = hounslow, ReturnType = typeof(ConnectService) },
            new CommandItem() { Name = sutton.Name, BaseUrl = "https://api.suttoninformationhub.org.uk/core/v1/", AdminAreaCode = sutton.AdminAreaCode, ParentOrganisation = sutton, ReturnType = typeof(ConnectService) },

        };

        foreach (var commandItem in commandItems)
        {
            if (!string.IsNullOrEmpty(testOnly) && testOnly != commandItem.Name)
            {
                continue;
            }

            Console.WriteLine($"Starting {commandItem.Name} Mapper");
            IConnectMapper mapper = CreateMapper(arg, commandItem);
            await mapper.AddOrUpdateServices();
            Console.WriteLine($"Finished {commandItem.Name} Mapper");
        }

        return 0;
    }

    private IConnectMapper CreateMapper(string arg, CommandItem commandItem)
    {
#pragma warning disable S1075 // URIs should not be hardcoded
        IPostcodeLocationClientService postcodeLocationClientService = new PostcodeLocationClientService("http://api.postcodes.io");
#pragma warning restore S1075 // URIs should not be hardcoded        
        IOrganisationClientService organisationClientService = new OrganisationClientService(arg);
        IPostCodeCacheLookupService postCodeCacheLookupService = new PostCodeCacheLookupService(postcodeLocationClientService, ApplicationDbContext!);

        IConnectMapper mapper;

        switch (commandItem.ReturnType)
        {

            default:
                {
                    IConnectClientService<ConnectService> clientService = new ConnectClientService<ConnectService>(commandItem.BaseUrl);
                    mapper = new ConnectMapper(this,postCodeCacheLookupService, clientService, organisationClientService, commandItem.AdminAreaCode, commandItem.Name, commandItem.ParentOrganisation);
                }
                break;
        }

        return mapper;
    }
}
