using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using PluginBase;
using SportEngland.Services;
using System.Web;

namespace SportEngland;

internal class SportEnglandImportMapper : BaseMapper
{
    public string Name => "Sport England Mapper";

    private readonly ISportEnglandClientService _sportEnglandClientService;
    private readonly OrganisationWithServicesDto _sportEngland;
    private readonly IPostcodeLocationClientService _postcodeLocationClientService;
    public SportEnglandImportMapper(IPostcodeLocationClientService postcodeLocationClientService, ISportEnglandClientService sportEnglandClientService, IOrganisationClientService organisationClientService, string adminAreaCode, string key, OrganisationWithServicesDto parentLA)
        : base(organisationClientService, adminAreaCode, parentLA, key)
    {
        _postcodeLocationClientService = postcodeLocationClientService;
        _sportEnglandClientService = sportEnglandClientService;
        _sportEngland = parentLA;
    }

    public async Task AddOrUpdateServices()
    {
        const int maxRetry = 3;
        int currentPage = 1;
        int errors = 0;
        await CreateOrganisationDictionary();
        await CreateTaxonomyDictionary();
        SportEnglandModel sportEnglandModel = await _sportEnglandClientService.GetServices(9500000,100);

        foreach (var item in sportEnglandModel.items)
        {
            if (item.state == "deleted")
                continue;

            errors += await AddAndUpdateService(item.data);
        }
        Console.WriteLine($"Completed Page {currentPage} with {errors} errors");

        do
        {
            currentPage++;
            long changeNumber = GetChangeNumber(sportEnglandModel.next);
            if (changeNumber <= 0)
                break;
            int retry = 0;
            while (retry < maxRetry)
            {
                try
                {
                    sportEnglandModel = await _sportEnglandClientService.GetServices(changeNumber, 100);
                    break;
                }
                catch
                {
                    System.Threading.Thread.Sleep(1000);
                    retry++;
                    if (retry > maxRetry)
                    {
                        Console.WriteLine($"Failed to get page");
                        return;

                    }
                    Console.WriteLine($"Doing retry: {retry}");
                }
            }

            foreach (var item in sportEnglandModel.items)
            {
                if (item.state == "deleted")
                    continue;

                errors += await AddAndUpdateService(item.data);
            }
            Console.WriteLine($"Completed Page {currentPage} with {errors} errors");

        } 
        while (!string.IsNullOrEmpty(sportEnglandModel.next));

        Console.WriteLine("Completed Import");
    }

    private long GetChangeNumber(string url)
    {
        if (string.IsNullOrEmpty(url))
            return 0;

        Uri myUri = new Uri(url);
        string param1 = HttpUtility.ParseQueryString(myUri.Query).Get("afterChangeNumber") ?? string.Empty;
        long.TryParse(param1, out long value);
        return value;
    }

    private async Task<int> AddAndUpdateService(Data data)
    {
        List<string> errors = new List<string>();

        bool newOrganisation = false;
        OrganisationWithServicesDto serviceDirectoryOrganisation = _sportEngland;

        if (data.activePartnershipName != null)
        {
            string adminAreaCode = await GetAdminCode(data.postcode);


            if (_dictOrganisations.ContainsKey($"{adminAreaCode}{data.activePartnershipName}"))
            {
                serviceDirectoryOrganisation = _dictOrganisations[$"{adminAreaCode}{data.activePartnershipName}"];
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
                    Name = data.activePartnershipName,
                    Description = data.activePartnershipName.Truncate(496) ?? string.Empty,
                };

                newOrganisation = true;
            }
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
            Description = data.name,
            Accreditations = null,
            AssuredDate = null,
            AttendingAccess = AttendingAccessType.NotSet,
            AttendingType = AttendingType.Venue,
            DeliverableType = DeliverableType.NotSet,
            Status = StringToEnum.ConvertServiceStatusType("active"),
            Fees = null,
            CanFamilyChooseDeliveryLocation = false,
            Eligibilities = new List<EligibilityDto>(),
            CostOptions = GetCostOptionDtos(data, existingService),
            RegularSchedules = new List<RegularScheduleDto>(), 
            Contacts = GetContactDtos(data.contacts, existingService),
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

    private async Task<string> GetAdminCode(string postcode)
    {
        if (string.IsNullOrEmpty(postcode))
            return _adminAreaCode;

        try
        {
            PostcodesIoResponse postcodesIoResponse = await _postcodeLocationClientService.LookupPostcode(postcode);
            if (postcodesIoResponse == null)
            {
                return _adminAreaCode;
            }

            if (postcodesIoResponse.Result.Codes.admin_county == "E99999999")
                return postcodesIoResponse.Result.Codes.admin_district;

            return postcodesIoResponse.Result.Codes.admin_county;
        }
        catch
        {
            return _adminAreaCode;
        }

        
    }

    private List<CostOptionDto> GetCostOptionDtos(Data data, ServiceDto? existingService)
    {
        var accessibility = data.facilities.Select(x => x.accessibility).FirstOrDefault();
        if (accessibility == null || accessibility.name == "Free Public Access")
        {
            return new List<CostOptionDto>();
        }

        List<CostOptionDto> listCostOptionDto = new List<CostOptionDto>();

        if (accessibility.name.Contains("Pay") || accessibility.name.Contains("Membership"))
        {
            var newCostOption = new CostOptionDto
            {
                AmountDescription = accessibility.name,
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

    private List<LocationDto> GetLocations(Data data, ServiceDto? existingService)
    {
        if (string.IsNullOrEmpty(data.postcode))
        {
            return new List<LocationDto>();
        }

        List<LocationDto> listLocationDto = new List<LocationDto>();

        string name = (data.buildingNumber > 0) ? $"{data.buildingNumber.ToString()} {data.buildingName}" : data.buildingName;
        

        var newLocation = new LocationDto
        {
            LocationType = LocationType.NotSet,
            Name = data.name,
            Description = $"{name} {data.postcode}",
            Longitude = data.longitude,
            Latitude = data.latitude,
            Address1 = name,
            Address2 = data.thoroughfareName,
            City = data.postTown,
            PostCode = data.postcode,
            Country = "England",
            StateProvince = data.dependentLocality,
        };

        if (string.IsNullOrEmpty(newLocation.Address1) && !string.IsNullOrEmpty(newLocation.Address2))
        {
            newLocation.Address1 = newLocation.Address2;
            newLocation.Address2 = null;
        }

        if (string.IsNullOrEmpty(newLocation.StateProvince))
        {
            newLocation.StateProvince = " ";
        }

        newLocation.RegularSchedules = GetRegularSchedules(data.facilities, existingService);

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

    private List<RegularScheduleDto> GetRegularSchedules(Facility[] facilities, ServiceDto? existingService)
    {
        if (facilities == null || !facilities.Any())
        {
            return new List<RegularScheduleDto>();
        }

        List<RegularScheduleDto> listRegularScheduleDto = new List<RegularScheduleDto>();

        foreach (Facility facility in facilities)
        {
            if (facility.openingTimes != null && facility.openingTimes.Any())
            {
                foreach(Openingtime openingtime in facility.openingTimes)
                {
                    var regularScheduleItem = new RegularScheduleDto
                    {
                        OpensAt = Helper.GetDateFromString(openingtime.openingTime),
                        ClosesAt = Helper.GetDateFromString(openingtime.closingTime),
                        ValidFrom = facility.startDate,
                        Interval = openingtime.periodOpenFor.name
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
                
            }
        }

        return listRegularScheduleDto;
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
            var newContact = new ContactDto
            {
                Title =  contact.title.Truncate(46) ?? default!,
                Telephone = contact.telephone,
                TextPhone = contact.telephone,
                Email = contact.email,
                Name = $"{contact.forename} {contact.surname}",
                Url = contact.website
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
}
