namespace SouthamptonImporter.Services;


internal class SouthamptonService
{
    public string id { get; set; } = default!;
    public string name { get; set; } = default!;
    public string description { get; set; } = default!;
    public string url { get; set; } = default!;
    public string referral_notes { get; set; } = default!;
    public object needs_referral { get; set; } = default!;
    public string ofsted_urn { get; set; } = default!;
    public string ofsted_url { get; set; } = default!;
    public object ofsted_latest_inspection { get; set; } = default!;
    public string cqc_url { get; set; } = default!;
    public string schedule_description { get; set; } = default!;
    public object eligible_for2_yo_funding { get; set; } = default!;
    public bool eligible_for34_yo_funding { get; set; } = default!;
    public object eligible_for_haf_funding { get; set; } = default!;
    public object eligible_for30_h_entitlement { get; set; } = default!;
    public DateTime assured_date { get; set; }
    public ServiceAtLocation[] service_at_locations { get; set; } = default!;
    public Organization organization { get; set; } = default!;
    public CostOptions[] cost_options { get; set; } = default!;
    public RegularSchedule[] regular_schedules { get; set; } = default!;
    public Language[] languages { get; set; } = default!;
    public ServiceArea[] service_areas { get; set; } = default!;
    public object[] school_pickups { get; set; } = default!;
    public object[] links { get; set; } = default!;
    public object[] special_needs { get; set; } = default!;
    public Contact[] contacts { get; set; } = default!;
    public Eligibility[] eligibilitys { get; set; } = default!;
    public ServiceTaxonomys[] service_taxonomys { get; set; } = default!;
    public string status { get; set; } = default!;
    public object distance_away { get; set; } = default!;
    public object distance_away_is_service_area { get; set; } = default!;
    public object alert { get; set; } = default!;
    public object local_offer { get; set; } = default!;
    public string cqc_current_rating { get; set; } = default!;
    public object cqc_last_inspection { get; set; } = default!;
    public string extra_section_heading { get; set; } = default!;
    public string extra_section_content { get; set; } = default!;
    public bool hide_address { get; set; }
}



