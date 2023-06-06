using Polly;
using RestSharp;
using System.Net;
using System.Text.Json;

namespace BuckingshireImporter.Services;

internal interface IBuckinghamshireClientService
{
    Task<BuckinghapshireService> GetServicesByPage(int pageNumber);
}

internal class BuckinghamshireClientService : IBuckinghamshireClientService
{
    private readonly RestClient _client;
    private readonly int _maxRetries = 3;
    private readonly int _retryDelayMilliseconds = 2000;

    public BuckinghamshireClientService(string baseUri)
    {
        _client = new RestClient(baseUri);
    }

    public async Task<BuckinghapshireService> GetServicesByPage(int pageNumber)
    {
        var request = new RestRequest($"services/?&page={pageNumber}");

        var policy = Policy
            .HandleResult<RestResponse<BuckinghapshireService>>(r => r.StatusCode != HttpStatusCode.OK)
            .WaitAndRetryAsync(_maxRetries, attempt =>
            {
                Console.WriteLine($"Retrying ({attempt}/{_maxRetries}) in {_retryDelayMilliseconds}ms...");
                return TimeSpan.FromMilliseconds(_retryDelayMilliseconds);
            });


        var result = await policy.ExecuteAsync(async () =>
        {
            var response = await _client.ExecuteAsync<BuckinghapshireService>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response;
            }
            return new RestResponse<BuckinghapshireService>();
        });

        return JsonSerializer.Deserialize<BuckinghapshireService>(result.Content ?? string.Empty) ?? new BuckinghapshireService();
    }
}
