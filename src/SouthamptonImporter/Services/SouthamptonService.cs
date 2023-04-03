namespace SouthamptonImporter.Services;


internal class SouthamptonService
{
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string url { get; set; }
    public string referral_notes { get; set; }
    public object needs_referral { get; set; }
    public string ofsted_urn { get; set; }
    public string ofsted_url { get; set; }
    public object ofsted_latest_inspection { get; set; }
    public string cqc_url { get; set; }
    public string schedule_description { get; set; }
    public object eligible_for2_yo_funding { get; set; }
    public bool eligible_for34_yo_funding { get; set; }
    public object eligible_for_haf_funding { get; set; }
    public object eligible_for30_h_entitlement { get; set; }
    public DateTime assured_date { get; set; }
    public ServiceAtLocation[] service_at_locations { get; set; }
    public Organization organization { get; set; }
    public CostOptions[] cost_options { get; set; }
    public RegularSchedule[] regular_schedules { get; set; }
    public Language[] languages { get; set; }
    public ServiceArea[] service_areas { get; set; }
    public object[] school_pickups { get; set; }
    public object[] links { get; set; }
    public object[] special_needs { get; set; }
    public Contact[] contacts { get; set; }
    public Eligibility[] eligibilitys { get; set; }
    public ServiceTaxonomys[] service_taxonomys { get; set; }
    public string status { get; set; }
    public object distance_away { get; set; }
    public object distance_away_is_service_area { get; set; }
    public object alert { get; set; }
    public object local_offer { get; set; }
    public string cqc_current_rating { get; set; }
    public object cqc_last_inspection { get; set; }
    public string extra_section_heading { get; set; }
    public string extra_section_content { get; set; }
    public bool hide_address { get; set; }
}



