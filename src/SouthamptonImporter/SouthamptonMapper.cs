using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using PluginBase;
using SouthamptonImporter.Services;

namespace SouthamptonImporter;

internal class SouthamptonMapper : BaseMapper
{
    private readonly ISouthamptonClientService _southamptonClientService;
    private readonly IDataInputCommand _dataInputCommand;

    public string Name => "Southampton Mapper";

    public SouthamptonMapper(IDataInputCommand dataInputCommand,ISouthamptonClientService southamptonClientService, IOrganisationClientService organisationClientService, string adminAreaCode, string key, OrganisationWithServicesDto parentLA)
        : base(organisationClientService, adminAreaCode, parentLA, key)
    {
        _dataInputCommand = dataInputCommand;
        _southamptonClientService = southamptonClientService;
    }

    public async Task AddOrUpdateServices()
    {
        const int startPage = 1;
        await CreateOrganisationDictionary();
        await CreateTaxonomyDictionary();
        SouthamptonSimpleService simpleService = await _southamptonClientService.GetServicesByPage(startPage);
        int totalPages = simpleService.totalPages;

        await LoopSimpleServices(startPage, totalPages);

        for (int i = startPage + 1; i <= totalPages; i++)
        {
            await LoopSimpleServices(i, totalPages);
        }
    }

    private async Task LoopSimpleServices(int page, int totalPages)
    {
        int itemCount = -1;
        int errorCount = 0;
        SouthamptonSimpleService simpleService = await _southamptonClientService.GetServicesByPage(page);
        foreach (var itemId in simpleService.content.Select(x => x.id))
        {
            try
            {
                itemCount++;
                SouthamptonService southamptonService = await _southamptonClientService.GetServiceById(itemId);
                errorCount += await AddAndUpdateService(southamptonService);
            }
            catch
            {
                Console.WriteLine($"This is only a simple service id: {itemId}");
                _dataInputCommand.Progress = $"This is only a simple service id: {itemId}";
                errorCount += await AddAndUpdateSimpleService(simpleService.content[itemCount]);
            }
        }

        Console.WriteLine($"Completed Page {page} of {totalPages} with {errorCount} errors");

    }

    private async Task<int> AddAndUpdateSimpleService(Content simpleService)
    {
        List<string> errors = new List<string>();
        OrganisationWithServicesDto serviceDirectoryOrganisation = default!;

        bool newOrganisation = false;
        if (_dictOrganisations.ContainsKey($"{_adminAreaCode}{simpleService.organization.name}"))
        {
            serviceDirectoryOrganisation = _dictOrganisations[$"{_adminAreaCode}{simpleService.organization.name}"];
            //Get latest
            serviceDirectoryOrganisation = await _organisationClientService.GetOrganisationById(serviceDirectoryOrganisation.Id.ToString());
        }
        else
        {
            const OrganisationType organisationType = OrganisationType.VCFS;
            serviceDirectoryOrganisation = new OrganisationWithServicesDto
            {
                AdminAreaCode = _adminAreaCode,
                OrganisationType = organisationType,
                Name = !string.IsNullOrEmpty(simpleService.organization.id) ? simpleService.organization.name : simpleService.name,
                Description = !string.IsNullOrEmpty(simpleService.organization.id) ? simpleService.organization.description : simpleService.name,
            };

            newOrganisation = true;
        }

        var serviceOwnerReferenceId = $"{_adminAreaCode.Replace("E", "")}{simpleService.id}";

        ServiceDto? existingService = serviceDirectoryOrganisation.Services.FirstOrDefault(s => s.ServiceOwnerReferenceId == serviceOwnerReferenceId);

        ServiceDto serviceDto = new ServiceDto()
        {
            Id = (existingService != null) ? existingService.Id : 0,
            OrganisationId = serviceDirectoryOrganisation.Id,
            ServiceType = ServiceType.InformationSharing,
            ServiceOwnerReferenceId = serviceOwnerReferenceId,
            Name = simpleService.name,
            Description = simpleService.description,
            Accreditations = default!,
            AssuredDate = simpleService.assured_date,
            AttendingAccess = AttendingAccessType.NotSet,
            AttendingType = AttendingType.NotSet,
            DeliverableType = DeliverableType.NotSet,
            Status = StringToEnum.ConvertServiceStatusType(simpleService.status),
            Fees = default!,
            CanFamilyChooseDeliveryLocation = false
        };


        errors = await AddOrUpdateDirectoryService(newOrganisation, serviceDirectoryOrganisation, serviceDto, serviceOwnerReferenceId, errors);


        foreach (string error in errors)
        {
            Console.WriteLine(error);
        }

        return errors.Count;

    }

    private async Task<int> AddAndUpdateService(SouthamptonService southamptonService)
    {
        List<string> errors = new List<string>();

        OrganisationWithServicesDto serviceDirectoryOrganisation = default!;

        bool newOrganisation = false;
        if (_dictOrganisations.ContainsKey($"{_adminAreaCode}{southamptonService.organization.name}"))
        {
            serviceDirectoryOrganisation = _dictOrganisations[$"{_adminAreaCode}{southamptonService.organization.name}"];
            //Get latest
            serviceDirectoryOrganisation = await _organisationClientService.GetOrganisationById(serviceDirectoryOrganisation.Id.ToString());
        }
        else
        {
            const OrganisationType organisationType = OrganisationType.VCFS;
            serviceDirectoryOrganisation = new OrganisationWithServicesDto
            {
                AdminAreaCode = _adminAreaCode,
                OrganisationType = organisationType,
                Name = !string.IsNullOrEmpty(southamptonService.organization.id) ? southamptonService.organization.name : southamptonService.name,
                Description = !string.IsNullOrEmpty(southamptonService.organization.id) ? southamptonService.organization.description : southamptonService.name,
                Services = new List<ServiceDto>()
            };

            newOrganisation = true;
        }

        var serviceOwnerReferenceId = $"{_adminAreaCode.Replace("E", "")}{southamptonService.id}";

        ServiceDto? existingService = serviceDirectoryOrganisation.Services.FirstOrDefault(s => s.ServiceOwnerReferenceId == serviceOwnerReferenceId);

        ServiceDto serviceDto = new ServiceDto()
        {
            Id = (existingService != null) ? existingService.Id : 0,
            OrganisationId = serviceDirectoryOrganisation.Id,
            ServiceType = ServiceType.InformationSharing,
            ServiceOwnerReferenceId = serviceOwnerReferenceId,
            Name = southamptonService.name,
            Description = southamptonService.description,
            Accreditations = default!,
            AssuredDate = southamptonService.assured_date,
            AttendingAccess = AttendingAccessType.NotSet,
            AttendingType = AttendingType.NotSet,
            DeliverableType = DeliverableType.NotSet,
            Status = StringToEnum.ConvertServiceStatusType(southamptonService.status),
            Fees = default!,
            CanFamilyChooseDeliveryLocation = false,
            Eligibilities = GetEligibilityDtos(southamptonService.eligibilitys, existingService),
            CostOptions = GetCostOptionDtos(southamptonService.cost_options, existingService),
            ServiceAreas = GetServiceAreas(southamptonService.service_areas, existingService),
            Fundings = new List<FundingDto>(),
            RegularSchedules = GetRegularSchedules(southamptonService.regular_schedules, existingService, null),
            HolidaySchedules = new List<HolidayScheduleDto>(),
            Contacts = GetContactDtos(southamptonService.contacts, existingService),
            Languages = GetLanguages(southamptonService.languages, existingService),
            Taxonomies = await GetServiceTaxonomies(southamptonService.service_taxonomys, existingService),
            Locations = GetLocations(southamptonService.service_at_locations, existingService),

        };



        errors = await AddOrUpdateDirectoryService(newOrganisation, serviceDirectoryOrganisation, serviceDto, serviceOwnerReferenceId, errors);

        foreach (string error in errors)
        {
            Console.WriteLine(error);
        }

        return errors.Count;
    }

    private List<EligibilityDto> GetEligibilityDtos(Eligibility[] eligibilities, ServiceDto? existingService)
    {
        List<EligibilityDto> listEligibilityDto = new List<EligibilityDto>();

        foreach (Eligibility eligibility in eligibilities)
        {
            var newEligibility = new EligibilityDto
            {
                MinimumAge = eligibility.minimum_age ?? 0,
                MaximumAge = eligibility.maximum_age ?? 0,
                EligibilityType = EligibilityType.NotSet
            };

            newEligibility.EligibilityType = newEligibility.MaximumAge < 18 ? EligibilityType.Child : EligibilityType.NotSet;
            if (newEligibility.MinimumAge >= 18)
            {
                newEligibility.EligibilityType = EligibilityType.Adult;
            }

            if (existingService != null)
            {
                var existingItem = existingService.Eligibilities.FirstOrDefault(x => x.Equals(newEligibility));

                if (existingItem != null)
                {
                    listEligibilityDto.Add(existingItem);
                    continue;
                }
            }

            listEligibilityDto.Add(newEligibility);
        }

        return listEligibilityDto;
    }

    private List<CostOptionDto> GetCostOptionDtos(CostOptions[] costOptions, ServiceDto? existingService)
    {
        if (costOptions == null || !costOptions.Any())
        {
            return new List<CostOptionDto>();
        }

        List<CostOptionDto> listCostOptionDto = new List<CostOptionDto>();

        foreach (CostOptions costOption in costOptions)
        {
            if (string.IsNullOrEmpty(costOption.amount_description))
                continue;

            var newCostOption = new CostOptionDto
            {
                AmountDescription = costOption.amount_description,
                Amount = costOption.amount,
                Option = costOption.option,
            };

            if (existingService != null)
            {
                var existingItem = existingService.CostOptions.FirstOrDefault(x => x.Equals(newCostOption));

                if (existingItem != null)
                {
                    listCostOptionDto.Add(existingItem);
                    continue;
                }
            }

            listCostOptionDto.Add(newCostOption);
        }

        return listCostOptionDto;
    }

    private List<ServiceAreaDto> GetServiceAreas(ServiceArea[] serviceAreas, ServiceDto? existingService)
    {
        if (serviceAreas == null || !serviceAreas.Any())
        {
            return new List<ServiceAreaDto>();
        }

        List<ServiceAreaDto> listServiceAreaDto = new List<ServiceAreaDto>();

        foreach (ServiceArea serviceArea in serviceAreas)
        {
            if (serviceArea == null)
                continue;

            var newServiceArea = new ServiceAreaDto
            {
                ServiceAreaName = serviceArea.name,
            };

            if (existingService != null)
            {
                var existing = existingService.ServiceAreas.FirstOrDefault(x => x.Equals(newServiceArea));

                if (existing != null)
                {
                    listServiceAreaDto.Add(existing);
                    continue;
                }
            }

            listServiceAreaDto.Add(newServiceArea);
        }

        return listServiceAreaDto;
    }

    private List<RegularScheduleDto> GetRegularSchedules(RegularSchedule[] regularSchedules, ServiceDto? existingService, long? loactionid)
    {
        if (regularSchedules == null || !regularSchedules.Any())
        {
            return new List<RegularScheduleDto>();
        }

        List<RegularScheduleDto> listRegularScheduleDto = new List<RegularScheduleDto>();

        foreach (RegularSchedule regularSchedule in regularSchedules)
        {
            var regularScheduleItem = new RegularScheduleDto
            {
                OpensAt = Helper.GetDateFromString(regularSchedule.opens_at),
                ClosesAt = Helper.GetDateFromString(regularSchedule.closes_at),
                Freq = FrequencyType.NotSet,
                ByDay = regularSchedule.weekday,
            };


            if (existingService != null && existingService.RegularSchedules != null)
            {
                RegularScheduleDto? existingItem = existingService.RegularSchedules.FirstOrDefault(x => x.Equals(regularScheduleItem));

                if (existingItem != null)
                {
                    listRegularScheduleDto.Add(existingItem);
                    continue;
                }
            }

            listRegularScheduleDto.Add(regularScheduleItem);
        }

        return listRegularScheduleDto;
    }

    private List<LocationDto> GetLocations(ServiceAtLocation[] serviceAtLocations, ServiceDto? existingService)
    {
        if (serviceAtLocations == null || !serviceAtLocations.Any())
        {
            return new List<LocationDto>();
        }

        List<LocationDto> listLocationDto = new List<LocationDto>();
        HashSet<string> hashLocationId = new HashSet<string>();

        foreach (ServiceAtLocation serviceAtLocation in serviceAtLocations)
        {
            if (serviceAtLocation == null || serviceAtLocation.location == null || hashLocationId.Contains(serviceAtLocation.location.id))
            {
                continue;
            }

            hashLocationId.Add(serviceAtLocation.location.id);

            PhysicalAddresses? physicalAddress = null;
            if (serviceAtLocation != null && serviceAtLocation.location != null && serviceAtLocation.location.physical_addresses != null && serviceAtLocation.location.physical_addresses.Any())
            {
                physicalAddress = serviceAtLocation.location.physical_addresses[0] ?? default!;
            }

            var newLocation = new LocationDto
            {
                LocationType = LocationType.NotSet,
                Name = serviceAtLocation?.location?.name ?? default!,
                Description = physicalAddress != null ? $"{physicalAddress.address1} {physicalAddress.postal_code}" : null,
                Longitude = serviceAtLocation?.location?.longitude ?? default!,
                Latitude = serviceAtLocation?.location?.latitude ?? default!,
                Address1 = physicalAddress != null ? physicalAddress.address1 : default!,
                City = physicalAddress != null ? physicalAddress.city : default!,
                PostCode = physicalAddress != null ? physicalAddress.postal_code : default!,
                Country = physicalAddress != null ? physicalAddress.country : default!,
                StateProvince = physicalAddress != null ? physicalAddress.state_province : default!,
            };

            if (existingService != null)
            {
                LocationDto? existingItem = existingService.Locations.FirstOrDefault(x => x.Equals(newLocation));

                if (existingItem != null)
                {
                    listLocationDto.Add(existingItem);
                    continue;
                }
            }


            listLocationDto.Add(newLocation);
        }

        return listLocationDto;
    }

    private List<ContactDto> GetContactDtos(Contact[] contacts, ServiceDto? existingService)
    {
        if (!contacts.Any())
        {
            return new List<ContactDto>();
        }

        var list = new List<ContactDto>();
        foreach (var contact in contacts)
        {
            string phone = default!;
            if (contact != null && contact.phones != null && contact.phones.Any())
            {
                var contactItem = contact.phones.FirstOrDefault();
                if (contactItem != null)
                {
                    phone = contactItem.number;
                }
            }

            if (string.IsNullOrEmpty(phone))
            {
                continue;
            }

            var newContact = new ContactDto
            {
                Title = contact?.title ?? default!,
                Telephone = phone,
                TextPhone = phone,
            };

            if (existingService != null)
            {
                ContactDto? existingItem = existingService.Contacts.FirstOrDefault(x => x.Equals(newContact));

                if (existingItem != null)
                {
                    list.Add(existingItem);
                    continue;
                }
            }

            list.Add(newContact);
        }

        return list;
    }

    private List<LanguageDto> GetLanguages(Language[] languages, ServiceDto? existingService)
    {
        if (languages == null || !languages.Any())
        {
            return new List<LanguageDto>();
        }

        List<LanguageDto> listLanguageDto = new List<LanguageDto>();

        foreach (Language language in languages)
        {
            var newLanguage = new LanguageDto
            {
                Name = language.name,
            };

            if (existingService != null)
            {
                LanguageDto existing = existingService.Languages.FirstOrDefault(x => x.Name == newLanguage.Name) ?? default!;
                if (existing != null)
                {
                    listLanguageDto.Add(existing);
                    continue;
                }
            }

            listLanguageDto.Add(newLanguage);
        }

        return listLanguageDto;
    }

    private async Task<List<TaxonomyDto>> GetServiceTaxonomies(ServiceTaxonomys[] serviceTaxonomies, ServiceDto? existingService)
    {
        if (serviceTaxonomies == null || !serviceTaxonomies.Any())
        {
            return new List<TaxonomyDto>();
        }

        List<TaxonomyDto> listTaxonomyDto = new List<TaxonomyDto>();

        foreach (Taxonomy taxonomy in serviceTaxonomies.Select(x => x.taxonomy))
        {
            TaxonomyDto taxonomyDto = new TaxonomyDto
            {
                Name = taxonomy.name,
                TaxonomyType = TaxonomyType.ServiceCategory
            };

            if (!_dictTaxonomies.ContainsKey(taxonomyDto.Name.ToLower()))
            {
                long result = await _organisationClientService.CreateTaxonomy(taxonomyDto);
                if (result > 0)
                {
                    _dictTaxonomies[taxonomyDto.Name.ToLower()] = taxonomyDto;
                }

            }

            if (existingService != null)
            {
                TaxonomyDto? existing = existingService.Taxonomies.FirstOrDefault(x => x.Equals(taxonomyDto));

                if (existing != null)
                {
                    listTaxonomyDto.Add(existing);
                    continue;
                }
            }

            listTaxonomyDto.Add(taxonomyDto);
        }

        return listTaxonomyDto;
    }
}
