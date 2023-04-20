namespace PublicPartnershipImporter.Services;

public class PublicPartnershipSimpleService
{
    public int totalelements { get; set; }
    public int totalpages { get; set; }
    public int number { get; set; }
    public int size { get; set; }
    public bool first { get; set; }
    public bool last { get; set; }
    public Content[] content { get; set; } = default!;
}

public class Content
{
    public string id { get; set; } = default!;
    public object accreditations { get; set; } = default!;
    public string assured_date { get; set; } = default!;
    public object attending_access { get; set; } = default!;
    public object attending_type { get; set; } = default!;
    public Contact[] contacts { get; set; } = default!;
    public CostOptions[] cost_options { get; set; } = default!;
    public string deliverable_type { get; set; } = default!;
    public string description { get; set; } = default!;
    public Eligibility[] eligibilitys { get; set; } = default!;
    public string email { get; set; } = default!;
    public string fees { get; set; } = default!;
    public Funding[] fundings { get; set; } = default!;
    public HolidaySchedule[] holiday_schedules { get; set; } = default!;
    public Language[] languages { get; set; } = default!;
    public string name { get; set; } = default!;
    public RegularSchedule[] regular_schedules { get; set; } = default!;
    public Review[] reviews { get; set; } = default!;
    public ServiceArea[] service_areas { get; set; } = default!;
    public ServiceAtLocation[] service_at_locations { get; set; } = default!;
    public ServiceTaxonomys[] service_taxonomys { get; set; } = default!;
    public string status { get; set; } = default!;
    public object url { get; set; } = default!;
    public Organization organization { get; set; } = default!;
}

public class Organization
{
    public string description { get; set; } = default!;
    public string id { get; set; } = default!;
    public string logo { get; set; } = default!;
    public string name { get; set; } = default!;
    public object reviews { get; set; } = default!;
    public object services { get; set; } = default!;
    public string uri { get; set; } = default!;
    public string url { get; set; } = default!;
}

public class Contact
{
    public string id { get; set; } = default!;
    public string name { get; set; } = default!;
    public string title { get; set; } = default!;
    public Phone[] phones { get; set; } = default!;
}

public class Phone
{
    public string id { get; set; } = default!;
    public string number { get; set; } = default!;
}

public class CostOptions
{
    public decimal amount { get; set; } = default!;
    public string amount_description { get; set; } = default!;
    public string id { get; set; } = default!;
    public string linkid { get; set; } = default!;
    public string option { get; set; } = default!;
    public string valid_from { get; set; } = default!;
    public string valid_to { get; set; } = default!;
}

public class Eligibility
{
    public string eligibility { get; set; } = default!;
    public string id { get; set; } = default!;
    public string linkid { get; set; } = default!;
    public string maximum_age { get; set; } = default!;
    public string minimum_age { get; set; } = default!;
}

public class Funding
{
    public string id { get; set; } = default!;
    public string source { get; set; } = default!;
}

public class Review
{
    public DateTime date { get; set; }
    public string description { get; set; } = default!;
    public string id { get; set; } = default!;
    public string score { get; set; } = default!;
    public object service { get; set; } = default!;
    public string title { get; set; } = default!;
    public string url { get; set; } = default!;
    public string widget { get; set; } = default!;
}

public class ServiceArea
{
    public string extent { get; set; } = default!;
    public string id { get; set; } = default!;
    public object linkid { get; set; } = default!;
    public string service_area { get; set; } = default!;
    public object uri { get; set; } = default!;
}

public class ServiceAtLocation
{
    public HolidaySchedule[] holiday_schedules { get; set; } = default!;
    public string id { get; set; } = default!;
    public Location location { get; set; } = default!;
    public RegularSchedule[] regular_schedule { get; set; } = default!;
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

public class Location
{
    public object accessibility_for_disabilities { get; set; } = default!;
    public string description { get; set; } = default!;
    public string id { get; set; } = default!;
    public float latitude { get; set; } = default!;
    public float longitude { get; set; } = default!;
    public string name { get; set; } = default!;
    public PhysicalAddresses[] physical_addresses { get; set; } = default!;
}

public class PhysicalAddresses
{
    public string address_1 { get; set; } = default!;
    public string city { get; set; } = default!;
    public string country { get; set; } = default!;
    public string id { get; set; } = default!;
    public string postal_code { get; set; } = default!;
    public string state_province { get; set; } = default!;
}

public class ServiceTaxonomys
{
    public string id { get; set; } = default!;
    public object linkid { get; set; } = default!;
    public Taxonomy taxonomy { get; set; } = default!;
}

public class Taxonomy
{
    public string id { get; set; } = default!;
    public string name { get; set; } = default!;
    public string vocabulary { get; set; } = default!;
    public object linktaxonomycollection { get; set; } = default!;
    public object servicetaxonomycollection { get; set; } = default!;
}

public class Language
{
    public string id { get; set; } = default!;
    public string service_id { get; set; } = default!;
    public string language { get; set; } = default!;
}
