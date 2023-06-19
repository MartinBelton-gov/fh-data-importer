using Newtonsoft.Json;
using Polly;
using RestSharp;
using System.Net;
//using System.Text.Json;

namespace HounslowconnectImporter.Services;

/*
 call https://api.hounslowconnect.com/core/v1/services?page=1&per_page=25
get Id
https://api.hounslowconnect.com/core/v1/service-locations?page=1&per_page=25&filter[service_id]=756ff4a3-45e4-4542-87ca-6327f558af30
get Location Id
Call get Location
 */

//Base Url: https://api.hounslowconnect.com/core/v1/services?page=1&per_page=25
public interface IConnectClientService<T> where T : new()
{
    Task<T> GetServices(string url);
    Task<Location> GetLocation(string locationId);
    Task<Organisation> GetOrganisation(string organisationId);
    Task<ServiceLocations> GetServiceLocation(string serviceId);
}

public class ConnectClientService<T> : IConnectClientService<T> where T : new()
{
    private readonly RestClient _client;
    private readonly JsonSerializerSettings _jsonSerializerSettings;
    private readonly int _maxRetries = 3;
    private readonly int _retryDelayMilliseconds = 2000;

    public ConnectClientService(string baseUri)
    {
        _client = new RestClient(baseUri);
        _jsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = new[] { new CustomDateTimeConverter() }
        };
    }

    public async Task<T> GetServices(string url)
    {
        var request = new RestRequest($"services?{url}");

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

        return JsonConvert.DeserializeObject<T>(result.Content ?? string.Empty, _jsonSerializerSettings) ?? new T();
    }

    // Base Url: https://api.hounslowconnect.com/core/v1/locations?page=1

    public async Task<Location> GetLocation(string locationId)
    {
        var request = new RestRequest($"locations/{locationId}");

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

        return JsonConvert.DeserializeObject<Location>(result.Content ?? string.Empty, _jsonSerializerSettings) ?? new Location();
    }

    public async Task<ServiceLocations> GetServiceLocation(string serviceId)
    {
        var request = new RestRequest($"service-locations?page=1&per_page=25&filter[service_id]={serviceId}");

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

        return JsonConvert.DeserializeObject<ServiceLocations>(result.Content ?? string.Empty, _jsonSerializerSettings) ?? new ServiceLocations();
    }

    public async Task<Organisation> GetOrganisation(string organisationId)
    {
        var request = new RestRequest($"organisations/{organisationId}");

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

        return JsonConvert.DeserializeObject<Organisation>(result.Content ?? string.Empty, _jsonSerializerSettings) ?? new Organisation();
    }
}

