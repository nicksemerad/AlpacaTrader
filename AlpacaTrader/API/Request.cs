using Microsoft.Extensions.Configuration;
using RestSharp;

namespace API;

/// <summary>
///   This class handles making requests to the alpaca API endpoints
/// </summary>
public class Request
{
    private readonly RestClient _client;
    private readonly RestRequest _request;
    
    public Request(string url)
    {
        RestClientOptions opts = new RestClientOptions(url);
        _client = new RestClient(opts);
        _request = new RestRequest();
        AddHeaders(_request);
    }

    private static void AddHeaders(RestRequest request)
    {
        // get the user secrets configuration
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets<Request>()
            .Build();

        // get the api key and secret key from the configuration
        string? apiKey = configuration["API"];
        string? secretKey = configuration["SECRET"];

        // make sure the keys aren't null
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(secretKey))
            throw new ArgumentException("Alpaca api key or private key not found.");

        // add the headers
        request.AddHeader("accept", "application/json");
        request.AddHeader("APCA-API-KEY-ID", apiKey);
        request.AddHeader("APCA-API-SECRET-KEY", secretKey);
    }

    /// <summary>
    ///   Get the response content as a string.
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetAsync()
    {
        RestResponse response = await _client.GetAsync(_request);
        return response.Content ??  string.Empty;
    }
}