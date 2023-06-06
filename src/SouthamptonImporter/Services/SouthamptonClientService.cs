using Polly;
using RestSharp;
using System.Net;
using System.Text.Json;

namespace SouthamptonImporter.Services;

internal interface ISouthamptonClientService
{
    Task<SouthamptonSimpleService> GetServicesByPage(int pageNumber);
    Task<SouthamptonService> GetServiceById(string id);
}

internal class SouthamptonClientService : ISouthamptonClientService
{
    private readonly RestClient _client;
    private readonly int _maxRetries = 3;
    private readonly int _retryDelayMilliseconds = 2000;

    public SouthamptonClientService(string baseUri)
    {
        _client = new RestClient(baseUri);
    }

    public async Task<SouthamptonSimpleService> GetServicesByPage(int pageNumber)
    {
        var request = new RestRequest($"services/?&page={pageNumber}");

        var policy = Policy
            .HandleResult<RestResponse<SouthamptonSimpleService>>(r => r.StatusCode != HttpStatusCode.OK)
            .WaitAndRetryAsync(_maxRetries, attempt =>
            {
                Console.WriteLine($"Retrying ({attempt}/{_maxRetries}) in {_retryDelayMilliseconds}ms...");
                return TimeSpan.FromMilliseconds(_retryDelayMilliseconds);
            });


        var result = await policy.ExecuteAsync(async () =>
        {
            var response = await _client.ExecuteAsync<SouthamptonSimpleService>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response;
            }
            return new RestResponse<SouthamptonSimpleService>();
        });

        return JsonSerializer.Deserialize<SouthamptonSimpleService>(result.Content ?? string.Empty) ?? new SouthamptonSimpleService();
    }

    public async Task<SouthamptonService> GetServiceById(string id)
    {
        var request = new RestRequest($"services/{id}");

        var policy = Policy
            .HandleResult<RestResponse<SouthamptonService>>(r => r.StatusCode != HttpStatusCode.OK)
            .WaitAndRetryAsync(_maxRetries, attempt =>
            {
                Console.WriteLine($"Retrying ({attempt}/{_maxRetries}) in {_retryDelayMilliseconds}ms...");
                return TimeSpan.FromMilliseconds(_retryDelayMilliseconds);
            });


        var result = await policy.ExecuteAsync(async () =>
        {
            var response = await _client.ExecuteAsync<SouthamptonService>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response;
            }
            return new RestResponse<SouthamptonService>();
        });

        return JsonSerializer.Deserialize<SouthamptonService>(result.Content ?? string.Empty) ?? new SouthamptonService();
    }
}

