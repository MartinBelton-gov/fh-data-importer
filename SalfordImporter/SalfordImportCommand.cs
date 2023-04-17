﻿using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using PluginBase;
using SalfordImporter.Services;

namespace SalfordImporter;

public class SalfordImportCommand : IDataInputCommand
{
    public string Name { get => "DataImporter"; }
    public string Description { get => "Imports Buckinghamshire Data."; }


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


        SalfordMapper salfordMapper = new SalfordMapper(salfordClientService, organisationClientService, salfordCouncil.AdminAreaCode, salfordCouncil.Name, salfordCouncil);
#pragma warning restore S1075 // URIs should not be hardcoded
        await salfordMapper.AddOrUpdateServices();
        Console.WriteLine($"Finished Salford Mapper");
        return 0;

    }
}