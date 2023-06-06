using Polly;
using RestSharp;
using System.Net;
using System.Text.Json;

namespace SalfordImporter.Services;

internal interface ISalfordClientService
{
    Task<SalfordService> GetServices(int? startIndex, int? count);
}

internal class SalfordClientService : ISalfordClientService
{
    private readonly RestClient _client;
    private readonly int _maxRetries = 3;
    private readonly int _retryDelayMilliseconds = 2000;


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

        var policy = Policy
            .HandleResult<RestResponse<SalfordService>>(r => r.StatusCode != HttpStatusCode.OK)
            .WaitAndRetryAsync(_maxRetries, attempt =>
            {
                Console.WriteLine($"Retrying ({attempt}/{_maxRetries}) in {_retryDelayMilliseconds}ms...");
                return TimeSpan.FromMilliseconds(_retryDelayMilliseconds);
            });


        var result = await policy.ExecuteAsync(async () =>
        {
            var response = await _client.ExecuteAsync<SalfordService>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response;
            }
            return new RestResponse<SalfordService>();
        });

        return JsonSerializer.Deserialize<SalfordService>(result.Content ?? string.Empty) ?? new SalfordService();
    }
}
