using RestSharp;

namespace SalfordImporter.Services;

internal interface ISalfordClientService
{
    Task<SalfordService> GetServices();
}

internal class SalfordClientService : ISalfordClientService
{
    private readonly RestClient _client;

    public SalfordClientService(string baseUri)
    {
        _client = new RestClient(baseUri);
    }

    public async Task<SalfordService> GetServices()
    {
        var request = new RestRequest($"?key=633eb0a9e4b0b3bc6d117a9a");

        return await _client.GetAsync<SalfordService>(request, CancellationToken.None) ?? new SalfordService();
    }
}
