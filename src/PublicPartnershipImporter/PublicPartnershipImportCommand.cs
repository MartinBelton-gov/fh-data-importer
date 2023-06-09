﻿using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using PublicPartnershipImporter.Services;

namespace PublicPartnershipImporter;

public class PublicPartnershipImportCommand : IDataInputCommand
{
    public string Name { get => "DataImporter"; }
    public string Description { get => "Imports some other Data."; }
    public IServiceScope? ServiceScope { get; set; }

    public async Task<int> Execute(string arg, string testOnly)
    {
        const OrganisationType organisationType = OrganisationType.LA;
        var hullCouncil = new OrganisationWithServicesDto
        {
            AdminAreaCode = "E06000010",
            OrganisationType = organisationType,
            Name = "Hull City Council",
            Description = "Hull City Council",
            Logo = default!,
            Uri = "https://www.hull.gov.uk/",
            Url = "https://www.hull.gov.uk/",
        };

        if (!string.IsNullOrEmpty(testOnly) && testOnly != hullCouncil.Name)
        {
            return 0;
        }

        Console.WriteLine($"Starting Public Partnership Mapper (Hull City)");
#pragma warning disable S1075 // URIs should not be hardcoded
        IPublicPartnershipClientService publicPartnershipClientService = new PublicPartnershipClientService("https://lgaapi.connecttosupport.org/");
        IOrganisationClientService organisationClientService = new OrganisationClientService(arg);

        PublicPartnershipMapper publicPartnershipMapper = new PublicPartnershipMapper(publicPartnershipClientService, organisationClientService, hullCouncil.AdminAreaCode, hullCouncil.Name, hullCouncil);
        
#pragma warning restore S1075 // URIs should not be hardcoded
        await publicPartnershipMapper.AddOrUpdateServices();
        Console.WriteLine($"Finished Public Partnership Mapper (Hull City)");
        return 0;
    }
}
