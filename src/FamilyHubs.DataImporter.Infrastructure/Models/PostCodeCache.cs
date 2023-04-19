using System.ComponentModel.DataAnnotations;

namespace FamilyHubs.DataImporter.Infrastructure.Models
{
    public class PostCodeCache
    {
        [Key] 
        public string PostCode { get; set; } = default!;
        public double Latitude { get; set; } = default!;
        public double Longitude { get; set; } = default!;
        public string AdminDistrict { get; set; } = default!;
        public string AdminCounty { get; set; } = default!;
    }
}
