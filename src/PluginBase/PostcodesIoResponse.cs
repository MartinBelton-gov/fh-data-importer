using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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