using FamilyHubs.DataImporter.Infrastructure;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using SalfordImporter.Services;
using static PluginBase.BaseMapper;
using static System.Formats.Asn1.AsnWriter;

namespace SalfordImporter;

public class SalfordImportCommand : IDataInputCommand
{
    public CancellationTokenSource? CancellationTokenSource { get; set; }
    public UpdateProgress? UpdateProgressDelegate { get; set; }
    public string Name { get => "DataImporter"; }
    public string Description { get => "Imports Buckinghamshire Data."; }

    public ApplicationDbContext? ApplicationDbContext { get; set; }

    public async Task<int> Execute(string arg, string testOnly)
    {
        const OrganisationType organisationType = OrganisationType.LA;
        OrganisationWithServicesDto salfordCouncil = new OrganisationWithServicesDto
        {
            AdminAreaCode = "E08000006",
            OrganisationType = organisationType,
            Name = "Salford City Council",
            Description = "Salford City Council",
            Logo = default!,
            Uri = "https://www.salford.gov.uk/",
            Url = "https://www.salford.gov.uk/",
        };

        if (!string.IsNullOrEmpty(testOnly) && testOnly != salfordCouncil.Name)
        {
            return 0;
        }

        Console.WriteLine($"Starting Salford Mapper");
#pragma warning disable S1075 // URIs should not be hardcoded
        ISalfordClientService salfordClientService = new SalfordClientService("https://api.openobjects.com/v2/salforddirectory/records");
        IOrganisationClientService organisationClientService = new OrganisationClientService(arg);
        IPostcodeLocationClientService postcodeLocationClientService = new PostcodeLocationClientService("http://api.postcodes.io");
        IPostCodeCacheLookupService postCodeCacheLookupService = new PostCodeCacheLookupService(postcodeLocationClientService, ApplicationDbContext!);

        IServiceDirectoryMapper ServiceDirectoryMapper = new SalfordMapper(this, salfordClientService, organisationClientService, postCodeCacheLookupService, salfordCouncil.AdminAreaCode, salfordCouncil.Name, salfordCouncil);
#pragma warning restore S1075 // URIs should not be hardcoded
        ServiceDirectoryMapper.UpdateProgressDelegate = UpdateProgressDelegate;
        await ServiceDirectoryMapper.AddOrUpdateServices();
        Console.WriteLine($"Finished Salford Mapper");
        return 0;

    }
}