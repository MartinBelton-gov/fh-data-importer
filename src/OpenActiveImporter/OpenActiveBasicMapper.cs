using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using OpenActiveImporter.Services;
using PluginBase;

namespace OpenActiveImporter;

internal class OpenActiveBasicMapper : BaseMapper, IServiceDirectoryMapper
{
    public string Name => "Open Active Basic Mapper";

    private readonly IOpenActiveClientService<OpenActiveBasicService> _openActiveClientService;
    private readonly OrganisationWithServicesDto _parentOrganisation;
    private readonly IPostCodeCacheLookupService _postCodeCacheLookupService;
    public OpenActiveBasicMapper(IPostCodeCacheLookupService postCodeCacheLookupService, IOpenActiveClientService<OpenActiveBasicService> openActiveClientService, IOrganisationClientService organisationClientService, string adminAreaCode, string key, OrganisationWithServicesDto parentLA)
        : base(organisationClientService, adminAreaCode, parentLA, key)
    {
        _postCodeCacheLookupService = postCodeCacheLookupService;
        _openActiveClientService = openActiveClientService;
        _parentOrganisation = parentLA;
    }

    public async Task AddOrUpdateServices()
    {
        int currentPage = 1;
        int errors = 0;
        await CreateOrganisationDictionary();
        await CreateTaxonomyDictionary();
        OpenActiveBasicService openActiveService = await _openActiveClientService.GetServices(string.Empty);

        foreach (var item in openActiveService.items)
        {
            if (item.state == "deleted")
                continue;

            errors += await AddAndUpdateService(item.data);
        }
        Console.WriteLine($"Completed Page {currentPage} with {errors} errors");

        do
        {
            currentPage++;
            string urlParam = GetUrlParameters(openActiveService.next);
            if (string.IsNullOrEmpty(urlParam))
                break;

            openActiveService = await _openActiveClientService.GetServices(urlParam);

            if (!openActiveService.items.Any())
                break;

            foreach (var item in openActiveService.items)
            {
                if (item.state == "deleted")
                    continue;

                errors += await AddAndUpdateService(item.data);
            }
            Console.WriteLine($"Completed Page {currentPage} with {errors} errors");

        }
        while (!string.IsNullOrEmpty(openActiveService.next));

        Console.WriteLine("Completed Import");
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

    private async Task<int> AddAndUpdateService(BasicData data)
    {
        List<string> errors = new List<string>();

        bool newOrganisation = false;
        OrganisationWithServicesDto serviceDirectoryOrganisation = _parentOrganisation;

        string adminAreaCode = _adminAreaCode;

        if (_dictOrganisations.ContainsKey($"{adminAreaCode}{data.organizer.name}"))
        {
            serviceDirectoryOrganisation = _dictOrganisations[$"{adminAreaCode}{data.organizer.name}"];
            //Get latest
            serviceDirectoryOrganisation = await _organisationClientService.GetOrganisationById(serviceDirectoryOrganisation.Id.ToString());
        }
        else
        {
            string postcode = data.location.address.postalCode;
            adminAreaCode = await _postCodeCacheLookupService.GetAdminCode(postcode, _adminAreaCode, data.location.geo.longitude, data.location.geo.latitude);

            const OrganisationType organisationType = OrganisationType.Company;
            serviceDirectoryOrganisation = new OrganisationWithServicesDto
            {
                AdminAreaCode = adminAreaCode,
                OrganisationType = organisationType,
                Name = data.organizer.name,
                Description = data.organizer.name.Truncate(496) ?? string.Empty,
                Url = data.organizer.url,
            };

            newOrganisation = true;
        }

        var serviceOwnerReferenceId = $"{_adminAreaCode.Replace("E", "")}{data.organizer.name}";

        ServiceDto? existingService = serviceDirectoryOrganisation.Services.FirstOrDefault(s => s.ServiceOwnerReferenceId == serviceOwnerReferenceId);

        List<string> amienties = new List<string>();
        if (data != null && data.location != null && data.location.amenityFeature != null && data.location.amenityFeature.Any())
        {
            foreach (Amenityfeature amenityfeature in data.location.amenityFeature)
            {
                if (amenityfeature.value) 
                {
                    amienties.Add(amenityfeature.name);
                }
            }
        }

        ServiceDto serviceDto = new ServiceDto()
        {
            Id = (existingService != null) ? existingService.Id : 0,
            OrganisationId = serviceDirectoryOrganisation.Id,
            ServiceType = ServiceType.InformationSharing,
            ServiceOwnerReferenceId = serviceOwnerReferenceId,
            Name = data?.name ?? string.Empty,
            Description = (amienties.Any()) ? $"Has the folowing: {string.Join(',', amienties)}" : data?.name ?? string.Empty,
            Accreditations = null,
            AssuredDate = null,
            AttendingAccess = AttendingAccessType.NotSet,
            AttendingType = AttendingType.Venue,
            DeliverableType = DeliverableType.NotSet,
            Status = StringToEnum.ConvertServiceStatusType("active"),
            Fees = null,
            CanFamilyChooseDeliveryLocation = false,
            ServiceDeliveries = new List<ServiceDeliveryDto> { new ServiceDeliveryDto { Name = ServiceDeliveryType.InPerson } },
            Eligibilities = GetEligibilities(data, existingService),
            CostOptions = GetCostOptionDtos(data?.offers, existingService),
            RegularSchedules = new List<RegularScheduleDto>(),
            Contacts = GetContacts(data, existingService),
            Taxonomies = new List<TaxonomyDto>()
            {
                _dictTaxonomies["activities, clubs and groups"]
            },
            Locations = GetLocations(data, existingService),
        };

        errors = await AddOrUpdateDirectoryService(newOrganisation, serviceDirectoryOrganisation, serviceDto, serviceOwnerReferenceId, errors);

        foreach (string error in errors)
        {
            Console.WriteLine(error);
        }

        return errors.Count;
    }

    private List<EligibilityDto> GetEligibilities(BasicData? data, ServiceDto? existingService)
    {
        if (data == null || data.offers == null)
        {
            return new List<EligibilityDto>();
        }

        List<EligibilityDto> listEligibilities = new List<EligibilityDto>();

        List<Agerange> ageranges = data.offers.Select(x => x.ageRange).ToList();


        foreach(Agerange ageRange in ageranges) 
        {
            if (ageRange == null)
                continue;

            EligibilityDto newEligibility = new EligibilityDto
            {
                MaximumAge = ageRange.maxValue,
                MinimumAge = ageRange.minValue,
                EligibilityType = EligibilityType.Family
            };

            if (newEligibility.MinimumAge >= 18)
                newEligibility.EligibilityType = EligibilityType.Adult;

            if (newEligibility.MaximumAge <= 18)
                newEligibility.EligibilityType = EligibilityType.Child;

            listEligibilities.Add(newEligibility);

            //bool added = false;
            //if (existingService != null)
            //{
            //    EligibilityDto? existingItem = existingService.Eligibilities.FirstOrDefault(x => x.Equals(newEligibility));

            //    if (existingItem != null)
            //    {
            //        listEligibilities.Add(existingItem);
            //        added = true;
            //    }
            //}

            //if (!added)
            //{
            //    listEligibilities.Add(newEligibility);
            //}
        }

        return listEligibilities;
    }

    private List<LocationDto> GetLocations(BasicData? data, ServiceDto? existingService)
    {
        if (data == null || data.location == null || data.location.address == null || string.IsNullOrEmpty(data.location.address.postalCode))
        {
            return new List<LocationDto>();
        }

        List<LocationDto> listLocationDto = new List<LocationDto>();

        var newLocation = new LocationDto
        {
            LocationType = LocationType.NotSet,
            Name = data.location.name,
            Description = $"{data.location.name} - {data.location.address.postalCode}",
            Longitude = data.location.geo.longitude,
            Latitude = data.location.geo.latitude,
            Address1 = data.location.address.streetAddress,
            Address2 = data.location.address.addressLocality,
            City = data.location.address.addressRegion,
            PostCode = data.location.address.postalCode,
            Country = "England",
            StateProvince = data.location.address.addressRegion,
        };

        if (string.IsNullOrEmpty(newLocation.Address1) && !string.IsNullOrEmpty(newLocation.Address2))
        {
            newLocation.Address1 = newLocation.Address2;
            newLocation.Address2 = null;
        }

        if (string.IsNullOrEmpty(newLocation.Address1) && string.IsNullOrEmpty(newLocation.Address2))
        {
            newLocation.Address1 = " ";
        }

        if (string.IsNullOrEmpty(newLocation.StateProvince))
        {
            newLocation.StateProvince = " ";
        }

        newLocation.RegularSchedules = GetRegularSchedules(data.eventSchedule, existingService);

        bool added = false;
        if (existingService != null)
        {
            LocationDto? existingItem = existingService.Locations.FirstOrDefault(x => x.Equals(newLocation));

            if (existingItem != null)
            {
                listLocationDto.Add(existingItem);
                added = true;
            }
        }

        if (!added)
        {
            listLocationDto.Add(newLocation);
        }

        return listLocationDto;
    }

    private List<RegularScheduleDto> GetRegularSchedules(Eventschedule[] eventschedules, ServiceDto? existingService)
    {
        if (eventschedules == null || !eventschedules.Any())
        {
            return new List<RegularScheduleDto>();
        }

        List<RegularScheduleDto> listRegularScheduleDto = new List<RegularScheduleDto>();

        foreach (Eventschedule eventschedule in eventschedules)
        {
            List<string> days = new List<string>();
            foreach (string item in eventschedule.byDay)
            {
                days.Add(item.Replace("https://schema.org/", ""));
            }

            var regularScheduleItem = new RegularScheduleDto
            {
                OpensAt = Helper.GetDateFromString(eventschedule.startTime),
                //ClosesAt = Helper.GetDateFromString(eventschedule.endTime),
                ValidFrom = Helper.GetDateFromString(eventschedule.startDate),
                ValidTo = Helper.GetDateFromString(eventschedule.endDate),
                ByDay = string.Join(',', days),
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

    private List<CostOptionDto> GetCostOptionDtos(Offer[]? offers, ServiceDto? existingService)
    {
        if (offers == null || !offers.Any())
        {
            return new List<CostOptionDto>();
        }

        List<CostOptionDto> listCostOptionDto = new List<CostOptionDto>();

        foreach (Offer offer in offers)
        {
            var newCostOption = new CostOptionDto
            {
                AmountDescription = $"{offer.name} £{offer.price}",
                Amount = (decimal)offer.price
            };

            bool added = false;
            if (existingService != null)
            {
                var existingItem = existingService.CostOptions.FirstOrDefault(x => x.Equals(newCostOption));

                if (existingItem != null)
                {
                    listCostOptionDto.Add(existingItem);
                    added = true;
                }
            }

            if (!added)
            {
                listCostOptionDto.Add(newCostOption);
            }
        }

        return listCostOptionDto;
    }

    private List<ContactDto> GetContacts(BasicData? data, ServiceDto? existingService)
    {
        if (data == null || data.location == null || string.IsNullOrEmpty(data.location.telephone))
        {
            return new List<ContactDto>();
        }

        List<ContactDto> listContactDto = new List<ContactDto>();

        var newContact = new ContactDto
        {
            Name = data.location.name,
            Telephone = data.location.telephone,
        };

        bool added = false;
        if (existingService != null)
        {
            ContactDto? existingItem = existingService.Contacts.FirstOrDefault(x => x.Equals(newContact));

            if (existingItem != null)
            {
                listContactDto.Add(existingItem);
                added = true;
            }
        }

        if (!added)
        {
            listContactDto.Add(newContact);
        }

        return listContactDto;
    }
}
