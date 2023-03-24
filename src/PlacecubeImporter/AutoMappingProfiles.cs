using AutoMapper;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using PlacecubeImporter.Services;
using PluginBase;

namespace PlacecubeImporter;

internal class AutoMappingProfiles : Profile
{
    public AutoMappingProfiles()
    {

        CreateMap<string, int>().ConvertUsing(s => Convert.ToInt32(s));
        CreateMap<string, long>().ConvertUsing(s => Convert.ToInt64(s));
        CreateMap<string, DateTime?>().ConvertUsing(new DateTimeTypeConverter());
        CreateMap<Phone[], string>().ConvertUsing(new PhonesToSingleConverter());

        CreateMap<AccessibilityForDisabilitiesDto, AccessibilityForDisabilities>().ReverseMap();
        CreateMap<CostOptionDto, CostOptions>()
            .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.service_id, opt => opt.MapFrom(src => src.ServiceId))
            .ForMember(dest => dest.amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.amount_description, opt => opt.MapFrom(src => src.AmountDescription))
            .ForMember(dest => dest.option, opt => opt.MapFrom(src => src.Option))
            .ForMember(dest => dest.valid_from, opt => opt.MapFrom(src => src.ValidFrom))
            .ForMember(dest => dest.valid_to, opt => opt.MapFrom(src => src.ValidTo))
            .ReverseMap();
        CreateMap<EligibilityDto, Eligibility>()
            .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.service_id, opt => opt.MapFrom(src => src.ServiceId))
            .ForMember(dest => dest.minimum_age, opt => opt.MapFrom(src => src.MinimumAge))
            .ForMember(dest => dest.maximum_age, opt => opt.MapFrom(src => src.MaximumAge))
            .ForMember(dest => dest.eligibility, opt => opt.MapFrom(src => EligibilityType.NotSet))
            .ReverseMap();
        CreateMap<FundingDto, Funding>()
            .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.service_id, opt => opt.MapFrom(src => src.ServiceId))
            .ForMember(dest => dest.source, opt => opt.MapFrom(src => src.Source))
            .ReverseMap();
        CreateMap<LanguageDto, Language>()
            .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.service_id, opt => opt.MapFrom(src => src.ServiceId))
            .ForMember(dest => dest.language, opt => opt.MapFrom(src => src.Name))
            .ReverseMap();
        //CreateMap<ReviewDto, Review>().ReverseMap();
        //CreateMap<ServiceDto, Service>().ReverseMap();
        CreateMap<ServiceAreaDto, ServiceArea>()
            .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.extent, opt => opt.MapFrom(src => src.Extent))
            .ForMember(dest => dest.service_area, opt => opt.MapFrom(src => src.ServiceAreaName))
            .ReverseMap();
        //CreateMap<ServiceDeliveryDto, ServiceDelivery>().ReverseMap();
        //CreateMap<OrganisationDto, Organisation>().ReverseMap();
        //CreateMap<OrganisationWithServicesDto, Organisation>().ReverseMap();
        //CreateMap<OrganisationExDto, Organisation>().ReverseMap();

        CreateMap<LocationDto, Location>()
            .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.latitude, opt => opt.MapFrom(src => src.Latitude))
            .ForMember(dest => dest.longitude, opt => opt.MapFrom(src => src.Longitude))
            .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.Name))
            .ReverseMap();
        CreateMap<Location, Location>();

        CreateMap<TaxonomyDto, Taxonomy>()
            .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.vocabulary, opt => opt.MapFrom(src => TaxonomyType.ServiceCategory))
            .ReverseMap();
        CreateMap<Taxonomy, Taxonomy>();

        CreateMap<ContactDto, Contact>()
            .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.phones, opt => opt.MapFrom(src => src.Telephone))
            .ForMember(dest => dest.phones, opt => opt.MapFrom(src => src.TextPhone))
            .ReverseMap();
        CreateMap<Contact, Contact>();

        CreateMap<HolidayScheduleDto, HolidaySchedule>()
            .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.service_id, opt => opt.MapFrom(src => src.ServiceId))
            .ForMember(dest => dest.service_at_location_id, opt => opt.MapFrom(src => src.LocationId))
            .ForMember(dest => dest.closed, opt => opt.MapFrom(src => src.Closed))
            .ForMember(dest => dest.open_at, opt => opt.MapFrom(src => src.OpensAt))
            .ForMember(dest => dest.closes_at, opt => opt.MapFrom(src => src.ClosesAt))
            .ForMember(dest => dest.start_date, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.end_date, opt => opt.MapFrom(src => src.EndDate))
            .ReverseMap();
        CreateMap<HolidaySchedule, HolidaySchedule>();

        CreateMap<RegularScheduleDto, RegularSchedule>()
            .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.opens_at, opt => opt.MapFrom(src => src.OpensAt))
            .ForMember(dest => dest.valid_from, opt => opt.MapFrom(src => src.ValidFrom))
            .ForMember(dest => dest.valid_to, opt => opt.MapFrom(src => src.ValidTo))
            .ForMember(dest => dest.byday, opt => opt.MapFrom(src => src.ByDay))
            .ForMember(dest => dest.bymonthday, opt => opt.MapFrom(src => src.ByMonthDay))
            .ForMember(dest => dest.description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.dtstart, opt => opt.MapFrom(src => src.DtStart))
            .ForMember(dest => dest.freq, opt => opt.MapFrom(src => src.Freq))
            .ForMember(dest => dest.interval, opt => opt.MapFrom(src => src.Interval))
            .ReverseMap();
        CreateMap<RegularSchedule, RegularSchedule>();
    }
}


