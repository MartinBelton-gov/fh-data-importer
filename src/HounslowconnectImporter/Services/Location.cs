namespace HounslowconnectImporter.Services;


public class Location
{
    public LocationData data { get; set; } = default!;
}

public class LocationData
{
    public string id { get; set; } = default!;
    public bool has_image { get; set; }
    public string address_line_1 { get; set; } = default!;
    public string address_line_2 { get; set; } = default!;
    public string address_line_3 { get; set; } = default!;
    public string city { get; set; } = default!;
    public string county { get; set; } = default!;
    public string postcode { get; set; } = default!;
    public string country { get; set; } = default!;
    public float lat { get; set; }
    public float lon { get; set; }
    public object accessibility_info { get; set; } = default!;
    public bool has_wheelchair_access { get; set; }
    public bool has_induction_loop { get; set; }
    public bool has_accessible_toilet { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
}

