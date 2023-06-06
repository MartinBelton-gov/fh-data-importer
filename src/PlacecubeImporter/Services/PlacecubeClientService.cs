using Polly;
using RestSharp;
using System.Net;
using System.Text.Json;

namespace PlacecubeImporter.Services
{
    internal interface IPlacecubeClientService
    {
        Task<PlacecubeSimpleService> GetServicesByPage(int pageNumber);
        Task<PlacecubeService> GetServiceById(string id);
    }

    internal class PlacecubeClientService : IPlacecubeClientService
    {
        private readonly RestClient _client;
        private readonly int _maxRetries = 3;
        private readonly int _retryDelayMilliseconds = 2000;

        public PlacecubeClientService(string baseUri)
        {
            _client = new RestClient(baseUri);
        }

        public async Task<PlacecubeSimpleService> GetServicesByPage(int pageNumber)
        {
            var request = new RestRequest($"services/?&page={pageNumber}");

            var policy = Policy
                .HandleResult<RestResponse<PlacecubeSimpleService>>(r => r.StatusCode != HttpStatusCode.OK)
                .WaitAndRetryAsync(_maxRetries, attempt =>
                {
                    Console.WriteLine($"Retrying ({attempt}/{_maxRetries}) in {_retryDelayMilliseconds}ms...");
                    return TimeSpan.FromMilliseconds(_retryDelayMilliseconds);
                });


            var result = await policy.ExecuteAsync(async () =>
            {
                var response = await _client.ExecuteAsync<PlacecubeSimpleService>(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return response;
                }
                return new RestResponse<PlacecubeSimpleService>();
            });

            return JsonSerializer.Deserialize<PlacecubeSimpleService>(result.Content ?? string.Empty) ?? new PlacecubeSimpleService();
        }

        public async Task<PlacecubeService> GetServiceById(string id)
        {
            var request = new RestRequest($"services/{id}");

            var policy = Policy
                .HandleResult<RestResponse<PlacecubeService>>(r => r.StatusCode != HttpStatusCode.OK)
                .WaitAndRetryAsync(_maxRetries, attempt =>
                {
                    Console.WriteLine($"Retrying ({attempt}/{_maxRetries}) in {_retryDelayMilliseconds}ms...");
                    return TimeSpan.FromMilliseconds(_retryDelayMilliseconds);
                });


            var result = await policy.ExecuteAsync(async () =>
            {
                var response = await _client.ExecuteAsync<PlacecubeService>(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return response;
                }
                return new RestResponse<PlacecubeService>();
            });

            return JsonSerializer.Deserialize<PlacecubeService>(result.Content ?? string.Empty) ?? new PlacecubeService();
        }
    }
}
