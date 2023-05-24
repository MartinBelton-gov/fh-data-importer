using RestSharp;

namespace SportEngland.Services;



internal interface ISportEnglandClientService
{
    Task<SportEnglandModel> GetServices(long changenumber = 9500000, int limit = 100);
}

internal class SportEnglandClientService : ISportEnglandClientService
{
    private readonly RestClient _client;

    public SportEnglandClientService(string baseUri)
    {
        _client = new RestClient(baseUri);
    }

    public async Task<SportEnglandModel> GetServices(long changenumber=9500000, int limit=100)
    {
        var request = new RestRequest($"sites?afterChangeNumber={changenumber}&limit={limit}");

        return await _client.GetAsync<SportEnglandModel>(request, CancellationToken.None) ?? new SportEnglandModel();
    }
}

