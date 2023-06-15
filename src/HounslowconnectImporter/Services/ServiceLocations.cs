namespace HounslowconnectImporter.Services;


public class ServiceLocations
{
    public ServiceLocationsDatum[] data { get; set; } = default!;
    public Links links { get; set; } = default!;
    public Meta meta { get; set; } = default!;
}

public class ServiceLocationsDatum
{
    public string id { get; set; } = default!;
    public string service_id { get; set; } = default!;
    public string location_id { get; set; } = default!;
    public bool has_image { get; set; }
    public string name { get; set; } = default!;
    public bool is_open_now { get; set; }
    public RegularOpeningHours[] regular_opening_hours { get; set; } = default!;
    public HolidayOpeningHours[] holiday_opening_hours { get; set; } = default!;
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
}

public class RegularOpeningHours
{
    public string frequency { get; set; } = default!;
    public int weekday { get; set; }
    public string opens_at { get; set; } = default!;
    public string closes_at { get; set; } = default!;
}

public class HolidayOpeningHours
{
    public bool is_closed { get; set; }
    public string starts_at { get; set; } = default!;
    public string ends_at { get; set; } = default!;
    public string opens_at { get; set; } = default!;
    public string closes_at { get; set; } = default!;
}

