namespace PlacecubeImporter.Services
{
    public class PlacecubeSimpleService
    {
        public int totalElements { get; set; } = default!;
        public int totalPages { get; set; } = default!;
        public int number { get; set; } = default!;
        public int size { get; set; } = default!;
        public bool first { get; set; } = default!;
        public bool last { get; set; } = default!;
        public bool empty { get; set; } = default!;
        public Content[] content { get; set; } = default!;
    }

    public class Content
    {
        public string id { get; set; } = default!;
        public string name { get; set; } = default!;
        public string description { get; set; } = default!;
        public string url { get; set; } = default!;
        public string email { get; set; } = default!;
        public string status { get; set; } = default!;
        public string fees { get; set; } = default!;
        public string accreditations { get; set; } = default!;
        public string deliverable_type { get; set; } = default!;
        public string attending_type { get; set; } = default!;
        public string attending_access { get; set; } = default!;
        public string pc_attendingAccess_additionalInfo { get; set; } = default!;
        public string assured_date { get; set; } = default!;
        public Organization organization { get; set; } = default!;
        public PcMetadata pc_metadata { get; set; } = default!;
        public PcTargetaudience[] pc_targetAudience { get; set; } = default!;
    }

    public class Organization
    {
        public string id { get; set; } = default!;
        public string name { get; set; } = default!;
        public string description { get; set; } = default!;
        public string url { get; set; } = default!;
        public string logo { get; set; } = default!;
        public string uri { get; set; } = default!;
    }

    public class PcMetadata
    {
        public string date_created { get; set; } = default!;
        public string date_modified { get; set; } = default!;
        public string date_assured { get; set; } = default!;
        public string assured_by { get; set; } = default!;
    }

    public class PcTargetaudience
    {
        public string id { get; set; } = default!;
        public string audienceType { get; set; } = default!;
    }

}
