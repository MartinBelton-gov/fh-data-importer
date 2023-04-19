using RestSharp;

namespace PluginBase;

public interface IPostcodeLocationClientService
{
    Task<PostcodesIoResponse> LookupPostcode(string postcode);
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
}
