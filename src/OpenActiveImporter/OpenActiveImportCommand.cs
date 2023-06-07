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
    public IServiceScope? ServiceScope { get; set; }

    public async Task<int> Execute(string arg, string testOnly)
    {
        const OrganisationType organisationType = OrganisationType.Company;
        OrganisationWithServicesDto sportEngland = new OrganisationWithServicesDto
        {
            AdminAreaCode = "E08000008",
            OrganisationType = organisationType,
            Name = "Active Tameside",
            Description = "Active Tameside",
            Logo = default!,
            Uri = "https://www.activetameside.com/",
            Url = "https://www.activetameside.com/",
        };

        if (!string.IsNullOrEmpty(testOnly) && testOnly != sportEngland.Name)
        {
            return 0;
        }

        Console.WriteLine($"Starting Open Active Mapper");
#pragma warning disable S1075 // URIs should not be hardcoded
        var db = ServiceScope?.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IPostcodeLocationClientService postcodeLocationClientService = new PostcodeLocationClientService("http://api.postcodes.io");
        IOpenActiveClientService openActiveClientService = new OpenActiveClientService("https://tameside-openactive.legendonlineservices.co.uk/api/sessions");
        IOrganisationClientService organisationClientService = new OrganisationClientService(arg);
        IPostCodeCacheLookupService postCodeCacheLookupService = new PostCodeCacheLookupService(postcodeLocationClientService, db!);


        OpenActiveMapper sportEnglandImportMapper = new OpenActiveMapper(postCodeCacheLookupService, openActiveClientService, organisationClientService, sportEngland.AdminAreaCode, sportEngland.Name, sportEngland);
#pragma warning restore S1075 // URIs should not be hardcoded
        await sportEnglandImportMapper.AddOrUpdateServices();
        Console.WriteLine($"Finished Sport England Mapper");



        return 0;
    }
}