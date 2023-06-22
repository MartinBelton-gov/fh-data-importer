using FamilyHubs.DataImporter.Infrastructure;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using PluginBase;
using SalfordImporter.Services;
using System.Text;
using System.Text.Json;

namespace SalfordImporter;


internal class SalfordMapper : BaseMapper
{
    public string Name => "Salford Mapper";

    private readonly IDataInputCommand _dataInputCommand;
    private readonly ISalfordClientService _salfordClientService;
    private readonly IPostCodeCacheLookupService _postCodeCacheLookupService;
    public SalfordMapper(IDataInputCommand dataInputCommand,ISalfordClientService salfordClientService, IOrganisationClientService organisationClientService, IPostCodeCacheLookupService postCodeCacheLookupService, string adminAreaCode, string key, OrganisationWithServicesDto parentLA)
        : base(organisationClientService, adminAreaCode, parentLA, key)
    {
        _dataInputCommand = dataInputCommand;
        _salfordClientService = salfordClientService;
        _postCodeCacheLookupService = postCodeCacheLookupService;
    }

    public async Task AddOrUpdateServices()
    {
        int errors = 0;
        await CreateOrganisationDictionary();
        await CreateTaxonomyDictionary();
        SalfordService salfordService = await _salfordClientService.GetServices(null,null);
        int totalrecords = salfordService.totalRecords;
        int recordNumber = 0;
        foreach (var salfordRecord in salfordService.records)
        {
            recordNumber++;
            errors = await AddAndUpdateService(salfordRecord);
            Console.WriteLine($"Completed Record {recordNumber} of {totalrecords} with {errors} errors");
            _dataInputCommand.Progress = $"Completed Record {recordNumber} of {totalrecords} with {errors} errors";
        }

        while (recordNumber + 1 < totalrecords)
        {
            if ((recordNumber + 100) < totalrecords)
            {
                salfordService = await _salfordClientService.GetServices(recordNumber + 1, 100);
            }
            else
            {
                salfordService = await _salfordClientService.GetServices(recordNumber + 1, totalrecords-recordNumber);
            }

            foreach (var salfordRecord in salfordService.records)
            {
                recordNumber++;
                errors = await AddAndUpdateService(salfordRecord);
                Console.WriteLine($"Completed Record {recordNumber} of {totalrecords} with {errors} errors");
            }
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
                Name = salfordRecord.title, //!string.IsNullOrEmpty(salfordRecord.venue_name) ? salfordRecord.venue_name: salfordRecord.title,
                Description = !string.IsNullOrEmpty(salfordRecord.description) ? salfordRecord.description : salfordRecord.title,
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
            Name = salfordRecord.title,
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
            CostOptions = GetCostOptionDtos(salfordRecord, existingService),
            //RegularSchedules = GetRegularSchedules(content.regular_schedules, existingService, null),
            Contacts = GetContactDtos(salfordRecord, existingService),
            //Taxonomies = await GetServiceTaxonomies(content.taxonomies, existingService),
            Locations = await GetLocations(salfordRecord, existingService),

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
                        EligibilityType = numbers.ElementAt(1) < 18 ? EligibilityType.Child : EligibilityType.NotSet
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

    private decimal ExtractCost(string value)
    {
        if (string.IsNullOrEmpty(value) || string.Compare("Free",value,StringComparison.OrdinalIgnoreCase) == 0) 
        {
            return 0.0M;
        }
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
                if(readingNumber && value[i] == '.')
                {
                    stringBuilder.Append(value[i]);
                    continue;
                }
                break;
            }

            
        }

        if (stringBuilder.Length > 0 && decimal.TryParse(stringBuilder.ToString(), out decimal number))
        {
            return number;
        }

        return 0.0M;
    }

    private string[] ConvertObjectToStringArray(object value)
    {
        if (value == null)
        {
            return new string[0];
        }
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
            Name = salfordRecord.contact_name,
            Telephone = phoneNumber != null && phoneNumber.Any() ? phoneNumber[0] : string.Empty,
            Email = salfordRecord.contact_email
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

    private async Task<List<LocationDto>> GetLocations(SalfordRecord salfordRecord, ServiceDto? existingService)
    {
        if (string.IsNullOrEmpty(salfordRecord.venue_postcode))
        {
            return new List<LocationDto>();
        }

        (double latitude, double logtitude) = await GetCoordinates(salfordRecord.venue_postcode);

        string[] phoneNumber = ConvertObjectToStringArray(salfordRecord.contact_telephone);

        var newContact = new ContactDto
        {
            Name = salfordRecord.contact_name,
            Telephone = phoneNumber != null && phoneNumber.Any() ? phoneNumber[0] : string.Empty,
            Email = salfordRecord.contact_email
        };

        var newLocation = new LocationDto
        {
            LocationType = LocationType.NotSet,
            Name = !string.IsNullOrEmpty(salfordRecord.venue_name) ? salfordRecord.venue_name : salfordRecord.public_address_1,
            //Description = physicalAddress != null ? $"{physicalAddress.address_1} {physicalAddress.postal_code}" : null,
            Longitude = latitude,
            Latitude = logtitude,
            Address1 = !string.IsNullOrEmpty(salfordRecord.public_address_1) ? salfordRecord.public_address_1 : " ",
            Address2 = !string.IsNullOrEmpty(salfordRecord.public_address_2) ? salfordRecord.public_address_2 : " ",
            City = !string.IsNullOrEmpty(salfordRecord.public_address_3) ? salfordRecord.public_address_3 : " ",
            StateProvince = !string.IsNullOrEmpty(salfordRecord.public_address_4) ? salfordRecord.public_address_4 : " ",
            PostCode = !string.IsNullOrEmpty(salfordRecord.venue_postcode) ? salfordRecord.venue_postcode : " ",
            Country = "England",
            RegularSchedules = GetRegularSchedules(salfordRecord.date_activity_period, existingService),
            Contacts = new List<ContactDto>()
            {
                newContact
            }
        };

        return new List<LocationDto>()
        { 
            newLocation
        };
    }

    private List<RegularScheduleDto> GetRegularSchedules(DateActivityPeriod dateActivityPeriod, ServiceDto? existingService)
    {
        if (dateActivityPeriod == null || !dateActivityPeriod.weekdays.Any()) 
        { 
            return new List<RegularScheduleDto>();
        }

        List<RegularScheduleDto> listRegularScheduleDto = new List<RegularScheduleDto>();

        foreach (var day in dateActivityPeriod.weekdays)
        {
            var regularScheduleItem = new RegularScheduleDto
            {
                Freq = FrequencyType.NotSet,
                ByDay = GetDayOfTheWeek(day),
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

    private string GetDayOfTheWeek(string value)
    {
        if (int.TryParse(value, out int dayOfTheWeek))
        {
            switch (dayOfTheWeek)
            {
                case 1:
                    return "Sunday";
                case 2:
                    return "Monday";
                case 3:
                    return "Tuesday";
                case 4:
                    return "Wednesday";
                case 5:
                    return "Thursday";
                case 6:
                    return "Friday";
                case 7:
                    return "Saturday";

            }
        }

        return string.Empty;
    }

    private List<CostOptionDto> GetCostOptionDtos(SalfordRecord salfordRecord, ServiceDto? existingService)
    {
        if (salfordRecord.cost_table == null) 
        {
            return new List<CostOptionDto>();
        }

        List<CostOptionDto> costOptionDtos = new List<CostOptionDto>();

        string json = string.Empty;
        try
        {
            json = salfordRecord.cost_table.ToString() ?? string.Empty;
        }
        catch
        {
            return new List<CostOptionDto>();
        }

        if (string.IsNullOrEmpty(json))
        {
            return new List<CostOptionDto>();
        }

        try
        {
            CostTable[] results = JsonSerializer.Deserialize<CostTable[]>(json) ?? new CostTable[0];
            if (results.Any())
            {
                foreach (CostTable costTable in results)
                {
                    var costOptionDto = new CostOptionDto
                    {
                        AmountDescription = salfordRecord.cost_description,
                        Amount = ExtractCost(costTable.cost_amount),
                        Option = costTable?.cost_type?.displayName
                    };

                    if (existingService != null && existingService.CostOptions != null)
                    {
                        CostOptionDto? existingItem = existingService.CostOptions.FirstOrDefault(x => x.Equals(costOptionDto));

                        if (existingItem != null)
                        {
                            costOptionDtos.Add(existingItem);
                            continue;
                        }
                    }

                    costOptionDtos.Add(costOptionDto);
                }
            }
        }
        catch
        {
            return new List<CostOptionDto>();
        }


        return new List<CostOptionDto>();
    }

    private async Task<(double latitude, double logtitude)> GetCoordinates(string postCode)
    {
        if (string.IsNullOrEmpty(postCode)) 
        {
            Console.WriteLine($"Empty postcode return zero lat/long");
            return (0.0, 0.0);
        }

        return await _postCodeCacheLookupService.GetCoordinates(postCode);

    }

}
