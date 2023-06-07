using Polly;
using RestSharp;
using System.Net;
using System.Text.Json;

namespace OpenActiveImporter.Services;

internal interface IOpenActiveClientService
{
    Task<OpenActiveService> GetServices(string url);
}

internal class OpenActiveClientService : IOpenActiveClientService
{
    private readonly RestClient _client;
    private readonly int _maxRetries = 3;
    private readonly int _retryDelayMilliseconds = 2000;

    public OpenActiveClientService(string baseUri)
    {
        _client = new RestClient(baseUri);
    }

    public async Task<OpenActiveService> GetServices(string url)
    {

        var request = new RestRequest($"?{url}");

        var policy = Policy
            .HandleResult<RestResponse<OpenActiveService>>(r => r.StatusCode != HttpStatusCode.OK)
            .WaitAndRetryAsync(_maxRetries, attempt =>
            {
                Console.WriteLine($"Retrying ({attempt}/{_maxRetries}) in {_retryDelayMilliseconds}ms...");
                return TimeSpan.FromMilliseconds(_retryDelayMilliseconds);
            });


        var result = await policy.ExecuteAsync(async () =>
        {
            var response = await _client.ExecuteAsync<OpenActiveService>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response;
            }
            return new RestResponse<OpenActiveService>();
        });

        return JsonSerializer.Deserialize<OpenActiveService>(result.Content ?? string.Empty) ?? new OpenActiveService();
    }
}


