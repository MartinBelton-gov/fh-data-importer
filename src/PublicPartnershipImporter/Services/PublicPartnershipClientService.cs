using RestSharp;

namespace PublicPartnershipImporter.Services;

public interface IPublicPartnershipClientService
{
    Task<PublicPartnershipSimpleService> GetServicesByPage(int pageNumber);
}

public class PublicPartnershipClientService : IPublicPartnershipClientService
{
    private readonly RestClient _client;

    public PublicPartnershipClientService(string baseUri)
    {
        _client = new RestClient(baseUri);
    }

    public async Task<PublicPartnershipSimpleService> GetServicesByPage(int pageNumber)
    {
        var request = new RestRequest($"services/?&page={pageNumber}");

        return await _client.GetAsync<PublicPartnershipSimpleService>(request, CancellationToken.None) ?? new PublicPartnershipSimpleService();
    }
}

