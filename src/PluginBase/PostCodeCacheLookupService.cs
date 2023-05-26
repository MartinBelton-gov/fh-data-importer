using Ardalis.Specification;
using FamilyHubs.DataImporter.Infrastructure;
using FamilyHubs.DataImporter.Infrastructure.Models;
using System.Text;

namespace PluginBase;

public interface IPostCodeCacheLookupService
{
    Task<(double latitude, double logtitude)> GetCoordinates(string postCode);
    Task<string> GetAdminCode(string postCode, string parentAdminCode, double longitude, double latitude);
}

public class PostCodeCacheLookupService : IPostCodeCacheLookupService
{
    private readonly IPostcodeLocationClientService _postcodeLocationClientService;
    private readonly ApplicationDbContext _applicationDbContext;

    public class AdminCountyResults
    {
        public string AdminCounty { get; set; } = default!;
        public int Count { get; set; }
    }
    public PostCodeCacheLookupService(IPostcodeLocationClientService postcodeLocationClientService, ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
        _postcodeLocationClientService = postcodeLocationClientService; 
    }

    public async Task<(double latitude, double logtitude)> GetCoordinates(string postCode)
    {
        if (string.IsNullOrEmpty(postCode))
        {
            Console.WriteLine($"Empty postcode return zero lat/long");
            return (0.0, 0.0);
        }

        try
        {
            var postcodeCache = _applicationDbContext.PostCodeCache.FirstOrDefault(x => x.PostCode == postCode);
            if (postcodeCache != null)
            {
                return (postcodeCache.Latitude, postcodeCache.Longitude);
            }

            PostcodesIoResponse postcodesIoResponse = await _postcodeLocationClientService.LookupPostcode(postCode);
            PostCodeCache postCodeCache = new PostCodeCache
            {
                PostCode = postCode,
                AdminCounty = postcodesIoResponse.Result.Codes.admin_county,
                AdminDistrict = postcodesIoResponse.Result.Codes.admin_district,
                Latitude = postcodesIoResponse.Result.Latitude,
                Longitude = postcodesIoResponse.Result.Longitude,
            };

            _applicationDbContext.PostCodeCache.Add(postCodeCache);
            await _applicationDbContext.SaveChangesAsync();

            return (postcodesIoResponse.Result.Latitude, postcodesIoResponse.Result.Longitude);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            Console.WriteLine($"Failed to find Postcode: {postCode} from postcodes.io return zero lat/long");
            return (0.0, 0.0);
        }
    }

    public async Task<string> GetAdminCode(string postCode, string parentAdminCode, double longitude, double latitude)
    {
        if (string.IsNullOrEmpty(postCode))
        {
            Console.WriteLine($"Empty postcode return parent admin code");
            return parentAdminCode;
        }

        var postcodeCache = _applicationDbContext.PostCodeCache.FirstOrDefault(x => x.PostCode == postCode);
        if (postcodeCache != null)
        {
            return postcodeCache.AdminCounty == "E99999999" ? postcodeCache.AdminDistrict : postcodeCache.AdminCounty;
        }

        PostcodesIoResponse? postcodesIoResponse = await GetPostCode(postCode);

        if (postcodesIoResponse != null) 
        {
            return postcodesIoResponse.Result.Codes.admin_county == "E99999999" ? postcodesIoResponse.Result.Codes.admin_district : postcodesIoResponse.Result.Codes.admin_county;
        }

        try
        {
            ListPostcodesIoResponse listPostcodesIoResponses = await _postcodeLocationClientService.GetNearestPostcodeFromCoordinates(longitude, latitude);
            if (listPostcodesIoResponses == null || !listPostcodesIoResponses.result.Any()) 
            {
                return GetAdminCountyFromDatabase(postCode, parentAdminCode);
            }

            string adminCode = string.Empty;
            PostCodeCache postCodeCache;
            List<Result> results = listPostcodesIoResponses.result.ToList();
            foreach (var item in results) 
            { 
                if (string.IsNullOrEmpty(adminCode))
                {
                    postCodeCache = new PostCodeCache
                    {
                        PostCode = postCode,
                        AdminCounty = item.codes.admin_county,
                        AdminDistrict = item.codes.admin_district,
                        Latitude = item.latitude,
                        Longitude = item.longitude,
                    };
                    _applicationDbContext.PostCodeCache.Add(postCodeCache);
                    adminCode = item.codes.admin_county == "E99999999" ? item.codes.admin_district : item.codes.admin_county;
                }

                postCodeCache = new PostCodeCache
                {
                    PostCode = item.postcode,
                    AdminCounty = item.codes.admin_county,
                    AdminDistrict = item.codes.admin_district,
                    Latitude = item.latitude,
                    Longitude = item.longitude,
                };
                _applicationDbContext.PostCodeCache.Add(postCodeCache);
            }

            await _applicationDbContext.SaveChangesAsync();
            return adminCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            return GetAdminCountyFromDatabase(postCode, parentAdminCode);
        }
    }

    private async Task<PostcodesIoResponse?> GetPostCode(string postCode)
    {
        try
        {
            PostcodesIoResponse postcodesIoResponse = await _postcodeLocationClientService.LookupPostcode(postCode);
            PostCodeCache postCodeCache = new PostCodeCache
            {
                PostCode = postCode,
                AdminCounty = postcodesIoResponse.Result.Codes.admin_county,
                AdminDistrict = postcodesIoResponse.Result.Codes.admin_district,
                Latitude = postcodesIoResponse.Result.Latitude,
                Longitude = postcodesIoResponse.Result.Longitude,
            };

            _applicationDbContext.PostCodeCache.Add(postCodeCache);
            await _applicationDbContext.SaveChangesAsync();
            return postcodesIoResponse;
        }
        catch
        {
            return null;
        }
    }

    private string GetAdminCountyFromDatabase(string postcode, string parentAdminCode)
    {
        string[] postcodeparts = postcode.Split(' ');
        var summary = from cache in _applicationDbContext.PostCodeCache
                      where cache.PostCode.Substring(0, postcodeparts[0].Length) == postcodeparts[0]
                      group cache by new
                      {
                          cache.AdminCounty,
                          cache.AdminDistrict
                      } into g
                      orderby g.Count() descending
                      select new
                      {
                          AdminCounty = g.First().AdminCounty,
                          AdminDistrict = g.First().AdminDistrict,
                          Count = g.Count()
                      };

        if (summary == null || !summary.Any())
        {
            string firstPartOfPostCode = GetFirstPartOfPostCode(postcode);
            summary = from cache in _applicationDbContext.PostCodeCache
                          where cache.PostCode.Substring(0, firstPartOfPostCode.Length) == firstPartOfPostCode
                          group cache by new
                          {
                              cache.AdminCounty,
                              cache.AdminDistrict
                          } into g
                          orderby g.Count() descending
                          select new
                          {
                              AdminCounty = g.First().AdminCounty,
                              AdminDistrict = g.First().AdminDistrict,
                              Count = g.Count()
                          };
        }

#pragma warning disable S1751
        if (summary != null && summary.Any())
        {
            foreach (var item in summary)
            {
                if (item.AdminCounty == "E99999999")
                {
                    return item.AdminDistrict;
                }

                return item.AdminCounty;
            }
        }
#pragma warning restore S1751

        Console.WriteLine($"Failed to find Postcode: {postcode} from postcodes.io return parent admin code");
        return parentAdminCode;
    }

    private string GetFirstPartOfPostCode(string postcode) 
    {
        StringBuilder sb = new StringBuilder();
        int i = 0;
        while (!Char.IsDigit(postcode[i]))
        {
            sb.Append(postcode[i]);
            i++;
        }
        return sb.ToString();
    }
}
