using Newtonsoft.Json;

namespace PluginBase;

public class PostcodesIoResponse
{
    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("error")]
    public int Error { get; set; }

    [JsonProperty("result")]
    public PostcodeInfo Result { get; set; } = default!;
}

public class PostcodeInfo
{
    [JsonProperty("postcode")]
    public string Postcode { get; set; } = default!;

    public string AdminArea => string.Equals(Codes.admin_county, "E99999999", StringComparison.InvariantCultureIgnoreCase) ? Codes.admin_district : Codes.admin_county;

    [JsonProperty("latitude")]
    public double Latitude { get; set; }

    [JsonProperty("longitude")]
    public double Longitude { get; set; }

    [JsonProperty("outcode")]
    public string? OutCode { get; set; }

    [JsonProperty("country")]
    public string? Country { get; set; }

    [JsonProperty("codes")]
    public Codes Codes { get; set; } = default!;
}

public class Codes
{
    public string admin_district { get; set; } = default!;
    public string admin_county { get; set; } = default!;
    public string admin_ward { get; set; } = default!;

}


public class ListPostcodesIoResponse
{
    public int status { get; set; }
    public Result[] result { get; set; } = default!;
}

public class Result
{
    public string postcode { get; set; } = default!;
    public int quality { get; set; }
    public int eastings { get; set; }
    public int northings { get; set; }
    public string country { get; set; } = default!;
    public string nhs_ha { get; set; } = default!;
    public float longitude { get; set; }
    public float latitude { get; set; }
    public string european_electoral_region { get; set; } = default!;
    public string primary_care_trust { get; set; } = default!;
    public string region { get; set; } = default!;
    public string lsoa { get; set; } = default!;
    public string msoa { get; set; } = default!;
    public string incode { get; set; } = default!;
    public string outcode { get; set; } = default!;
    public string parliamentary_constituency { get; set; } = default!;
    public string admin_district { get; set; } = default!;
    public string parish { get; set; } = default!;
    public object admin_county { get; set; } = default!;
    public string date_of_introduction { get; set; } = default!;
    public string admin_ward { get; set; } = default!;
    public object ced { get; set; } = default!;
    public string ccg { get; set; } = default!;
    public string nuts { get; set; } = default!;
    public string pfa { get; set; } = default!;
    public Codes codes { get; set; } = default!;
    public float distance { get; set; } = default!;
}

