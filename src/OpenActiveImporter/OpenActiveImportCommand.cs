using FamilyHubs.DataImporter.Infrastructure;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;
using OpenActiveImporter.Services;
using PluginBase;

namespace OpenActiveImporter;



public class OpenActiveImportCommand : IDataInputCommand
{
    public string Name { get => "DataImporter"; }
    public string Description { get => "Imports OpenActive Data Data."; }
    public ApplicationDbContext? ApplicationDbContext { get; set; }
    public string Progress { get; set; } = default!;

    public async Task<int> Execute(string arg, string testOnly)
    {
        const OrganisationType organisationType = OrganisationType.Company;
        OrganisationWithServicesDto activeThameside = new OrganisationWithServicesDto
        {
            AdminAreaCode = "E08000008",
            OrganisationType = organisationType,
            Name = "Active Tameside",
            Description = "Active Tameside",
            Logo = default!,
            Uri = "https://www.activetameside.com/",
            Url = "https://www.activetameside.com/",
        };

        var leisureManagement = new OrganisationWithServicesDto
        {
            AdminAreaCode = "E10000008",
            OrganisationType = organisationType,
            Name = "LED Leisure Management Ltd",
            Description = "LED Leisure Management Ltd",
            Logo = default!,
            Uri = "https://www.ledleisure.co.uk/",
            Url = "https://www.ledleisure.co.uk/",
        };

        var bwdleisure = new OrganisationWithServicesDto
        {
            AdminAreaCode = "E06000008",
            OrganisationType = organisationType,
            Name = "BwD Leisure",
            Description = "BwD Leisure",
            Logo = default!,
            Uri = "https://bwdleisure.com/",
            Url = "https://bwdleisure.com/",
        };

        //var exerciseAnywhere = new OrganisationWithServicesDto
        //{
        //    AdminAreaCode = "E06000059",
        //    OrganisationType = organisationType,
        //    Name = "Excercise Anywhere",
        //    Description = "Excercise Anywhere",
        //    Logo = default!,
        //    Uri = "https://exercise-anywhere.com/",
        //    Url = "https://exercise-anywhere.com/",
        //};

        List<CommandItem> commandItems = new()
        {
            new CommandItem() { Name = activeThameside.Name, BaseUrl = "https://tameside-openactive.legendonlineservices.co.uk/api/sessions", AdminAreaCode = activeThameside.AdminAreaCode, ParentOrganisation = activeThameside, ReturnType = typeof(OpenActiveService) },
            //new CommandItem() { Name = exerciseAnywhere.Name, BaseUrl = "https://opendata.exercise-anywhere.com/api/rpde/session-series", AdminAreaCode = exerciseAnywhere.AdminAreaCode, ParentOrganisation = exerciseAnywhere },
            new CommandItem() { Name = leisureManagement.Name, BaseUrl = "https://opendata.leisurecloud.live/api/feeds/LED-live-session-series", AdminAreaCode = leisureManagement.AdminAreaCode, ParentOrganisation = leisureManagement, ReturnType = typeof(OpenActiveBasicService) },
            new CommandItem() { Name = bwdleisure.Name, BaseUrl = "https://blackburnwithdarwen-openactive.legendonlineservices.co.uk/api/sessions", AdminAreaCode = bwdleisure.AdminAreaCode, ParentOrganisation = bwdleisure, ReturnType = typeof(OpenActiveService) },
        };

        foreach (var commandItem in commandItems)
        {
            if (!string.IsNullOrEmpty(testOnly) && testOnly != commandItem.Name)
            {
                continue;
            }

            Console.WriteLine($"Starting {commandItem.Name} Mapper");
            IOpenActiveMapper mapper = CreateMapper(arg, commandItem);
            await mapper.AddOrUpdateServices();
            Console.WriteLine($"Finished {commandItem.Name} Mapper");
        }

        return 0;
    }

    private IOpenActiveMapper CreateMapper(string arg, CommandItem commandItem)
    {
#pragma warning disable S1075 // URIs should not be hardcoded
        IPostcodeLocationClientService postcodeLocationClientService = new PostcodeLocationClientService("http://api.postcodes.io");
#pragma warning restore S1075 // URIs should not be hardcoded        
        IOrganisationClientService organisationClientService = new OrganisationClientService(arg);
        IPostCodeCacheLookupService postCodeCacheLookupService = new PostCodeCacheLookupService(postcodeLocationClientService, ApplicationDbContext!);

        IOpenActiveMapper mapper;

        switch (commandItem.ReturnType) 
        {
            case Type t when t == typeof(OpenActiveService):
                {
                    IOpenActiveClientService<OpenActiveService> openActiveClientService = new OpenActiveClientService<OpenActiveService>(commandItem.BaseUrl);
                    mapper = new OpenActiveMapper(this, postCodeCacheLookupService, openActiveClientService, organisationClientService, commandItem.AdminAreaCode, commandItem.Name, commandItem.ParentOrganisation);
                }
                break;

            default:
                {
                    IOpenActiveClientService<OpenActiveBasicService> openActiveClientService = new OpenActiveClientService<OpenActiveBasicService>(commandItem.BaseUrl);
                    mapper = new OpenActiveBasicMapper(postCodeCacheLookupService, openActiveClientService, organisationClientService, commandItem.AdminAreaCode, commandItem.Name, commandItem.ParentOrganisation);
                }
                break;
        }

        return mapper;
    }
}