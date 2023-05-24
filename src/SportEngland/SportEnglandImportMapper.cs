using FamilyHubs.ServiceDirectory.Shared.Dto;
using PluginBase;
using SportEngland.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SportEngland;



internal class SportEnglandImportMapper : BaseMapper
{
    public string Name => "Buckinghamshire Mapper";

    private readonly ISportEnglandClientService _sportEnglandClientService;
    public SportEnglandImportMapper(ISportEnglandClientService sportEnglandClientService, IOrganisationClientService organisationClientService, string adminAreaCode, string key, OrganisationWithServicesDto parentLA)
        : base(organisationClientService, adminAreaCode, parentLA, key)
    {
        _sportEnglandClientService = sportEnglandClientService;
    }

    public async Task AddOrUpdateServices()
    {
        const int maxRetry = 3;
        const int startPage = 1;
        int errors = 0;
        await CreateOrganisationDictionary();
        await CreateTaxonomyDictionary();
        SportEnglandModel sportEnglandModel = await _sportEnglandClientService.GetServices(9500000,100);

        while(!string.IsNullOrEmpty(sportEnglandModel.next))
        {
            foreach (var item in sportEnglandModel.items)
            {
                if (item.state == "deleted")
                    continue;

                //errors += await AddAndUpdateService(content);
            }
            Console.WriteLine($"Completed Page {startPage} with {errors} errors");

            long changeNumber = GetChangeNumber(sportEnglandModel.next);
            if (changeNumber <= 0)
                break;

            sportEnglandModel = await _sportEnglandClientService.GetServices(changeNumber, 100);
        }
        
        

        //for (int i = startPage + 1; i <= totalPages; i++)
        //{
        //    errors = 0;

        //    int retry = 0;
        //    while (retry < maxRetry)
        //    {
        //        try
        //        {
        //            buckinghapshireService = await _buckinghamshireClientService.GetServicesByPage(i);
        //            break;
        //        }
        //        catch
        //        {
        //            System.Threading.Thread.Sleep(1000);
        //            retry++;
        //            if (retry > maxRetry)
        //            {
        //                Console.WriteLine($"Failed to get page");
        //                return;

        //            }
        //            Console.WriteLine($"Doing retry: {retry}");
        //        }
        //    }

        //    //foreach (var content in buckinghapshireService.content)
        //    //{
        //    //    errors += await AddAndUpdateService(content);
        //    //}
        //    Console.WriteLine($"Completed Page {i} of {totalPages} with {errors} errors");

        //}

    }

    private long GetChangeNumber(string url)
    {
        Uri myUri = new Uri(url);
        string param1 = HttpUtility.ParseQueryString(myUri.Query).Get("afterChangeNumber") ?? string.Empty;
        long.TryParse(param1, out long value);
        return value;
    }
}
