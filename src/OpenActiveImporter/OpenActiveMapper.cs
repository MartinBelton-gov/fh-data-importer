using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using OpenActiveImporter.Services;
using PluginBase;
using System.Web;

namespace OpenActiveImporter;

internal class OpenActiveMapper : BaseMapper
{
    public string Name => "Open Active Mapper";

    private readonly IOpenActiveClientService _openActiveClientService;
    private readonly OrganisationWithServicesDto _parentOrganisation;
    private readonly IPostCodeCacheLookupService _postCodeCacheLookupService;
    public OpenActiveMapper(IPostCodeCacheLookupService postCodeCacheLookupService, IOpenActiveClientService openActiveClientService, IOrganisationClientService organisationClientService, string adminAreaCode, string key, OrganisationWithServicesDto parentLA)
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
        OpenActiveService openActiveService = await _openActiveClientService.GetServices(string.Empty);

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

    private async Task<int> AddAndUpdateService(Data data)
    {
        List<string> errors = new List<string>();

        bool newOrganisation = false;
        OrganisationWithServicesDto serviceDirectoryOrganisation = _parentOrganisation;

        string adminAreaCode = _adminAreaCode; //await _postCodeCacheLookupService.GetAdminCode(data.superEvent.location.address.postalCode, _adminAreaCode, data.superEvent.location.geo.longitude, data.superEvent.location.geo.longitude);

        if (_dictOrganisations.ContainsKey($"{adminAreaCode}{data.organizer.name}"))
        {
            serviceDirectoryOrganisation = _dictOrganisations[$"{adminAreaCode}{data.organizer.name}"];
            //Get latest
            serviceDirectoryOrganisation = await _organisationClientService.GetOrganisationById(serviceDirectoryOrganisation.Id.ToString());
        }
        else
        {
            const OrganisationType organisationType = OrganisationType.Company;
            serviceDirectoryOrganisation = new OrganisationWithServicesDto
            {
                AdminAreaCode = adminAreaCode,
                OrganisationType = organisationType,
                Name = data.organizer.name,
                Description = data.organizer.name.Truncate(496) ?? string.Empty,
            };

            newOrganisation = true;
        }

        var serviceOwnerReferenceId = $"{_adminAreaCode.Replace("E", "")}{data.organizer.name}";

        ServiceDto? existingService = serviceDirectoryOrganisation.Services.FirstOrDefault(s => s.ServiceOwnerReferenceId == serviceOwnerReferenceId);

        List<string> activities = new List<string>();
        if (data.superEvent.superEvent.activity != null && data.superEvent.superEvent.activity.Any())
        {
            foreach (Activity activity in data.superEvent.superEvent.activity)
            {
                activities.Add(activity.prefLabel);
            }
        }



        ServiceDto serviceDto = new ServiceDto()
        {
            Id = (existingService != null) ? existingService.Id : 0,
            OrganisationId = serviceDirectoryOrganisation.Id,
            ServiceType = ServiceType.InformationSharing,
            ServiceOwnerReferenceId = serviceOwnerReferenceId,
            Name = data.name,
            Description = string.Join(',', activities),
            Accreditations = null,
            AssuredDate = null,
            AttendingAccess = AttendingAccessType.NotSet,
            AttendingType = AttendingType.Venue,
            DeliverableType = DeliverableType.NotSet,
            Status = StringToEnum.ConvertServiceStatusType("active"),
            Fees = null,
            CanFamilyChooseDeliveryLocation = false,
            ServiceDeliveries = new List<ServiceDeliveryDto> { new ServiceDeliveryDto { Name = ServiceDeliveryType.InPerson } },
            Eligibilities = new List<EligibilityDto>(),
            CostOptions = GetCostOptionDtos(data.offers, existingService),
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

    private List<LocationDto> GetLocations(Data data, ServiceDto? existingService)
    {
        if (string.IsNullOrEmpty(data.superEvent.location.address.postalCode))
        {
            return new List<LocationDto>();
        }

        List<LocationDto> listLocationDto = new List<LocationDto>();

        var newLocation = new LocationDto
        {
            LocationType = LocationType.NotSet,
            Name = data.superEvent.location.name,
            Description = $"{data.superEvent.location.name} - {data.superEvent.location.address.postalCode}",
            Longitude = data.superEvent.location.geo.longitude,
            Latitude = data.superEvent.location.geo.latitude,
            Address1 = data.superEvent.location.address.streetAddress,
            Address2 = data.superEvent.location.address.addressLocality,
            City = data.superEvent.location.address.addressRegion,
            PostCode = data.superEvent.location.address.postalCode,
            Country = "England",
            StateProvince = data.superEvent.location.address.addressRegion,
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

        newLocation.RegularSchedules = GetRegularSchedules(data.superEvent.eventSchedule, existingService);

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
        if (eventschedules  == null || !eventschedules.Any())
        {
            return new List<RegularScheduleDto>();
        }

        List<RegularScheduleDto> listRegularScheduleDto = new List<RegularScheduleDto>();

        foreach(Eventschedule eventschedule in eventschedules) 
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

    private List<CostOptionDto> GetCostOptionDtos(Offer[] offers, ServiceDto? existingService)
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

    private List<ContactDto> GetContacts(Data data, ServiceDto? existingService)
    {
        if (string.IsNullOrEmpty(data.superEvent.location.telephone))
        {
            return new List<ContactDto>();
        }

        List<ContactDto> listContactDto = new List<ContactDto>();

        var newContact = new ContactDto
        {
            Name = data.superEvent.location.name,
            Telephone = data.superEvent.location.telephone,
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