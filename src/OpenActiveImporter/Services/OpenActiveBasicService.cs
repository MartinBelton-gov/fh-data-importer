namespace OpenActiveImporter.Services;


public class OpenActiveBasicService
{
    public string next { get; set; } = default!;
    public BasicItem[] items { get; set; } = default!;
    public string license { get; set; } = default!;
}

public class BasicItem
{
    public string id { get; set; } = default!;
    public int modified { get; set; } = default!;
    public string kind { get; set; } = default!;
    public string state { get; set; } = default!;
    public BasicData data { get; set; } = default!;
}

public class BasicData
{
    public string[] context { get; set; } = default!;
    public string type { get; set; } = default!;
    public string id { get; set; } = default!;
    public string identifier { get; set; } = default!;
    public string name { get; set; } = default!;
    public string[] category { get; set; } = default!;
    public string duration { get; set; } = default!;
    public Eventschedule[] eventSchedule { get; set; } = default!;
    public Location location { get; set; } = default!;
    public Organizer organizer { get; set; } = default!;
    public Offer[] offers { get; set; } = default!;
}

public class Location
{
    public string type { get; set; } = default!;
    public string identifier { get; set; } = default!;
    public string name { get; set; } = default!;
    public string description { get; set; } = default!;
    public Address address { get; set; } = default!;
    public Amenityfeature[] amenityFeature { get; set; } = default!;
    public Geo geo { get; set; } = default!;
    public string telephone { get; set; } = default!;
    public string url { get; set; } = default!;
    public string betaformattedDescription { get; set; } = default!;
}

public class Address
{
    public string type { get; set; } = default!;
    public string addressCountry { get; set; } = default!;
    public string addressLocality { get; set; } = default!;
    public string addressRegion { get; set; } = default!;
    public string postalCode { get; set; } = default!;
    public string streetAddress { get; set; } = default!;
}

public class Geo
{
    public string type { get; set; } = default!;
    public float latitude { get; set; }
    public float longitude { get; set; }
}

public class Amenityfeature
{
    public string type { get; set; } = default!;
    public string name { get; set; } = default!;
    public bool value { get; set; } = default!;
}

public class Organizer
{
    public string type { get; set; } = default!;
    public string name { get; set; } = default!;
    public string description { get; set; } = default!;
    public string email { get; set; } = default!;
    public string legalName { get; set; } = default!;
    public Logo logo { get; set; } = default!;
    public string[] sameAs { get; set; } = default!;
    public string telephone { get; set; } = default!;
    public string url { get; set; } = default!;
    public string betaformattedDescription { get; set; } = default!;
}

public class Logo
{
    public string type { get; set; } = default!;
    public string url { get; set; } = default!;
}

public class Eventschedule
{
    public string type { get; set; } = default!;
    public string[] byDay { get; set; } = default!;
    public string duration { get; set; } = default!;
    public string endTime { get; set; } = default!;
    public string startDate { get; set; } = default!;
    public string startTime { get; set; } = default!;
    public string betatimeZone { get; set; } = default!;
    public string endDate { get; set; } = default!;
}

public class Offer
{
    public string type { get; set; } = default!;
    public string identifier { get; set; } = default!;
    public string name { get; set; } = default!;
    public string description { get; set; } = default!;
    public string[] acceptedPaymentMethod { get; set; } = default!;
    public float price { get; set; }
    public string priceCurrency { get; set; } = default!;
    public Agerange ageRange { get; set; } = default!;
}



