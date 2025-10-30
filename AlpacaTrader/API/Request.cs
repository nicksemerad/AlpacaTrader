using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Api;

/// <summary>
///   This class handles making requests to the alpaca API endpoints
/// </summary>
public class Request
{
    /// <summary>
    ///   The ILogger used to log events in this class.
    /// </summary>
    private static readonly ILogger RequestLog = Logger.Create<Request>();

    /// <summary>
    ///   The RestClient object that will be used to make the request.
    /// </summary>
    private readonly RestClient _client;

    /// <summary>
    ///   The RestRequest object requested by the _client.
    /// </summary>
    private readonly RestRequest _request;

    /// <summary>
    ///   The url that will be requested, in string form.
    /// </summary>
    private readonly string _url;

    /// <summary>
    ///   Builds a new Request for the url. Headers for the alpaca secret key and API key are added, as well as a
    ///   header stating to accept json responses.
    /// </summary>
    /// <param name="url"></param>
    public Request(string url)
    {
        var opts = new RestClientOptions(url);
        _client = new RestClient(opts);
        _request = new RestRequest();
        _url = url;
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

        // get the API key and secret key from the configuration
        var apiKey = configuration["API"];
        var secretKey = configuration["SECRET"];

        // make sure the keys aren't null
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(secretKey))
            throw new ArgumentException("Alpaca API key or private key not found.");

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
        // make the request and log the result details
        var response = await _client.GetAsync(_request);
        if (response.Content is null)
            RequestLog.LogError("REQUEST FAILED: response content is null");
        else
            RequestLog.LogInformation("=> {Url} [STATUS: {Code}] - {Desc}", _url, (int)response.StatusCode,
                response.StatusCode);

        // return an empty string if content is null
        return response.Content ?? string.Empty;
    }
}