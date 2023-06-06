using Polly;
using RestSharp;
using System.Net;
using System.Text.Json;

namespace SportEngland.Services;



internal interface ISportEnglandClientService
{
    Task<SportEnglandModel> GetServices(long changenumber = 9500000, int limit = 100);
}

internal class SportEnglandClientService : ISportEnglandClientService
{
    private readonly RestClient _client;
    private readonly int _maxRetries = 3;
    private readonly int _retryDelayMilliseconds = 2000;

    public SportEnglandClientService(string baseUri)
    {
        _client = new RestClient(baseUri);
    }

    public async Task<SportEnglandModel> GetServices(long changenumber = 9500000, int limit = 100)
    {
        var request = new RestRequest($"sites?afterChangeNumber={changenumber}&limit={limit}");

        var policy = Policy
            .HandleResult<RestResponse<SportEnglandModel>>(r => r.StatusCode != HttpStatusCode.OK)
            .WaitAndRetryAsync(_maxRetries, attempt =>
            {
                Console.WriteLine($"Retrying ({attempt}/{_maxRetries}) in {_retryDelayMilliseconds}ms...");
                return TimeSpan.FromMilliseconds(_retryDelayMilliseconds);
            });


        var result = await policy.ExecuteAsync(async () =>
        {
            var response = await _client.ExecuteAsync<SportEnglandModel>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response;
            }
            return new RestResponse<SportEnglandModel>();
        });

        return JsonSerializer.Deserialize<SportEnglandModel>(result.Content ?? string.Empty) ?? new SportEnglandModel();
    }
}

