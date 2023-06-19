namespace HounslowconnectImporter.Services;

//https://api.hounslowconnect.com/core/v1/organisations/c3b48f0f-8fae-4da8-bf76-2c374d9f7a03
public class Organisation
{
    public OrganisationData data { get; set; } = default!;
}

public class OrganisationData
{
    public string id { get; set; } = default!;
    public bool has_logo { get; set; }
    public string slug { get; set; } = default!;
    public string name { get; set; } = default!;
    public string description { get; set; } = default!;
    public string url { get; set; } = default!;
    public string email { get; set; } = default!;
    public string phone { get; set; } = default!;
    public SocialMedias[] social_medias { get; set; } = default!;
    public CategoryTaxonomies[] category_taxonomies { get; set; } = default!;
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
}

public class SocialMedias
{
    public string type { get; set; } = default!;
    public string url { get; set; } = default!;
}

