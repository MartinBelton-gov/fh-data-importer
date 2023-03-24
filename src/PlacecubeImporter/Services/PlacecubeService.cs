namespace PlacecubeImporter.Services
{
    public class PlacecubeService
    {
        public string id { get; set; } = default!;
        public string name { get; set; } = default!;
        public string description { get; set; } = default!;
        public string url { get; set; } = default!;
        public string email { get; set; } = default!;
        public string status { get; set; } = default!;
        public string fees { get; set; } = default!;
        public string accreditations { get; set; } = default!;
        public string deliverable_type { get; set; } = default!;
        public string attending_type { get; set; } = default!;
        public string attending_access { get; set; } = default!;
        public string pc_attendingAccess_additionalInfo { get; set; } = default!;
        public string assured_date { get; set; } = default!;
        public ServiceArea[] service_areas { get; set; }  = new ServiceArea[0];
        public Funding[] fundings { get; set; } = new Funding[0];
        public RegularSchedule[] regular_schedules { get; set; } = new RegularSchedule[0];
        public Eligibility[] eligibilitys { get; set; } = new Eligibility[0];
        public ServiceAtLocation[] service_at_locations { get; set; } = new ServiceAtLocation[0];
        public CostOptions[] cost_options { get; set; } = new CostOptions[0];
        public object[] reviews { get; set; } = new object[0];
        public Organization organization { get; set; } = default!;
        public Contact[] contacts { get; set; } = new Contact[0];
        public HolidaySchedule[] holiday_schedules { get; set; } = new HolidaySchedule[0];
        public ServiceTaxonomys[] service_taxonomys { get; set; } = new ServiceTaxonomys[0];
        public Language[] languages { get; set; } = new Language[0];
        public PcMetadata pc_metadata { get; set; } = default!;
        public PcTargetaudience[] pc_targetAudience { get; set; } = new PcTargetaudience[0];
    }

    public class CostOptions
    {
        public string id { get; set; } = default!;
        public string service_id { get; set; } = default!;
        public string valid_from { get; set; } = default!;
        public string valid_to { get; set; } = default!;
        public string option { get; set; } = default!;
        public decimal amount { get; set; } = default!;
        public string amount_description { get; set; } = default!;
    }

    public class Eligibility
    {
        public string id { get; set; } = default!;
        public string service_id { get; set; } = default!;
        public string eligibility { get; set; } = default!;
        public int minimum_age { get; set; } = default!;
        public int maximum_age { get; set; } = default!;
    }

    public class Funding
    {
        public string id { get; set; } = default!;
        public string service_id { get; set; } = default!;
        public string source { get; set; } = default!;
    }   

    public class Language
    {
        public string id { get; set; } = default!;
        public string service_id { get; set; } = default!;
        public string language { get; set; } = default!;
    }

    public class ServiceArea
    {
        public string service_area { get; set; } = default!;
        public string extent { get; set; } = default!;
        public string id { get; set; } = default!;
    }

    public class ServiceAtLocation
    {
        public RegularSchedule[] regular_schedule { get; set; } = new RegularSchedule[0];
        public HolidaySchedule[] holidayScheduleCollection { get; set; } = new HolidaySchedule[0];
        public Location location { get; set; } = default!;
    }

    public class HolidaySchedule
    {
        public string id { get; set; } = default!;
        public string service_id { get; set; } = default!;
        public string service_at_location_id { get; set; } = default!;
        public string closed { get; set; } = default!;
        public string open_at { get; set; } = default!;
        public string closes_at { get; set; } = default!;
        public string start_date { get; set; } = default!;
        public string end_date { get; set; } = default!;

    }

    public class Location
    {
        public AccessibilityForDisabilities[] accessibility_for_disabilities { get; set; } = default!;
        public PhysicalAddresses[] physical_addresses { get; set; } = default!;
        public string id { get; set; } = default!;
        public float latitude { get; set; } = default!;
        public float longitude { get; set; } = default!;
        public string name { get; set; } = default!;
    }

    public class AccessibilityForDisabilities
    {
        public string accessibility { get; set; } = default!;
    }

    public class PhysicalAddresses
    {
        public string address_1 { get; set; } = default!;
        public string postal_code { get; set; } = default!;
        public string state_province { get; set; } = default!;
        public string city { get; set; } = default!;
        public string country { get; set; } = default!;
    }

    public class RegularSchedule
    {
        public string closes_at { get; set; } = default!;
        public string opens_at { get; set; } = default!;
        public string valid_from { get; set; } = default!;
        public string valid_to { get; set; } = default!;
        public string byday { get; set; } = default!;
        public string bymonthday { get; set; } = default!;
        public string description { get; set; } = default!;
        public string dtstart { get; set; } = default!;
        public string freq { get; set; } = default!;
        public string id { get; set; } = default!;
        public string interval { get; set; } = default!;
    }

    public class Contact
    {
        public string id { get; set; } = default!;
        public string name { get; set; } = default!;
        public Phone[] phones { get; set; } = new Phone[0];
        public string title { get; set; } = default!;
    }

    public class Phone
    {
        public string number { get; set; } = default!;
    }

    public class ServiceTaxonomys
    {
        public string id { get; set; } = default!;
        public Taxonomy taxonomy { get; set; } = default!;
    }

    public class Taxonomy
    {
        public string id { get; set; } = default!;
        public string name { get; set; } = default!;
        public string vocabulary { get; set; } = default!;
    }

}
