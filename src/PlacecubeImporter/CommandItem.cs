﻿using FamilyHubs.ServiceDirectory.Shared.Dto;

namespace PlacecubeImporter
{
    internal class CommandItem
    {
        public string Name { get; set; } = default!; 
        public string BaseUrl { get; set; } = default!;
        public string AdminAreaCode { get; set; } = default!;
        public OrganisationWithServicesDto ParentOrganisation { get; set; } = default!;
    }
}
