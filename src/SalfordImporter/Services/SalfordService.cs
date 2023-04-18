namespace SalfordImporter.Services;
public class SalfordService
{
    public SalfordRecord[] records { get; set; } = default!;
    public int totalRecords { get; set; }
    public string nextPage { get; set; } = default!;
}

public class SalfordRecord
{
    public string venue_name { get; set; } = default!;
    public object ecd_timetable_openinghours_list { get; set; } = default!;
    public object contact_telephone { get; set; } = default!;
    public string description { get; set; } = default!;
    public string title { get; set; } = default!;
    public string contact_email { get; set; } = default!;
    public string contact_notes { get; set; } = default!;
    public string venue_postcode { get; set; } = default!;
    public object date_session_info { get; set; } = default!;
    public DateActivityPeriod date_activity_period { get; set; } = default!;
    public dynamic agerange { get; set; } = default!;
    public string contact_name { get; set; } = default!;
    public object website { get; set; } = default!;
    public string recordUri { get; set; } = default!;
    public string public_address_2 { get; set; } = default!;
    public string cost_description { get; set; } = default!;
    public string public_address_1 { get; set; } = default!;
    public object cost_table { get; set; } = default!;
    public string public_address_4 { get; set; } = default!;
    public string public_address_3 { get; set; } = default!;
    public string externalId { get; set; } = default!;
    public object date_timeofday { get; set; } = default!;
    public string date_displaydate { get; set; } = default!;
    public string lastUpdate { get; set; } = default!;
    public string notes_public { get; set; } = default!;
    public object images { get; set; } = default!;
    public Logo logo { get; set; } = default!;
    public string public_address_5 { get; set; } = default!;
    //public Files files { get; set; } = default!;
    public object files { get; set; } = default!;
}

public class DateActivityPeriod
{
    public string alwayson { get; set; } = default!;
    public string[] weekdays { get; set; } = default!;
    public string[] dates { get; set; } = default!;
}

public class Logo
{
    public string filename { get; set; } = default!;
    public string description { get; set; } = default!;
}

public class Files
{
    public string filename { get; set; } = default!;
    public string description { get; set; } = default!;
}

public class CostTable
{
    public string cost_amount { get; set; } = default!;
    public CostType cost_type { get; set; } = default!;
}

public class CostType
{
    public string displayName { get; set; } = default!;
    public string id { get; set; } = default!;
}

public class Website
{
    public NewWindow new_window { get; set; } = default!;
    public string url { get; set; } = default!;
    public string label { get; set; } = default!;
}

public class NewWindow
{
    public string displayName { get; set; } = default!;
    public string id { get; set; } = default!;
}



