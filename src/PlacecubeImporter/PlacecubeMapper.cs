using AutoMapper;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using PlacecubeImporter.Services;
using PluginBase;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PlacecubeImporter;

internal class PlacecubeMapper : BaseMapper, IServiceDirectoryMapper
{
    private readonly IPlacecubeClientService _placecubeClientService;
    

    public string Name => "Placecube Mapper";
    private readonly OrganisationWithServicesDto _parentLA;
    public PlacecubeMapper(IDataInputCommand dataInputCommand, IPlacecubeClientService placecubeClientService, IOrganisationClientService organisationClientService, IMapper mapper, string adminAreaCode, string key, OrganisationWithServicesDto parentLA)
        : base(organisationClientService, adminAreaCode, parentLA, key)
    {
        _placecubeClientService = placecubeClientService;
        _parentLA = parentLA;
    }

    public async Task AddOrUpdateServices()
    {
        const int startPage = 1;
        await CreateOrganisationDictionary();
        await CreateTaxonomyDictionary();
        PlacecubeSimpleService simpleService = await _placecubeClientService.GetServicesByPage(startPage);
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
        PlacecubeSimpleService elmbridgeSimpleService = await _placecubeClientService.GetServicesByPage(page);
        foreach (var itemId in elmbridgeSimpleService.content.Select(x => x.id))
        {
            try
            {
                itemCount++;
                PlacecubeService elmbridgeService = await _placecubeClientService.GetServiceById(itemId);
                errorCount += await AddAndUpdateService(elmbridgeService);
            }
            catch
            {
                ProgressUpdate(_parentLA.Name, $"This is only a simple service id: {itemId}");
                errorCount += await AddAndUpdateSimpleService(elmbridgeSimpleService.content[itemCount]);
            }
        }

        ProgressUpdate(_parentLA.Name, $"Completed Page {page} of {totalPages} with {errorCount} errors");

    }

    private async Task<int> AddAndUpdateSimpleService(Content placecubeSimpleService)
    {
        List<string> errors = new List<string>();
        OrganisationWithServicesDto serviceDirectoryOrganisation = default!;

        if (placecubeSimpleService.organization == null)
        {
            string error = $"Organisation is null for service id: {placecubeSimpleService.id}";
            errors.Add(error);
            ProgressUpdate(_parentLA.Name, error);
            return errors.Count;
        }

        bool newOrganisation = false;
        if (_dictOrganisations.ContainsKey($"{_adminAreaCode}{placecubeSimpleService.organization.name}"))
        {
            serviceDirectoryOrganisation = _dictOrganisations[$"{_adminAreaCode}{placecubeSimpleService.organization.name}"];
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
                Name = placecubeSimpleService.organization.name,
                Description = placecubeSimpleService.organization.description.Truncate(496) ?? string.Empty,
                Logo = placecubeSimpleService.organization.logo,
                Uri = placecubeSimpleService.organization.url,
                Url = placecubeSimpleService.organization.url,
            };

            newOrganisation = true;
        }

        var serviceOwnerReferenceId = $"{_adminAreaCode.Replace("E", "")}{placecubeSimpleService.id}";

        ServiceDto? existingService = serviceDirectoryOrganisation.Services.FirstOrDefault(s => s.ServiceOwnerReferenceId == serviceOwnerReferenceId);
      
        ServiceDto serviceDto = new ServiceDto()
        {
            Id = (existingService != null) ? existingService.Id : 0,
            OrganisationId = serviceDirectoryOrganisation.Id,
            ServiceType = ServiceType.InformationSharing,
            ServiceOwnerReferenceId = serviceOwnerReferenceId,
            Name = placecubeSimpleService.name,
            Description = placecubeSimpleService.description,
            Accreditations = placecubeSimpleService.accreditations,
            AssuredDate = Helper.GetDateFromString(placecubeSimpleService.assured_date),
            AttendingAccess = StringToEnum.ConvertAttendingAccessType(placecubeSimpleService.attending_access),
            AttendingType = StringToEnum.ConvertAttendingType(placecubeSimpleService.attending_type),
            DeliverableType = StringToEnum.ConvertDeliverableType(placecubeSimpleService.deliverable_type),
            Status = StringToEnum.ConvertServiceStatusType(placecubeSimpleService.status),
            Fees = placecubeSimpleService.fees,
            CanFamilyChooseDeliveryLocation = false
        };

       
        errors = await AddOrUpdateDirectoryService(newOrganisation, serviceDirectoryOrganisation, serviceDto, serviceOwnerReferenceId, errors);
        

        foreach (string error in errors)
        {
            ProgressUpdate(_parentLA.Name, error);
        }

        return errors.Count;

    }

    private async Task<int> AddAndUpdateService(PlacecubeService placecubeService)
    {
        List<string> errors = new List<string>();

        if (placecubeService.organization == null)
        {
            string error = $"Organisation is null for service id: {placecubeService.id}";
            errors.Add(error);
            ProgressUpdate(_parentLA.Name, error);
            return errors.Count;
        }
        
        OrganisationWithServicesDto serviceDirectoryOrganisation = default!;

        bool newOrganisation = false;
        if (_dictOrganisations.ContainsKey($"{_adminAreaCode}{placecubeService.organization.name}"))
        {
            serviceDirectoryOrganisation = _dictOrganisations[$"{_adminAreaCode}{placecubeService.organization.name}"];
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
                Name = placecubeService.organization.name,
                Description = placecubeService.organization.description.Truncate(496) ?? string.Empty,
                Logo = placecubeService.organization.logo,
                Uri = placecubeService.organization.url,
                Url = placecubeService.organization.url,
                Services = new List<ServiceDto>()
            };

            newOrganisation = true;
        }

        var serviceOwnerReferenceId = $"{_adminAreaCode.Replace("E", "")}{placecubeService.id}";

        ServiceDto? existingService = serviceDirectoryOrganisation.Services.FirstOrDefault(s => s.ServiceOwnerReferenceId == serviceOwnerReferenceId);

        ServiceDto serviceDto = new ServiceDto()
        {
            Id = (existingService != null) ? existingService.Id : 0,
            OrganisationId = serviceDirectoryOrganisation.Id,
            ServiceType = ServiceType.InformationSharing,
            ServiceOwnerReferenceId = serviceOwnerReferenceId,
            Name = placecubeService.name,
            Description = placecubeService.description,
            Accreditations = placecubeService.accreditations,
            AssuredDate = Helper.GetDateFromString(placecubeService.assured_date),
            AttendingAccess = StringToEnum.ConvertAttendingAccessType(placecubeService.attending_access),
            AttendingType = StringToEnum.ConvertAttendingType(placecubeService.attending_type),
            DeliverableType = StringToEnum.ConvertDeliverableType(placecubeService.deliverable_type),
            Status = StringToEnum.ConvertServiceStatusType(placecubeService.status),
            Fees = placecubeService.fees,
            CanFamilyChooseDeliveryLocation = false,
            Eligibilities = GetEligibilityDtos(placecubeService.eligibilitys, existingService),
            CostOptions = GetCostOptionDtos(placecubeService.cost_options, existingService),
            ServiceAreas = GetServiceAreas(placecubeService.service_areas, existingService),
            Fundings = GetFundings(placecubeService.fundings, existingService),
            RegularSchedules = GetRegularSchedules(placecubeService.regular_schedules, existingService, null),
            HolidaySchedules = GetHolidaySchedules(placecubeService.holiday_schedules, existingService, null),
            Contacts = GetContactDtos(placecubeService.contacts, existingService),
            Languages = GetLanguages(placecubeService.languages, existingService),
            Taxonomies = await GetServiceTaxonomies(placecubeService.service_taxonomys, existingService),
            Locations = GetLocations(placecubeService.service_at_locations, existingService),

        };

        

        errors = await AddOrUpdateDirectoryService(newOrganisation, serviceDirectoryOrganisation, serviceDto, serviceOwnerReferenceId, errors);

        foreach (string error in errors)
        {
            ProgressUpdate(_parentLA.Name, error);
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
                MinimumAge = eligibility.minimum_age,
                MaximumAge = eligibility.maximum_age,
                EligibilityType = eligibility.maximum_age < 18 ? EligibilityType.Child : EligibilityType.NotSet
            };

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
                ValidFrom = Helper.GetDateFromString(costOption.valid_from),
                ValidTo = Helper.GetDateFromString(costOption.valid_to),
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
            var newServiceArea = new ServiceAreaDto
            {
                ServiceAreaName = serviceArea.service_area,
                Extent = serviceArea.extent
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

    private List<FundingDto> GetFundings(Funding[] fundings, ServiceDto? existingService)
    {
        if (fundings == null || !fundings.Any())
        {
            return new List<FundingDto>();
        }

        List<FundingDto> listFundingDto = new List<FundingDto>();

        if (existingService != null)
        {
            listFundingDto = existingService.Fundings.ToList();
        }

        foreach (var source in fundings.Select(x => x.source))
        {
            var newFunding = new FundingDto
            {
                Source = source,
            };

            if (existingService != null)
            {
                var existingItem = existingService.Fundings.FirstOrDefault(x => x.Source == source);
                if (existingItem != null)
                {
                    listFundingDto.Add(existingItem);
                    continue;
                }
            }

            listFundingDto.Add(newFunding);
        }

        return listFundingDto;
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
                ValidFrom = Helper.GetDateFromString(regularSchedule.valid_from),
                ValidTo = Helper.GetDateFromString(regularSchedule.valid_to),
                DtStart = regularSchedule.dtstart,
                Freq = FrequencyType.NotSet,
                Interval = regularSchedule.interval,
                ByDay = regularSchedule.byday,
                ByMonthDay = regularSchedule.bymonthday,
                Description = regularSchedule.description,
                //LocationId = loactionid

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

    private List<HolidayScheduleDto> GetHolidaySchedules(HolidaySchedule[] holidaySchedules, ServiceDto? existingService, long? loactionid)
    {
        if (holidaySchedules == null || !holidaySchedules.Any())
        {
            return new List<HolidayScheduleDto>();
        }

        List<HolidayScheduleDto> listHolidayScheduleDto = new List<HolidayScheduleDto>();

        foreach (HolidaySchedule holidaySchedule in holidaySchedules)
        {

            bool.TryParse(holidaySchedule.closed, out bool closed);

            var newHolidaySchedule = new HolidayScheduleDto
            {
                Closed = closed,
                OpensAt = Helper.GetDateFromString(holidaySchedule.open_at),
                ClosesAt = Helper.GetDateFromString(holidaySchedule.closes_at),
                LocationId = loactionid,
                StartDate = default!,
                EndDate = default!
            };

            var startDate = Helper.GetDateFromString(holidaySchedule.start_date);
            if (startDate != null)
            {
                newHolidaySchedule.StartDate = startDate.Value;
            }

            var endDate = Helper.GetDateFromString(holidaySchedule.end_date);
            if (endDate != null)
            {
                newHolidaySchedule.StartDate = endDate.Value;
            }

            if (existingService != null && existingService.HolidaySchedules != null)
            {
                HolidayScheduleDto? existingItem = existingService.HolidaySchedules.FirstOrDefault(x => x.Equals(newHolidaySchedule));

                if (existingItem != null)
                {
                    listHolidayScheduleDto.Add(existingItem);
                    continue;
                }
            }
            

            listHolidayScheduleDto.Add(newHolidaySchedule);
        }

        return listHolidayScheduleDto;
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
                Description = physicalAddress != null ? $"{physicalAddress.address_1} {physicalAddress.postal_code}" : null,
                Longitude = serviceAtLocation?.location?.longitude ?? default!,
                Latitude = serviceAtLocation?.location?.latitude ?? default!,
                Address1 = physicalAddress != null ? physicalAddress.address_1 : default!,
                City = physicalAddress != null ? physicalAddress.city : default!,
                PostCode = physicalAddress != null ? physicalAddress.postal_code : default!,
                Country = physicalAddress != null ? physicalAddress.country : default!,
                StateProvince = physicalAddress != null ? physicalAddress.state_province : default!,
            };

            if (serviceAtLocation != null)
            {
                newLocation.RegularSchedules = GetRegularSchedules(serviceAtLocation.regular_schedule, existingService, null); 
                newLocation.HolidaySchedules = GetHolidaySchedules(serviceAtLocation.holidayScheduleCollection, existingService, null);
            }

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

            if (phone.Length > 49) 
            {
                phone = ExtractPhonenumber(phone);
            }

            var newContact = new ContactDto
            {
                Title = contact?.title.Truncate(46) ?? default!,
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
                Name = language.language,
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

    private string ExtractPhonenumber(string value)
    {
       
        StringBuilder stringBuilder = new StringBuilder();
        bool readingNumber = false;
        for (int i = 0; i < value.Length; i++)
        {

            if (Char.IsDigit(value[i]))
            {
                readingNumber = true;
                stringBuilder.Append(value[i]);
            }
            else
            {
                if (readingNumber && value[i] == ' ')
                {
                    stringBuilder.Append(value[i]);
                    continue;
                }
                readingNumber = false;
            }
        }

        return stringBuilder.ToString();
    }
}
