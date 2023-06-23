using FamilyHubs.DataImporter.Infrastructure;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using SouthamptonImporter.Services;
using static PluginBase.BaseMapper;

namespace SouthamptonImporter;

public class SouthamtonImportCommand : IDataInputCommand
{
    public CancellationTokenSource? CancellationTokenSource { get; set; }
    public UpdateProgress? UpdateProgressDelegate { get; set; }
    public string Name { get => "DataImporter"; }
    public string Description { get => "Imports Southampton Data."; }
    public ApplicationDbContext? ApplicationDbContext { get; set; }
    public async Task<int> Execute(string arg, string testOnly)
    {
        const OrganisationType organisationType = OrganisationType.LA;
        OrganisationWithServicesDto southamptonCouncil = new OrganisationWithServicesDto
        {
            AdminAreaCode = "E10000020",
            OrganisationType = organisationType,
            Name = "Southampton City Council",
            Description = "Southampton City Council",
            Logo = default!,
            Uri = "https://www.southampton.gov.uk/",
            Url = "https://www.southampton.gov.uk/",
        };

        if (!string.IsNullOrEmpty(testOnly) && testOnly != southamptonCouncil.Name)
        {
            return 0;
        }

        Console.WriteLine($"Starting Southampton Mapper");
#pragma warning disable S1075 // URIs should not be hardcoded
        ISouthamptonClientService southamptonClientService = new SouthamptonClientService("https://directory.southampton.gov.uk/api/");
        IOrganisationClientService organisationClientService = new OrganisationClientService(arg);


        IServiceDirectoryMapper ServiceDirectoryMapper = new SouthamptonMapper(this,southamptonClientService, organisationClientService, southamptonCouncil.AdminAreaCode, southamptonCouncil.Name, southamptonCouncil);
#pragma warning restore S1075 // URIs should not be hardcoded
        ServiceDirectoryMapper.UpdateProgressDelegate = UpdateProgressDelegate;
        await ServiceDirectoryMapper.AddOrUpdateServices();
        Console.WriteLine($"Finished Buckinghamshire Mapper");



        return 0;
    }
}
