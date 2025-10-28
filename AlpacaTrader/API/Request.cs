using Microsoft.Extensions.Configuration;
using RestSharp;

namespace API;

/// <summary>
///   This class handles making requests to the alpaca API endpoints
/// </summary>
public class Request
{
    /// <summary>
    ///   The RestClient object that will be used to make the request.
    /// </summary>
    private readonly RestClient _client;

    /// <summary>
    ///   The RestRequest object requested by the _client.
    /// </summary>
    private readonly RestRequest _request;

    /// <summary>
    ///   Builds a new Request for the url. Headers for the alpaca secret key and api key are added, as well as a
    ///   header stating to accept json responses.
    /// </summary>
    /// <param name="url"></param>
    public Request(string url)
    {
        var opts = new RestClientOptions(url);
        _client = new RestClient(opts);
        _request = new RestRequest();
        AddHeaders();
    }

    /// <summary>
    ///   Using the Configuration package the Alpaca API Secret-Key and API-Key are retrieved from project user
    ///   secrets. The two authentication headers are made with these keys and added to _request. A final
    ///   header is added to accept json. If either private keys failed to be retrieved an ArgumentException is thrown.
    /// </summary>
    /// <exception cref="ArgumentException">Throws if the secrets fail to be retrieved</exception>
    private void AddHeaders()
    {
        // get the user secrets configuration
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets<Request>()
            .Build();

        // get the api key and secret key from the configuration
        var apiKey = configuration["API"];
        var secretKey = configuration["SECRET"];

        // make sure the keys aren't null
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(secretKey))
            throw new ArgumentException("Alpaca api key or private key not found.");

        // add the headers
        _request.AddHeader("APCA-API-KEY-ID", apiKey);
        _request.AddHeader("APCA-API-SECRET-KEY", secretKey);
        _request.AddHeader("accept", "application/json");
    }

    /// <summary>
    ///   This Request's RestClient is used to send the RestRequest to the Alpaca API url. After waiting for the
    ///   RestResponse object to return, the response content string is taken and returned. If there was no response
    ///   content, an empty string is returned instead.
    /// </summary>
    /// <returns>The Request response's content string, or an empty string if there was no response content</returns>
    public async Task<string> GetAsync()
    {
        var response = await _client.GetAsync(_request);
        // log the request to the console
        Console.WriteLine($"REQUEST: {_client.Options.BaseUrl?.ToString()} STATUS: {response.StatusCode}");
        return response.Content ?? string.Empty;
    }
}