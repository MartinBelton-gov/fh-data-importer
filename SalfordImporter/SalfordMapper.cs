using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Microsoft.Extensions.Hosting;
using PluginBase;
using SalfordImporter.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace SalfordImporter;


internal class SalfordMapper : BaseMapper
{
    public string Name => "Buckinghamshire Mapper";

    private readonly ISalfordClientService _salfordClientService;
    public SalfordMapper(ISalfordClientService salfordClientService, IOrganisationClientService organisationClientService, string adminAreaCode, string key, OrganisationWithServicesDto parentLA)
        : base(organisationClientService, adminAreaCode, parentLA, key)
    {
        _salfordClientService = salfordClientService;
    }

    public async Task AddOrUpdateServices()
    {
        int errors = 0;
        await CreateOrganisationDictionary();
        await CreateTaxonomyDictionary();
        SalfordService salfordService = await _salfordClientService.GetServices();
        int totalPages = salfordService.totalRecords;
        int recordNumber = 0;
        foreach (var salfordRecord in salfordService.records)
        {
            recordNumber++;
            errors = await AddAndUpdateService(salfordRecord);
            Console.WriteLine($"Completed Record {recordNumber} of {totalPages} with {errors} errors");
        }
    }

    private async Task<int> AddAndUpdateService(SalfordRecord salfordRecord)
    {
        List<string> errors = new List<string>();

        OrganisationWithServicesDto serviceDirectoryOrganisation = default!;

        bool newOrganisation = false;
        if (_dictOrganisations.ContainsKey($"{_adminAreaCode}{salfordRecord.venue_name}"))
        {
            serviceDirectoryOrganisation = _dictOrganisations[$"{_adminAreaCode}{salfordRecord.venue_name}"];
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
                Name = !string.IsNullOrEmpty(salfordRecord.venue_name) ? salfordRecord.venue_name: salfordRecord.public_address_1,
                Description = salfordRecord.description,
                Logo = null,
                Uri = salfordRecord.recordUri,
                Url = salfordRecord.recordUri,
                Services = new List<ServiceDto>()
            };

            newOrganisation = true;
        }

        var serviceOwnerReferenceId = $"{_adminAreaCode.Replace("E", "")}{salfordRecord.externalId}";

        ServiceDto? existingService = serviceDirectoryOrganisation.Services.FirstOrDefault(s => s.ServiceOwnerReferenceId == serviceOwnerReferenceId);

        ServiceDto serviceDto = new ServiceDto()
        {
            Id = (existingService != null) ? existingService.Id : 0,
            OrganisationId = serviceDirectoryOrganisation.Id,
            ServiceType = ServiceType.InformationSharing,
            ServiceOwnerReferenceId = serviceOwnerReferenceId,
            Name = !string.IsNullOrEmpty(salfordRecord.venue_name) ? salfordRecord.venue_name : salfordRecord.public_address_1,
            Description = salfordRecord.description,
            Accreditations = null,
            AssuredDate = null,
            AttendingAccess = AttendingAccessType.NotSet,
            AttendingType = AttendingType.NotSet,
            DeliverableType = DeliverableType.NotSet,
            Status = ServiceStatusType.Active,
            Fees = null,
            CanFamilyChooseDeliveryLocation = false,
            Eligibilities = GetEligibilityDtos(salfordRecord, existingService),
            //CostOptions = GetCostOptionDtos(content.cost_options, existingService),
            //RegularSchedules = GetRegularSchedules(content.regular_schedules, existingService, null),
            Contacts = GetContactDtos(salfordRecord, existingService),
            //Taxonomies = await GetServiceTaxonomies(content.taxonomies, existingService),
            Locations = GetLocations(salfordRecord, existingService),

        };

        errors = await AddOrUpdateDirectoryService(newOrganisation, serviceDirectoryOrganisation, serviceDto, serviceOwnerReferenceId, errors);

        foreach (string error in errors)
        {
            Console.WriteLine(error);
        }

        return errors.Count;
    }

    private List<EligibilityDto> GetEligibilityDtos(SalfordRecord salfordRecord, ServiceDto? existingService)
    {
        List<EligibilityDto> listEligibilityDto = new List<EligibilityDto>();

        if ((object)salfordRecord.agerange == null)
        {
            return listEligibilityDto;
        }

        string result = salfordRecord.agerange.ToString();
        result = result.Replace("[", "").Replace("]", "") ?? string.Empty;
        if (!string.IsNullOrEmpty(result)) 
        {
            string[] parts = result.Split(',');
            foreach (string item in parts)
            {
                List<int> numbers = ExtractNumbers(item);
                if (numbers.Any() && numbers.Count == 2)
                {
                    var newEligibility = new EligibilityDto
                    {
                        MinimumAge = numbers.ElementAt(0),
                        MaximumAge = numbers.ElementAt(1),
                        EligibilityType = EligibilityType.NotSet
                    };

                    if (existingService != null)
                    {
                        var existingItem = existingService.Eligibilities.FirstOrDefault(x => x.Equals(newEligibility));
                        if (existingItem != null)
                        {
                            listEligibilityDto.Add(existingItem);
                        }
                    }

                    listEligibilityDto.Add(newEligibility);
                }

            }
        }

        return listEligibilityDto;
    }

    private List<int> ExtractNumbers(string value)
    {
        List<int> numbers = new List<int>();
        StringBuilder stringBuilder = new StringBuilder();
        bool readingNumber = false;
        for(int i = 0; i < value.Length; i++)
        {

            if (Char.IsDigit(value[i]))
            {
                readingNumber = true;
                stringBuilder.Append(value[i]);
            }
            else
            {
                readingNumber = false;
            }

            if (!readingNumber && stringBuilder.Length > 0 && int.TryParse(stringBuilder.ToString(), out int number))
            {
                numbers.Add(number);
                stringBuilder.Clear();
            }
        }

        return numbers;
    }

    private string[] ConvertObjectToStringArray(object value)
    {
        string result = value.ToString() ?? string.Empty;
        result = result.Replace("[", "").Replace("]", "") ?? string.Empty;
        return result.Split(',');
    }

    private List<ContactDto> GetContactDtos(SalfordRecord salfordRecord, ServiceDto? existingService)
    {
        if (string.IsNullOrEmpty(salfordRecord.venue_postcode))
        {
            return new List<ContactDto>();
        }

        var list = new List<ContactDto>();

        string[] phoneNumber = ConvertObjectToStringArray(salfordRecord.contact_telephone);

        var newContact = new ContactDto
        {
            Title = salfordRecord.title,
            Name = salfordRecord.contact_name,
            Telephone = phoneNumber != null && phoneNumber.Any() ? phoneNumber[0] : string.Empty,
        };

        if (existingService != null)
        {
            ContactDto? existingItem = existingService.Contacts.FirstOrDefault(x => x.Equals(newContact));

            if (existingItem != null)
            {
                list.Add(existingItem);
            }
        }

        list.Add(newContact);

        return list;
    }

    private List<LocationDto> GetLocations(SalfordRecord salfordRecord, ServiceDto? existingService)
    {
        if (string.IsNullOrEmpty(salfordRecord.venue_postcode))
        {
            return new List<LocationDto>();
        }

        var newLocation = new LocationDto
        {
            LocationType = LocationType.NotSet,
            Name = !string.IsNullOrEmpty(salfordRecord.venue_name) ? salfordRecord.venue_name : salfordRecord.public_address_1,
            //Description = physicalAddress != null ? $"{physicalAddress.address_1} {physicalAddress.postal_code}" : null,
            Longitude = 0.0D,
            Latitude = 0.0D,
            Address1 = !string.IsNullOrEmpty(salfordRecord.public_address_1) ? salfordRecord.public_address_1 : default!,
            Address2 = !string.IsNullOrEmpty(salfordRecord.public_address_2) ? salfordRecord.public_address_2 : default!,
            City = !string.IsNullOrEmpty(salfordRecord.public_address_3) ? salfordRecord.public_address_3 : default!,
            StateProvince = !string.IsNullOrEmpty(salfordRecord.public_address_4) ? salfordRecord.public_address_4 : default!,
            PostCode = !string.IsNullOrEmpty(salfordRecord.venue_postcode) ? salfordRecord.venue_postcode : default!,
            Country = "England",
        };

        return new List<LocationDto>()
        { 
            newLocation
        };
    }

}
