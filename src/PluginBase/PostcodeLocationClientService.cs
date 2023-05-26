using RestSharp;

namespace PluginBase;

public interface IPostcodeLocationClientService
{
    Task<PostcodesIoResponse> LookupPostcode(string postcode);
    Task<ListPostcodesIoResponse> GetNearestPostcodeFromCoordinates(double longtitude, double latitude);
}
public class PostcodeLocationClientService : IPostcodeLocationClientService
{
    private readonly Dictionary<string, PostcodesIoResponse> _postCodesCache = new Dictionary<string, PostcodesIoResponse>();

    private readonly RestClient _client;
    public PostcodeLocationClientService(string baseUri)
    {
        _client = new RestClient(baseUri);
    }

    public async Task<PostcodesIoResponse> LookupPostcode(string postcode)
    {
        var formattedPostCode = postcode.Replace(" ", "").ToLower();

        if (_postCodesCache.ContainsKey(formattedPostCode))
            return _postCodesCache[formattedPostCode];

        var request = new RestRequest($"/postcodes/{formattedPostCode}");

        PostcodesIoResponse postcodesIoResponse = await _client.GetAsync<PostcodesIoResponse>(request, CancellationToken.None) ?? new PostcodesIoResponse();

        _postCodesCache.Add(formattedPostCode, postcodesIoResponse!);

        return postcodesIoResponse!;
    }

    public async Task<ListPostcodesIoResponse> GetNearestPostcodeFromCoordinates(double longtitude, double latitude)
    {
        var request = new RestRequest($"/postcodes/?lon={longtitude}&lat={latitude}");

        ListPostcodesIoResponse listpostcodesIoResponse = await _client.GetAsync<ListPostcodesIoResponse>(request, CancellationToken.None) ?? new ListPostcodesIoResponse();
        
        return listpostcodesIoResponse!;
    }
}
