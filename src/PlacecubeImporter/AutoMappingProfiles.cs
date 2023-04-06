using AutoMapper;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using PlacecubeImporter.Services;
using PluginBase;

namespace PlacecubeImporter;

public class AutoMappingProfiles : Profile
{
    public AutoMappingProfiles()
    {

        //CreateMap<string, int>().ConvertUsing(s => Convert.ToInt32(s));
        //CreateMap<string, long>().ConvertUsing(s => Convert.ToInt64(s));
        CreateMap<string, DateTime?>().ConvertUsing(new DateTimeTypeConverter());
        CreateMap<Phone[], string>().ConvertUsing(new PhonesToSingleConverter());

        CreateMap<AccessibilityForDisabilitiesDto, AccessibilityForDisabilities>()
            .ForMember(dest => dest.accessibility, opt => opt.MapFrom(src => src.Accessibility))
            .ReverseMap();
        CreateMap<CostOptions, CostOptionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
            .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.service_id))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.amount))
            .ForMember(dest => dest.AmountDescription, opt => opt.MapFrom(src => src.amount_description))
            .ForMember(dest => dest.Option, opt => opt.MapFrom(src => src.option))
            .ForMember(dest => dest.ValidFrom, opt => opt.MapFrom(src => Helper.GetDateFromString(src.valid_from)))
            .ForMember(dest => dest.ValidTo, opt => opt.MapFrom(src => Helper.GetDateFromString(src.valid_to)))
            .ReverseMap();
        CreateMap<Eligibility, EligibilityDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
            .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.service_id))
            .ForMember(dest => dest.MinimumAge, opt => opt.MapFrom(src => src.minimum_age))
            .ForMember(dest => dest.MaximumAge, opt => opt.MapFrom(src => src.maximum_age))
            .ForMember(dest => dest.EligibilityType, opt => opt.MapFrom(src => StringToEnum.ConvertEligibilityType(src.eligibility)))
            .ReverseMap();
        CreateMap<Funding, FundingDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
            .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.service_id))
            .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.source))
            .ReverseMap();
        CreateMap<Language, LanguageDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
            .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.service_id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.language))
            .ReverseMap();
        //CreateMap<ReviewDto, Review>().ReverseMap();
        //CreateMap<ServiceDto, Service>().ReverseMap();
        CreateMap<ServiceArea, ServiceAreaDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
            .ForMember(dest => dest.Extent, opt => opt.MapFrom(src => src.extent))
            .ForMember(dest => dest.ServiceAreaName, opt => opt.MapFrom(src => src.service_area))
            .ReverseMap();
        //CreateMap<ServiceDeliveryDto, ServiceDelivery>().ReverseMap();
        //CreateMap<OrganisationDto, Organisation>().ReverseMap();
        //CreateMap<OrganisationWithServicesDto, Organisation>().ReverseMap();
        //CreateMap<OrganisationExDto, Organisation>().ReverseMap();

        CreateMap<Location, LocationDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
            .ForMember(dest => dest.LocationType, opt => opt.MapFrom(src => LocationType.FamilyHub))
            .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.latitude))
            .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.longitude))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
            .ForMember(dest => dest.AccessibilityForDisabilities, opt => opt.MapFrom(src => src.accessibility_for_disabilities))
            .ForMember(dest => dest.Address1, opt => opt.MapFrom(src => src.physical_addresses.First().address_1))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.physical_addresses.First().city))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.physical_addresses.First().country))
            .ForMember(dest => dest.PostCode, opt => opt.MapFrom(src => src.physical_addresses.First().postal_code))
            .ForMember(dest => dest.StateProvince, opt => opt.MapFrom(src => src.physical_addresses.First().state_province))
            .ReverseMap();

        CreateMap<Location, Location>();

        CreateMap<Taxonomy, TaxonomyDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
            .ForMember(dest => dest.TaxonomyType, opt => opt.MapFrom(src => TaxonomyType.ServiceCategory))
            .ReverseMap();
        CreateMap<Taxonomy, Taxonomy>();

        CreateMap<Contact, ContactDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.title))
            .ForMember(dest => dest.Telephone, opt => opt.MapFrom(src => src.phones))
            .ForMember(dest => dest.TextPhone, opt => opt.MapFrom(src => src.phones))
            .ReverseMap();
        CreateMap<Contact, Contact>();

        CreateMap<HolidaySchedule, HolidayScheduleDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
            .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.service_id))
            .ForMember(dest => dest.LocationId, opt => opt.MapFrom(src => src.service_at_location_id))
            .ForMember(dest => dest.Closed, opt => opt.MapFrom(src => src.closed))
            .ForMember(dest => dest.OpensAt, opt => opt.MapFrom(src => src.open_at))
            .ForMember(dest => dest.ClosesAt, opt => opt.MapFrom(src => src.closes_at))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.start_date))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.end_date))
            .ReverseMap();
        CreateMap<HolidaySchedule, HolidaySchedule>();

        CreateMap<RegularSchedule, RegularScheduleDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
            .ForMember(dest => dest.OpensAt, opt => opt.MapFrom(src => src.opens_at))
            .ForMember(dest => dest.ValidFrom, opt => opt.MapFrom(src => src.valid_from))
            .ForMember(dest => dest.ValidTo, opt => opt.MapFrom(src => src.valid_to))
            .ForMember(dest => dest.ByDay, opt => opt.MapFrom(src => src.byday))
            .ForMember(dest => dest.ByMonthDay, opt => opt.MapFrom(src => src.bymonthday))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.description))
            .ForMember(dest => dest.DtStart, opt => opt.MapFrom(src => src.dtstart))
            .ForMember(dest => dest.Freq, opt => opt.MapFrom(src => src.freq))
            .ForMember(dest => dest.Interval, opt => opt.MapFrom(src => src.interval))
            .ReverseMap();
        CreateMap<RegularSchedule, RegularSchedule>();
    }
}


