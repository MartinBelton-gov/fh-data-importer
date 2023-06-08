using Polly;
using RestSharp;
using System.Net;
using System.Text.Json;

namespace OpenActiveImporter.Services;

internal interface IOpenActiveClientService<T> where T : new()
{
    Task<T> GetServices(string url);
}

internal class OpenActiveClientService<T> : IOpenActiveClientService<T> where T : new()
{
    private readonly RestClient _client;
    private readonly int _maxRetries = 3;
    private readonly int _retryDelayMilliseconds = 2000;

    public OpenActiveClientService(string baseUri)
    {
        _client = new RestClient(baseUri);
    }

    public async Task<T> GetServices(string url)
    {
        var request = new RestRequest($"?{url}");

        var policy = Policy
            .HandleResult<RestResponse>(r => r.StatusCode != HttpStatusCode.OK)
            .WaitAndRetryAsync(_maxRetries, attempt =>
            {
                Console.WriteLine($"Retrying ({attempt}/{_maxRetries}) in {_retryDelayMilliseconds}ms...");
                return TimeSpan.FromMilliseconds(_retryDelayMilliseconds);
            });

        var result = await policy.ExecuteAsync(async () =>
        {
            var response = await _client.ExecuteAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response;
            }
            return new RestResponse();
        });

        return JsonSerializer.Deserialize<T>(result.Content ?? string.Empty) ?? new T();
    }
}


