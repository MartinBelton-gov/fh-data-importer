using RestSharp;

namespace SouthamptonImporter.Services;

internal interface ISouthamptonClientService
{
    Task<SouthamptonSimpleService> GetServicesByPage(int pageNumber);
    Task<SouthamptonService> GetServiceById(string id);
}

internal class SouthamptonClientService : ISouthamptonClientService
{
    private readonly RestClient _client;

    public SouthamptonClientService(string baseUri)
    {
        _client = new RestClient(baseUri);
    }

    public async Task<SouthamptonSimpleService> GetServicesByPage(int pageNumber)
    {
        var request = new RestRequest($"services/?&page={pageNumber}");

        return await _client.GetAsync<SouthamptonSimpleService>(request, CancellationToken.None) ?? new SouthamptonSimpleService();
    }

    public async Task<SouthamptonService> GetServiceById(string id)
    {
        var request = new RestRequest($"services/{id}");

        return await _client.GetAsync<SouthamptonService>(request, CancellationToken.None) ?? new SouthamptonService();
    }
}

