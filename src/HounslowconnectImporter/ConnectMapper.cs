using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using HounslowconnectImporter.Services;
using PluginBase;

namespace HounslowconnectImporter;

public class ConnectMapper : BaseMapper, IServiceDirectoryMapper
{
    public string Name => "Open Connect Mapper";

    private readonly IConnectClientService<ConnectService> _connectClientService;
    private readonly OrganisationWithServicesDto _parentOrganisation;
    private readonly IPostCodeCacheLookupService _postCodeCacheLookupService;
    public ConnectMapper(IDataInputCommand dataInputCommand, IPostCodeCacheLookupService postCodeCacheLookupService, IConnectClientService<ConnectService> connectClientService, IOrganisationClientService organisationClientService, string adminAreaCode, string key, OrganisationWithServicesDto parentLA)
        : base(organisationClientService, adminAreaCode, parentLA, key)
    {
        _postCodeCacheLookupService = postCodeCacheLookupService;
        _connectClientService = connectClientService;
        _parentOrganisation = parentLA;
    }

    public async Task AddOrUpdateServices()
    {
        int currentPage = 1;
        int errors = 0;
        await CreateOrganisationDictionary();
        await CreateTaxonomyDictionary();
        ConnectService connectService = await _connectClientService.GetServices(string.Empty);

        foreach (var item in connectService.data)
        {
            errors += await AddAndUpdateService(item);
        }
        ProgressUpdate(_parentOrganisation.Name, $"Completed Page {currentPage} with {errors} errors");
        

        do
        {
            currentPage++;
            string urlParam = GetUrlParameters(connectService.links.next);
            if (string.IsNullOrEmpty(urlParam))
                break;

            connectService = await _connectClientService.GetServices(urlParam);

            if (!connectService.data.Any())
                break;

            foreach (var item in connectService.data)
            {               
                errors += await AddAndUpdateService(item);
            }
            ProgressUpdate(_parentOrganisation.Name, $"Completed Page {currentPage} with {errors} errors");

        }
        while (!string.IsNullOrEmpty(connectService.links.next));

        ProgressUpdate(_parentOrganisation.Name, "Completed Import");
    }

    public async Task<OtherDetails> GetLocationDetails(string organisationId, string serviceId)
    {
        OtherDetails otherDetails = new OtherDetails();
        otherDetails.Organisation = await _connectClientService.GetOrganisation(organisationId);
        otherDetails.ServiceLocations = await _connectClientService.GetServiceLocation(serviceId);
        foreach (var serviceLocationsDatum in otherDetails.ServiceLocations.data)
        {
            var location = await _connectClientService.GetLocation(serviceLocationsDatum.location_id);
            if (location != null)
                otherDetails.Locations.Add(location);
        }

        return otherDetails;
    }

    private async Task<int> AddAndUpdateService(Datum data)
    {
        OtherDetails locationDetails = await GetLocationDetails(data.organisation_id, data.id);

        List<string> errors = new List<string>();

        if (data.organisation_id == null || locationDetails.Organisation == null)
        {
            string error = $"Organisation is null for service id: {data.id}";
            errors.Add(error);
            ProgressUpdate(_parentOrganisation.Name, error);
            return errors.Count;
        }

        OrganisationWithServicesDto serviceDirectoryOrganisation = _parentOrganisation;

        bool newOrganisation = false;

        string adminAreaCode = _adminAreaCode;

        Location location = locationDetails?.Locations?.FirstOrDefault() ?? default!;
        if (location != null)
        {
            adminAreaCode = await _postCodeCacheLookupService.GetAdminCode(location.data.postcode, _adminAreaCode, location.data.lon, location.data.lat);
        }

        if (_dictOrganisations.ContainsKey($"{adminAreaCode}{locationDetails?.Organisation.data.name}"))
        {
            serviceDirectoryOrganisation = _dictOrganisations[$"{adminAreaCode}{locationDetails?.Organisation.data.name}"];
            //Get latest
            serviceDirectoryOrganisation = await _organisationClientService.GetOrganisationById(serviceDirectoryOrganisation.Id.ToString());
        }
        else
        {
            const OrganisationType organisationType = OrganisationType.VCFS;
            serviceDirectoryOrganisation = new OrganisationWithServicesDto
            {
                AdminAreaCode = adminAreaCode,
                OrganisationType = organisationType,
                Name = locationDetails?.Organisation?.data?.name ?? default!,
                Description = locationDetails?.Organisation.data.description.Truncate(496) ?? string.Empty,
                Logo = string.Empty, //todo find logo
                Uri = locationDetails?.Organisation.data.url,
                Url = locationDetails?.Organisation.data.url,
                Services = new List<ServiceDto>()
            };

            newOrganisation = true;
        }

        var serviceOwnerReferenceId = $"{_adminAreaCode.Replace("E", "")}{data.id}";

        ServiceDto? existingService = serviceDirectoryOrganisation.Services.FirstOrDefault(s => s.ServiceOwnerReferenceId == serviceOwnerReferenceId);

        ServiceDto serviceDto = new ServiceDto()
        {
            Id = (existingService != null) ? existingService.Id : 0,
            OrganisationId = serviceDirectoryOrganisation.Id,
            ServiceType = ServiceType.InformationSharing,
            ServiceOwnerReferenceId = serviceOwnerReferenceId,
            Name = data.name,
            Description = data.description,
            Accreditations = default!,
            AssuredDate = default!,
            AttendingAccess = default!, // todo StringToEnum.ConvertAttendingAccessType(placecubeService.attending_access),
            AttendingType = default!, // todo StringToEnum.ConvertAttendingType(placecubeService.attending_type),
            DeliverableType = DeliverableType.Information, // todo StringToEnum.ConvertDeliverableType(placecubeService.deliverable_type),
            Status = StringToEnum.ConvertServiceStatusType(data.status),
            Fees = data.fees_text,
            CanFamilyChooseDeliveryLocation = false,
            Eligibilities = GetEligibilityDtos(data.eligibility_types.custom, existingService),
            CostOptions = GetCostOptionDtos(data, existingService),
            ServiceAreas = GetServiceAreas(serviceDirectoryOrganisation.Name, existingService),
            //Fundings = GetFundings(placecubeService.fundings, existingService),
            //RegularSchedules = GetRegularSchedules(placecubeService.regular_schedules, existingService, null),
            //HolidaySchedules = GetHolidaySchedules(placecubeService.holiday_schedules, existingService, null),
            Contacts = GetContactDtos(data, existingService),
            Languages = GetLanguageDtos(data, existingService),
            Taxonomies = await GetServiceTaxonomies(data.category_taxonomies, existingService),
            Locations = GetLocations(locationDetails?.ServiceLocations.data ?? default!, locationDetails?.Locations ?? default!, existingService),

        };

        

        errors = await AddOrUpdateDirectoryService(newOrganisation, serviceDirectoryOrganisation, serviceDto, serviceOwnerReferenceId, errors);

        foreach (string error in errors)
        {
            ProgressUpdate(_parentOrganisation.Name, error);
        }

        return errors.Count;
    }

    private string GetUrlParameters(string url)
    {
        if (string.IsNullOrEmpty(url))
            return string.Empty;

        var parts = url.Split("?");
        if (parts != null && parts.Length == 2)
            return parts[1];

        return string.Empty;
    }

    private List<ServiceAreaDto> GetServiceAreas(string serviceArea, ServiceDto? existingService)
    {
        if (string.IsNullOrEmpty(serviceArea))
        {
            return new List<ServiceAreaDto>();
        }

        List<ServiceAreaDto> listServiceAreaDto = new List<ServiceAreaDto>();

        
        var newServiceArea = new ServiceAreaDto
        {
            ServiceAreaName = serviceArea,
        };

        if (existingService != null)
        {
            var existing = existingService.ServiceAreas.FirstOrDefault(x => x.Equals(newServiceArea));

            if (existing != null)
            {
                listServiceAreaDto.Add(existing);
                return listServiceAreaDto;
            }
        }

        listServiceAreaDto.Add(newServiceArea);
       

        return listServiceAreaDto;
    }

    private List<LanguageDto> GetLanguageDtos(Datum data, ServiceDto? existingService)
    {
        if (data.eligibility_types != null && data.eligibility_types.custom != null && !string.IsNullOrEmpty(data.eligibility_types.custom.language))
        {
            return new List<LanguageDto> { new LanguageDto { Name = data.eligibility_types.custom.language, ServiceId = (existingService != null) ? existingService.Id : 0 } };
        }

        return new List<LanguageDto>();
    }

    private List<EligibilityDto> GetEligibilityDtos(Custom custom, ServiceDto? existingService)
    {
        if (custom == null)
        {
            return new List<EligibilityDto>();
        }
            
        
        List<EligibilityDto> listEligibilityDto = new List<EligibilityDto>();

        EligibilityDto newEligibility = default!;
        if (!string.IsNullOrEmpty(custom.age_group))
        {
            newEligibility = GetAgeGroupEligibility(custom.age_group);
            
        }

        if (!string.IsNullOrEmpty(custom.disability))
        {
            newEligibility = GetDisabilityEligibility(custom.age_group);

        }

        if (newEligibility  == null) 
        {
            return new List<EligibilityDto>();
        }

        if (existingService != null)
        {
            var existingItem = existingService.Eligibilities.FirstOrDefault(x => x.Equals(newEligibility));

            if (existingItem != null)
            {
                listEligibilityDto.Add(existingItem);
                return listEligibilityDto;
            }
        }

        listEligibilityDto.Add(newEligibility);


        return listEligibilityDto;
    }

    private EligibilityDto GetAgeGroupEligibility(string value)
    {
        var ageRange = value.Split('-');
        var minimumAge = 0;
        var maximumAge = 0;
        if (ageRange.Length == 2)
        {
            int.TryParse(new string(ageRange[0].Where(char.IsDigit).ToArray()), out minimumAge);
            int.TryParse(new string(ageRange[1].Where(char.IsDigit).ToArray()), out maximumAge);

            EligibilityType eligibilityType = minimumAge >= 18 ? EligibilityType.Adult : EligibilityType.Child;
            if (eligibilityType == EligibilityType.Child && minimumAge > 12 && minimumAge < 18)
            {
                eligibilityType = EligibilityType.Teen;
            }

            return new EligibilityDto
            {
                MinimumAge = minimumAge,
                MaximumAge = maximumAge,
                EligibilityType = eligibilityType
            };
        }
        if (value.Contains("Over"))
        {
            int.TryParse(new string(ageRange[0].Where(char.IsDigit).ToArray()), out minimumAge);

            return new EligibilityDto
            {
                MinimumAge = minimumAge,
                MaximumAge = 125,
                EligibilityType = maximumAge < 18 ? EligibilityType.Child : EligibilityType.Adult
            };
        }
        if (value.Contains("Pansion"))
        {
            return new EligibilityDto
            {
                MinimumAge = 66,
                MaximumAge = 125,
                EligibilityType = maximumAge < 18 ? EligibilityType.Child : EligibilityType.Adult
            };
        }

        return default!;
    }

    private EligibilityDto GetDisabilityEligibility(string value)
    {
        
        switch (value)
        {
            case "Adults aged 18+ with learning disabilities":
                return new EligibilityDto()
                {
                    EligibilityType = EligibilityType.Adult,
                    MinimumAge = 18,
                    MaximumAge = 125,
                };
            case "Families and carers of anyone with SEN":
                return new EligibilityDto()
                {
                    EligibilityType = EligibilityType.Family,
                    MinimumAge = 0,
                    MaximumAge = 125,
                };
            case "Carers":
                return new EligibilityDto()
                {
                    EligibilityType = EligibilityType.Family,
                    MinimumAge = 0,
                    MaximumAge = 125,
                };
            case "Special educational needs":
                return new EligibilityDto()
                {
                    EligibilityType = EligibilityType.Child,
                    MinimumAge = 0,
                    MaximumAge = 25,
                };
            default:
                return default!;
        }
    }

    private List<RegularScheduleDto> GetRegularSchedules(RegularOpeningHours[] regularOpeningHours, ServiceDto? existingService, long? loactionid)
    {
        if (regularOpeningHours == null || !regularOpeningHours.Any())
        {
            return new List<RegularScheduleDto>();
        }

        List<RegularScheduleDto> listRegularScheduleDto = new List<RegularScheduleDto>();

        

        foreach (RegularOpeningHours regularOpeningHour in regularOpeningHours)
        {
            DayOfWeek dayOfWeek = (DayOfWeek)regularOpeningHour.weekday;

            var regularScheduleItem = new RegularScheduleDto
            {
                OpensAt = Helper.GetDateFromString(regularOpeningHour.opens_at),
                ClosesAt = Helper.GetDateFromString(regularOpeningHour.closes_at),
                Freq = FrequencyType.NotSet,
                ByDay = dayOfWeek.ToString()
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

    private List<HolidayScheduleDto> GetHolidaySchedules(HolidayOpeningHours[] holidaySchedules, ServiceDto? existingService, long? loactionid)
    {
        if (holidaySchedules == null || !holidaySchedules.Any())
        {
            return new List<HolidayScheduleDto>();
        }

        List<HolidayScheduleDto> listHolidayScheduleDto = new List<HolidayScheduleDto>();

        foreach (HolidayOpeningHours holidaySchedule in holidaySchedules)
        {
            var newHolidaySchedule = new HolidayScheduleDto
            {
                Closed = holidaySchedule.is_closed,
                OpensAt = Helper.GetDateFromString(holidaySchedule.opens_at),
                ClosesAt = Helper.GetDateFromString(holidaySchedule.closes_at),
                LocationId = loactionid,
                StartDate = Helper.GetDateFromString(holidaySchedule.starts_at) ?? default!,
                EndDate = Helper.GetDateFromString(holidaySchedule.ends_at) ?? default!,
            };

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

    private List<LocationDto> GetLocations(ServiceLocationsDatum[] serviceAtLocations, List<Location> apiLocations, ServiceDto? existingService)
    {
        if (serviceAtLocations == null || !serviceAtLocations.Any())
        {
            return new List<LocationDto>();
        }

        List<LocationDto> listLocationDto = new List<LocationDto>();

        HashSet<string> hashLocationId = new HashSet<string>();

        foreach (ServiceLocationsDatum serviceAtLocation in serviceAtLocations)
        {
            if (serviceAtLocation == null || serviceAtLocation.location_id == null || apiLocations == null || !apiLocations.Any(x => x.data.id == serviceAtLocation.location_id) || hashLocationId.Contains(serviceAtLocation.location_id))
            {
                continue;
            }

            hashLocationId.Add(serviceAtLocation.location_id);

            Location apiLocation = apiLocations.Find(x => x.data.id == serviceAtLocation.location_id) ?? default!;

            var newLocation = new LocationDto
            {
                LocationType = LocationType.NotSet,
                Name = serviceAtLocation.name ?? default!,
                Description = apiLocation.data.address_line_1 != null ? $"{apiLocation.data.address_line_1} {apiLocation.data.postcode}" : default!,
                Longitude = apiLocation.data.lon,
                Latitude = apiLocation.data.lat,
                Address1 = apiLocation.data.address_line_1 != null ? apiLocation.data.address_line_1 : default!,
                Address2 = apiLocation.data.address_line_2 != null ? apiLocation.data.address_line_2 : default!,
                City = apiLocation.data.city != null ? apiLocation.data.city : default!,
                PostCode = apiLocation.data.postcode != null ? apiLocation.data.postcode : default!,
                Country = apiLocation.data.country != null ? apiLocation.data.country : default!,
                StateProvince = apiLocation.data.county != null ? apiLocation.data.county : default!,
            };

            if (serviceAtLocation != null)
            {
                newLocation.RegularSchedules = GetRegularSchedules(serviceAtLocation.regular_opening_hours, existingService, null);
                newLocation.HolidaySchedules = GetHolidaySchedules(serviceAtLocation.holiday_opening_hours, existingService, null);
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

    private async Task<List<TaxonomyDto>> GetServiceTaxonomies(CategoryTaxonomies[] serviceTaxonomies, ServiceDto? existingService)
    {
        if (serviceTaxonomies == null || !serviceTaxonomies.Any())
        {
            return new List<TaxonomyDto>();
        }

        List<TaxonomyDto> listTaxonomyDto = new List<TaxonomyDto>();

        foreach (CategoryTaxonomies taxonomy in serviceTaxonomies)
        {
            if (string.IsNullOrEmpty(taxonomy.name))
            {
                continue;
            }
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

    private List<CostOptionDto> GetCostOptionDtos(Datum data, ServiceDto? existingService)
    {
        if (data == null || data.is_free)
        {
            return new List<CostOptionDto>();
        }
        
        List<CostOptionDto> listCostOptionDto = new List<CostOptionDto>();

        var newCostOption = new CostOptionDto
        {
            AmountDescription = data.fees_text
        };

        if (!string.IsNullOrEmpty(data.fees_url)) 
        {
            newCostOption.AmountDescription += $" Further details: {data.fees_url}";
        }

        if (existingService != null)
        {
            var existingItem = existingService.CostOptions.FirstOrDefault(x => x.Equals(newCostOption));

            if (existingItem != null)
            {
                listCostOptionDto.Add(existingItem);
                return listCostOptionDto;
            }
        }

        listCostOptionDto.Add(newCostOption);
       

        return listCostOptionDto;
    }

    private List<ContactDto> GetContactDtos(Datum data, ServiceDto? existingService)
    {
        if (data == null || string.IsNullOrEmpty(data.contact_name))
        {
            return new List<ContactDto>();
        }

        var list = new List<ContactDto>();
        
        var newContact = new ContactDto
        {
            Name = data.contact_name ?? default!,
            Telephone = data.contact_phone ?? " ",
            Email = data.contact_email ?? " ",
        };

        if (existingService != null)
        {
            ContactDto? existingItem = existingService.Contacts.FirstOrDefault(x => x.Equals(newContact));

            if (existingItem != null)
            {
                list.Add(existingItem);
                return list;
            }
        }

        list.Add(newContact);
        
        return list;
    }

}