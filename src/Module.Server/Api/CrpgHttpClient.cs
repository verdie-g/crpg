using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Helpers.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Crpg.Module.Api;

/// <summary>
/// Client for Crpg.WebApi.Controllers.GamesController.
/// </summary>
internal class CrpgHttpClient : ICrpgClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _serializerSettings;

    public CrpgHttpClient()
    {
        HttpClientHandler httpClientHandler = new() { AutomaticDecompression = DecompressionMethods.GZip };
        _httpClient = new HttpClient(httpClientHandler)
        {
            BaseAddress = new Uri("https://api.c-rpg.eu"),
            Timeout = TimeSpan.FromSeconds(3),
        };
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        string version = typeof(CrpgHttpClient).Assembly.GetName().Version.ToString();
        _httpClient.DefaultRequestHeaders.Add("User-Agent",  "cRPG/" + version);

        _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            },
            Converters = new JsonConverter[]
            {
                new TimeSpanConverter(),
                new ArrayStringEnumFlagsConverter(),
                new StringEnumConverter(),
            },
        };
    }

    public Task<CrpgResult<CrpgUser>> GetUserAsync(Platform platform, string platformUserId,
        string userName, CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> queryParameters = new(StringComparer.Ordinal)
        {
            ["platform"] = platform.ToString(),
            ["platformUserId"] = platformUserId,
            ["userName"] = userName,
        };
        return Get<CrpgUser>("games/users", queryParameters, cancellationToken);
    }

    public Task<CrpgResult<CrpgClan>> GetClanAsync(int clanId, CancellationToken cancellationToken = default)
    {
        return Get<CrpgClan>("games/clans/" + clanId, null, cancellationToken);
    }

    public Task<CrpgResult<CrpgUsersUpdateResponse>> UpdateUsersAsync(CrpgGameUsersUpdateRequest req, CancellationToken cancellationToken = default)
    {
        return Put<CrpgGameUsersUpdateRequest, CrpgUsersUpdateResponse>("games/users", req, cancellationToken);
    }

    public void Dispose() => _httpClient.Dispose();

    private Task<CrpgResult<TResponse>> Get<TResponse>(string requestUri, Dictionary<string, string>? queryParameters,
        CancellationToken cancellationToken) where TResponse : class
    {
        if (queryParameters != null)
        {
            FormUrlEncodedContent urlEncodedContent = new(queryParameters);
            string query = urlEncodedContent.ReadAsStringAsync().Result;
            requestUri += '?' + query;
        }

        HttpRequestMessage msg = new(HttpMethod.Get, requestUri);
        return Send<TResponse>(msg, cancellationToken);
    }

    private Task<CrpgResult<TResponse>> Put<TRequest, TResponse>(string requestUri, TRequest payload, CancellationToken cancellationToken) where TResponse : class
    {
        HttpRequestMessage msg = new(HttpMethod.Put, requestUri)
        {
            Content = new StringContent(JsonConvert.SerializeObject(payload, _serializerSettings), Encoding.UTF8, "application/json"),
        };

        return Send<TResponse>(msg, cancellationToken);
    }

    private Task<CrpgResult<TResponse>> Post<TRequest, TResponse>(string requestUri, TRequest payload, CancellationToken cancellationToken) where TResponse : class
    {
        HttpRequestMessage msg = new(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(JsonConvert.SerializeObject(payload, _serializerSettings), Encoding.UTF8, "application/json"),
        };

        return Send<TResponse>(msg, cancellationToken);
    }

    private async Task<CrpgResult<TResponse>> Send<TResponse>(HttpRequestMessage msg, CancellationToken cancellationToken) where TResponse : class
    {
        for (int retry = 0; retry < 2; retry += 1)
        {
            if (_httpClient.DefaultRequestHeaders.Authorization == null)
            {
                await RefreshAccessToken();
            }

            // TODO: log request
            var res = await _httpClient.SendAsync(msg, cancellationToken);
            string json = await res.Content.ReadAsStringAsync();

            if (res.StatusCode == HttpStatusCode.Unauthorized)
            {
                // TODO: log warn unauthorized.
                _httpClient.DefaultRequestHeaders.Authorization = null;
                continue;
            }

            if (!res.IsSuccessStatusCode)
            {
                throw new Exception($"{res.StatusCode}: {json}");
            }

            return JsonConvert.DeserializeObject<CrpgResult<TResponse>>(json, _serializerSettings);
        }

        throw new Exception("Couldn't send request even after refreshing access token");
    }

    private async Task RefreshAccessToken()
    {
        // TODO: log refreshing
        var tokenRequest = new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "game_api"),
            new KeyValuePair<string, string>("client_id", "crpg-game-server"),
            new KeyValuePair<string, string>("client_secret", "tototo"),
        };

        var tokenResponse = await _httpClient.PostAsync("connect/token", new FormUrlEncodedContent(tokenRequest));
        string tokenResponseBody = await tokenResponse.Content.ReadAsStringAsync();
        if (!tokenResponse.IsSuccessStatusCode)
        {
            throw new Exception("Couldn't get token: " + tokenResponseBody);
        }

        string accessToken = JObject.Parse(tokenResponseBody)["access_token"].ToString();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        // TODO: log refresh ok
    }
}
