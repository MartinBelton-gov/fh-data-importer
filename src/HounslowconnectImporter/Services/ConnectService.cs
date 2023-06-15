using FamilyHubs.ServiceDirectory.Shared.Enums;
using System.Text.Json.Serialization;

namespace HounslowconnectImporter.Services;

//https://api.hounslowconnect.com/core/v1/services?page=1&per_page=25

public class OtherDetails
{
    public Organisation Organisation { get; set; } = default!;
    public ServiceLocations ServiceLocations { get; set; } = default!;
    public List<Location> Locations { get; set; } = new List<Location>();
}

public class ConnectService
{
    public Datum[] data { get; set; } = default!;
    public Links links { get; set; } = default!;
    public Meta meta { get; set; } = default!;
}

public class Links
{
    public string first { get; set; } = default!;
    public string last { get; set; } = default!;
    public object prev { get; set; } = default!;
    public string next { get; set; } = default!;
}

public class Meta
{
    public int? current_page { get; set; }
    public int? from { get; set; }
    public int? last_page { get; set; }
    public string path { get; set; } = default!;
    public int? per_page { get; set; }
    public int? to { get; set; }
    public int? total { get; set; }
}

public class Datum
{
    public string id { get; set; } = default!;
    public string organisation_id { get; set; } = default!;
    public bool has_logo { get; set; }
    public string slug { get; set; } = default!;
    public string name { get; set; } = default!;
    public string type { get; set; } = default!;
    public string status { get; set; } = default!;
    public string intro { get; set; } = default!;
    public string description { get; set; } = default!;
    public object wait_time { get; set; } = default!;
    public bool is_free { get; set; }
    public string fees_text { get; set; } = default!;
    public string fees_url { get; set; } = default!;
    public string testimonial { get; set; } = default!;
    public string video_embed { get; set; } = default!;
    public string url { get; set; } = default!;
    public string contact_name { get; set; } = default!;
    public string contact_phone { get; set; } = default!;
    public string contact_email { get; set; } = default!;
    public bool show_referral_disclaimer { get; set; }
    public string referral_method { get; set; } = default!;
    public object referral_button_text { get; set; } = default!;
    public object referral_email { get; set; } = default!;
    public object referral_url { get; set; } = default!;
    public UsefulInfos[] useful_infos { get; set; } = default!;
    public Offering[] offerings { get; set; } = default!;
    public GalleryItems[] gallery_items { get; set; } = default!;
    public CategoryTaxonomies[] category_taxonomies { get; set; } = default!;
    public EligibilityTypes eligibility_types { get; set; } = default!;
    public DateTime last_modified_at { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
}

public class EligibilityTypes
{
    public Custom custom { get; set; } = default!;
    public string[] taxonomies { get; set; } = default!;
}

public class Custom
{
    public string age_group { get; set; } = default!;
    public string disability { get; set; } = default!;
    public string ethnicity { get; set; } = default!;
    public object gender { get; set; } = default!;
    public object income { get; set; } = default!;
    public string language { get; set; } = default!;
    public object housing { get; set; } = default!;
    public object other { get; set; } = default!;
}

public class UsefulInfos
{
    public string title { get; set; } = default!;
    public string description { get; set; } = default!;
    public int? order { get; set; }
}

public class Offering
{
    public string offering { get; set; } = default!;
    public int? order { get; set; }
}

public class GalleryItems
{
    public string file_id { get; set; } = default!;
    public string url { get; set; } = default!;
    
    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime created_at { get; set; }
    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime updated_at { get; set; }
}

public class CategoryTaxonomies
{
    public string id { get; set; } = default!;
    public string parent_id { get; set; } = default!;
    public string name { get; set; } = default!;

    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime created_at { get; set; }
    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime updated_at { get; set; }
}