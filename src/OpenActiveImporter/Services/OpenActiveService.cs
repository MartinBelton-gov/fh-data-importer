namespace OpenActiveImporter.Services;


public class OpenActiveService
{
    public string next { get; set; } = default!;
    public string license { get; set; } = default!;
    public Item[] items { get; set; } = default!;
}

public class Item
{
    public string kind { get; set; } = default!;
    public string state { get; set; } = default!;
    public string id { get; set; } = default!;
    public int modified { get; set; }
    public Data data { get; set; } = default!;
}

public class Data
{
    public string[] context { get; set; } = default!;
    public Superevent superEvent { get; set; } = default!;
    public string type { get; set; } = default!;
    public string name { get; set; } = default!;
    public Organizer organizer { get; set; } = default!;
    public DateTime startDate { get; set; }
    public string identifier { get; set; } = default!;
    public DateTime endDate { get; set; }
    public string eventStatus { get; set; } = default!;
    public int maximumAttendeeCapacity { get; set; }
    public int remainingAttendeeCapacity { get; set; }
    public string duration { get; set; } = default!;
    public Offer[] offers { get; set; } = default!;
    public Leader[] leader { get; set; } = default!;
}

public class Superevent
{
    public Superevent1 superEvent { get; set; } = default!;
    public string type { get; set; } = default!;
    public Location location { get; set; } = default!;
    public string url { get; set; } = default!;
    public Eventschedule[] eventSchedule { get; set; } = default!;
    public string identifier { get; set; } = default!;
    public string description { get; set; } = default!;
}

public class Superevent1
{
    public string type { get; set; } = default!;
    public Activity[] activity { get; set; } = default!;
    public string name { get; set; } = default!;
    public string identifier { get; set; } = default!;
    public string genderRestriction { get; set; } = default!;
    public Agerange ageRange { get; set; } = default!;
}

public class Agerange
{
    public int minValue { get; set; }
    public int maxValue { get; set; }
    public string type { get; set; } = default!;
}

public class Activity
{
    public string prefLabel { get; set; } = default!;
    public string type { get; set; } = default!;
    public string id { get; set; } = default!;
    public string inScheme { get; set; } = default!;
}

public class Location
{
    public string identifier { get; set; } = default!;
    public string url { get; set; } = default!;
    public string name { get; set; } = default!;
    public Address address { get; set; } = default!;
    public string telephone { get; set; } = default!;
    public Geo geo { get; set; } = default!;
    public string type { get; set; } = default!;
}

public class Address
{
    public string streetAddress { get; set; } = default!;
    public string addressLocality { get; set; } = default!;
    public string addressRegion { get; set; } = default!;
    public string postalCode { get; set; } = default!;
    public string addressCountry { get; set; } = default!;
    public string type { get; set; } = default!;
}

public class Geo
{
    public float latitude { get; set; }
    public float longitude { get; set; }
    public string type { get; set; } = default!;
}

public class Eventschedule
{
    public string startDate { get; set; } = default!;
    public string endDate { get; set; } = default!;
    public string startTime { get; set; } = default!;
    public string repeatFrequency { get; set; } = default!;
    public string[] byDay { get; set; } = default!;
    public string type { get; set; } = default!;
}

public class Organizer
{
    public string name { get; set; } = default!;
    public string email { get; set; } = default!;
    public string type { get; set; } = default!;
}

public class Offer
{
    public string identifier { get; set; } = default!;
    public string name { get; set; } = default!;
    public float price { get; set; }
    public string priceCurrency { get; set; } = default!;
    public string type { get; set; } = default!;
}

public class Leader
{
    public string name { get; set; } = default!;
    public string type { get; set; } = default!;
}

