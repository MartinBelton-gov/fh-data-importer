using RestSharp;

namespace BuckingshireImporter.Services;

internal interface IBuckinghamshireClientService
{
    Task<BuckinghapshireService> GetServicesByPage(int pageNumber);
}

internal class BuckinghamshireClientService : IBuckinghamshireClientService
{
    private readonly RestClient _client;

    public BuckinghamshireClientService(string baseUri)
    {
        _client = new RestClient(baseUri);
    }

    public async Task<BuckinghapshireService> GetServicesByPage(int pageNumber)
    {
        var request = new RestRequest($"services/?&page={pageNumber}");

        return await _client.GetAsync<BuckinghapshireService>(request, CancellationToken.None) ?? new BuckinghapshireService();
    }
}
