using Polly;
using RestSharp;
using System.Net;
using System.Text.Json;

namespace PublicPartnershipImporter.Services;

public interface IPublicPartnershipClientService
{
    Task<PublicPartnershipSimpleService> GetServicesByPage(int pageNumber);
}

public class PublicPartnershipClientService : IPublicPartnershipClientService
{
    private readonly RestClient _client;
    private readonly int _maxRetries = 3;
    private readonly int _retryDelayMilliseconds = 2000;

    public PublicPartnershipClientService(string baseUri)
    {
        _client = new RestClient(baseUri);
    }

    public async Task<PublicPartnershipSimpleService> GetServicesByPage(int pageNumber)
    {
        var request = new RestRequest($"services/?&page={pageNumber}");

        var policy = Policy
            .HandleResult<RestResponse<PublicPartnershipSimpleService>>(r => r.StatusCode != HttpStatusCode.OK)
            .WaitAndRetryAsync(_maxRetries, attempt =>
            {
                Console.WriteLine($"Retrying ({attempt}/{_maxRetries}) in {_retryDelayMilliseconds}ms...");
                return TimeSpan.FromMilliseconds(_retryDelayMilliseconds);
            });


        var result = await policy.ExecuteAsync(async () =>
        {
            var response = await _client.ExecuteAsync<PublicPartnershipSimpleService>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response;
            }
            return new RestResponse<PublicPartnershipSimpleService>();
        });

        return JsonSerializer.Deserialize<PublicPartnershipSimpleService>(result.Content ?? string.Empty) ?? new PublicPartnershipSimpleService();
    }
}

