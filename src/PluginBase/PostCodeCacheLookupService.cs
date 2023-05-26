using FamilyHubs.DataImporter.Infrastructure.Models;
using FamilyHubs.DataImporter.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginBase;

public interface IPostCodeCacheLookupService
{
    Task<(double latitude, double logtitude)> GetCoordinates(string postCode);
    Task<string> GetAdminCode(string postCode, string parentAdminCode);
}

public class PostCodeCacheLookupService : IPostCodeCacheLookupService
{
    private readonly IPostcodeLocationClientService _postcodeLocationClientService;
    private readonly ApplicationDbContext _applicationDbContext;
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

    public async Task<string> GetAdminCode(string postCode, string parentAdminCode)
    {
        if (string.IsNullOrEmpty(postCode))
        {
            Console.WriteLine($"Empty postcode return parent admin code");
            return parentAdminCode;
        }

        try
        {
            var postcodeCache = _applicationDbContext.PostCodeCache.FirstOrDefault(x => x.PostCode == postCode);
            if (postcodeCache != null)
            {
                return postcodeCache.AdminCounty == "E99999999" ? postcodeCache.AdminDistrict : postcodeCache.AdminCounty;
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

            return postcodesIoResponse.Result.Codes.admin_county == "E99999999" ? postcodesIoResponse.Result.Codes.admin_district : postcodesIoResponse.Result.Codes.admin_county;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            Console.WriteLine($"Failed to find Postcode: {postCode} from postcodes.io return parent admin code");
            return parentAdminCode;
        }
    }
}
