using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using HounslowconnectImporter.Services;
using PluginBase;

namespace HounslowconnectImporter;

public interface IConnectMapper
{
    Task AddOrUpdateServices();
}

public class ConnectMapper : BaseMapper, IConnectMapper
{
    public string Name => "Open Active Mapper";

    private readonly IConnectClientService<ConnectService> _connectClientService;
    private readonly OrganisationWithServicesDto _parentOrganisation;
    private readonly IPostCodeCacheLookupService _postCodeCacheLookupService;
    public ConnectMapper(IPostCodeCacheLookupService postCodeCacheLookupService, IConnectClientService<ConnectService> connectClientService, IOrganisationClientService organisationClientService, string adminAreaCode, string key, OrganisationWithServicesDto parentLA)
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
        Console.WriteLine($"Completed Page {currentPage} with {errors} errors");

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
            Console.WriteLine($"Completed Page {currentPage} with {errors} errors");

        }
        while (!string.IsNullOrEmpty(connectService.links.next));

        Console.WriteLine("Completed Import");
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
            Console.WriteLine(error);
            return errors.Count;
        }

        OrganisationWithServicesDto serviceDirectoryOrganisation = default!;

        bool newOrganisation = false;
        if (_dictOrganisations.ContainsKey($"{_adminAreaCode}{locationDetails.Organisation.data.name}"))
        {
            serviceDirectoryOrganisation = _dictOrganisations[$"{_adminAreaCode}{locationDetails.Organisation.data.name}"];
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
                Name = locationDetails.Organisation.data.name,
                Description = locationDetails.Organisation.data.description.Truncate(496) ?? string.Empty,
                Logo = string.Empty, //todo find logo
                Uri = locationDetails.Organisation.data.url,
                Url = locationDetails.Organisation.data.url,
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
            DeliverableType = default!, // todo StringToEnum.ConvertDeliverableType(placecubeService.deliverable_type),
            Status = StringToEnum.ConvertServiceStatusType(data.status),
            //Fees = placecubeService.fees,
            CanFamilyChooseDeliveryLocation = false,
            //Eligibilities = GetEligibilityDtos(placecubeService.eligibilitys, existingService),
            //CostOptions = GetCostOptionDtos(placecubeService.cost_options, existingService),
            //ServiceAreas = GetServiceAreas(placecubeService.service_areas, existingService),
            //Fundings = GetFundings(placecubeService.fundings, existingService),
            //RegularSchedules = GetRegularSchedules(placecubeService.regular_schedules, existingService, null),
            //HolidaySchedules = GetHolidaySchedules(placecubeService.holiday_schedules, existingService, null),
            //Contacts = GetContactDtos(placecubeService.contacts, existingService),
            //Languages = GetLanguages(placecubeService.languages, existingService),
            //Taxonomies = await GetServiceTaxonomies(placecubeService.service_taxonomys, existingService),
            //Locations = GetLocations(placecubeService.service_at_locations, existingService),

        };

        errors = await AddOrUpdateDirectoryService(newOrganisation, serviceDirectoryOrganisation, serviceDto, serviceOwnerReferenceId, errors);

        foreach (string error in errors)
        {
            Console.WriteLine(error);
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

    //private List<EligibilityDto> GetEligibilityDtos(EligibilityTypes eligibilityTypes, ServiceDto? existingService)
    //{
    //    List<EligibilityDto> listEligibilityDto = new List<EligibilityDto>();

        
    //    var newEligibility = new EligibilityDto
    //    {
    //        MinimumAge = eligibility.minimum_age,
    //        MaximumAge = eligibility.maximum_age,
    //        EligibilityType = eligibility.maximum_age < 18 ? EligibilityType.Child : EligibilityType.NotSet
    //    };

    //    if (newEligibility.MinimumAge >= 18)
    //    {
    //        newEligibility.EligibilityType = EligibilityType.Adult;
    //    }

    //    if (existingService != null)
    //    {
    //        var existingItem = existingService.Eligibilities.FirstOrDefault(x => x.Equals(newEligibility));

    //        if (existingItem != null)
    //        {
    //            listEligibilityDto.Add(existingItem);
    //            return listEligibilityDto;
    //        }
    //    }

    //    listEligibilityDto.Add(newEligibility);
        

    //    return listEligibilityDto;
    //}
}