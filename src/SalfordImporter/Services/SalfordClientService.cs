using RestSharp;

namespace SalfordImporter.Services;

internal interface ISalfordClientService
{
    Task<SalfordService> GetServices(int? startIndex, int? count);
}

internal class SalfordClientService : ISalfordClientService
{
    private readonly RestClient _client;

    public SalfordClientService(string baseUri)
    {
        _client = new RestClient(baseUri);
    }

    public async Task<SalfordService> GetServices(int? startIndex, int? count)
    {
        string url = "?key=633eb0a9e4b0b3bc6d117a9a";
        if (startIndex != null && count != null) 
        {
            url += $"&startIndex={startIndex}&count={count}";
        }

        var request = new RestRequest(url);

        return await _client.GetAsync<SalfordService>(request, CancellationToken.None) ?? new SalfordService();
    }
}
