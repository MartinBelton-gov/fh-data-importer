namespace SouthamptonImporter.Services;

internal class SouthamptonSimpleService
{
    public int totalElements { get; set; }
    public int totalPages { get; set; }
    public int number { get; set; }
    public int size { get; set; }
    public bool last { get; set; }
    public bool first { get; set; }
    public bool empty { get; set; }
    public Content[] content { get; set; } = default!;
}

internal class Content
{
    public string id { get; set; } = default!;
    public string name { get; set; } = default!;
    public string description { get; set; } = default!;
    public string url { get; set; } = default!;
    public string referral_notes { get; set; } = default!;
    public bool? needs_referral { get; set; } = default!;
    public string ofsted_urn { get; set; } = default!;
    public string ofsted_url { get; set; } = default!;
    public OfstedLatestInspection ofsted_latest_inspection { get; set; } = default!;
    public string cqc_url { get; set; } = default!;
    public string schedule_description { get; set; } = default!;
    public bool? eligible_for2_yo_funding { get; set; }
    public bool? eligible_for34_yo_funding { get; set; }
    public object eligible_for_haf_funding { get; set; } = default!;
    public bool? eligible_for30_h_entitlement { get; set; }
    public DateTime assured_date { get; set; }
    public ServiceAtLocation[] service_at_locations { get; set; } = default!;
    public Organization organization { get; set; } = default!;
    public CostOptions[] cost_options { get; set; } = default!;
    public RegularSchedule[] regular_schedules { get; set; } = default!;
    public Language[] languages { get; set; } = default!;
    public ServiceArea[] service_areas { get; set; } = default!;
    public SchoolPickups[] school_pickups { get; set; } = default!;
    public Link[] links { get; set; } = default!;
    public SpecialNeeds[] special_needs { get; set; } = default!;
    public Contact[] contacts { get; set; } = default!;
    public Eligibility[] eligibilitys { get; set; } = default!;
    public ServiceTaxonomys[] service_taxonomys { get; set; } = default!;
    public string status { get; set; } = default!;
    public object distance_away { get; set; } = default!;
    public object distance_away_is_service_area { get; set; } = default!;
    public object alert { get; set; } = default!;
    public LocalOffer local_offer { get; set; } = default!;
    public string cqc_current_rating { get; set; } = default!;
    public object cqc_last_inspection { get; set; } = default!;
    public string extra_section_heading { get; set; } = default!;
    public string extra_section_content { get; set; } = default!;
    public bool hide_address { get; set; }
}

internal class OfstedLatestInspection
{
    public string type { get; set; } = default!;
    public DateTime date { get; set; }
    public string overall_judgement { get; set; } = default!;
}   

internal class Organization
{
    public string id { get; set; } = default!;
    public string name { get; set; } = default!;
    public string description { get; set; } = default!;
}

internal class LocalOffer
{
    public string description { get; set; } = default!;
    public string link { get; set; } = default!;
}

internal class ServiceAtLocation
{
    public object id { get; set; } = default!;
    public Location location { get; set; } = default!;
}

internal class Location
{
    public string id { get; set; } = default!;
    public string name { get; set; } = default!;
    public PhysicalAddresses[] physical_addresses { get; set; } = default!;
    public float? latitude { get; set; }
    public float? longitude { get; set; }
    public object[] accessibilities { get; set; } = default!;
}

internal class PhysicalAddresses
{
    public string address1 { get; set; } = default!;
    public string city { get; set; } = default!;
    public string state_province { get; set; } = default!;
    public string postal_code { get; set; } = default!;
    public string country { get; set; } = default!;
    public string id { get; set; } = default!;
}

internal class CostOptions
{
    public string option { get; set; } = default!;
    public decimal amount { get; set; }
    public string amount_description { get; set; } = default!;
    public string id { get; set; } = default!;
}

internal class RegularSchedule
{
    public string weekday { get; set; } = default!;
    public string opens_at { get; set; } = default!;
    public string closes_at { get; set; } = default!;
    public string id { get; set; } = default!;
}   

internal class Language
{
    public string id { get; set; } = default!;
    public string name { get; set; } = default!;
}

internal class ServiceArea
{
    public string id { get; set; } = default!;
    public string name { get; set; } = default!;
}

internal class SchoolPickups
{
    public string id { get; set; } = default!;
    public string name { get; set; } = default!;
}

internal class Link
{
    public string label { get; set; } = default!;
    public string url { get; set; } = default!;
}

internal class SpecialNeeds
{
    public string id { get; set; } = default!;
    public string name { get; set; } = default!;
}

internal class Contact
{
    public string id { get; set; } = default!;
    public string name { get; set; } = default!;
    public string title { get; set; } = default!;
    public string email { get; set; } = default!;
    public Phone[] phones { get; set; } = default!;
}

internal class Phone
{
    public string id { get; set; } = default!;
    public string number { get; set; } = default!;
}

internal class Eligibility
{
    public string id { get; set; } = default!;
    public int? minimum_age { get; set; }
    public int? maximum_age { get; set; }
    public string eligibility { get; set; } = default!;
}

internal class ServiceTaxonomys
{
    public string id { get; set; } = default!;
    public Taxonomy taxonomy { get; set; } = default!;
}

internal class Taxonomy
{
    public string id { get; set; } = default!;
    public string name { get; set; } = default!;
    public string vocabulary { get; set; } = default!;
    public object prerequisite_taxonomies { get; set; } = default!;
}

