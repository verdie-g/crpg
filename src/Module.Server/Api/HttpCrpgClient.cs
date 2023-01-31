using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.ActivityLogs;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Restrictions;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Helpers.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using TaleWorlds.Library;
using Platform = Crpg.Module.Api.Models.Users.Platform;

namespace Crpg.Module.Api;

/// <summary>
/// Client for Crpg.WebApi.Controllers.GamesController.
/// </summary>
internal class HttpCrpgClient : ICrpgClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly JsonSerializerSettings _serializerSettings;

    public HttpCrpgClient(string apiUrl, string apiKey)
    {
        HttpClientHandler httpClientHandler = new() { AutomaticDecompression = DecompressionMethods.GZip };
        _httpClient = new HttpClient(httpClientHandler)
        {
            BaseAddress = new Uri(apiUrl),
            Timeout = TimeSpan.FromSeconds(3),
        };
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        string version = typeof(HttpCrpgClient).Assembly.GetName().Version!.ToString();
        _httpClient.DefaultRequestHeaders.Add("User-Agent",  "cRPG/" + version);

        _apiKey = apiKey;

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
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> queryParameters = new(StringComparer.Ordinal)
        {
            ["platform"] = platform.ToString(),
            ["platformUserId"] = platformUserId,
        };
        return Get<CrpgUser>("games/users", queryParameters, cancellationToken);
    }

    public Task<CrpgResult<CrpgUser>> GetTournamentUserAsync(Platform platform, string platformUserId,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> queryParameters = new(StringComparer.Ordinal)
        {
            ["platform"] = platform.ToString(),
            ["platformUserId"] = platformUserId,
        };
        return Get<CrpgUser>("games/tournament-users", queryParameters, cancellationToken);
    }

    public Task CreateActivityLogsAsync(IList<CrpgActivityLog> activityLogs, CancellationToken cancellationToken = default)
    {
        return Post<IList<CrpgActivityLog>, object>("games/activity-logs", activityLogs, cancellationToken);
    }

    public Task<CrpgResult<CrpgClan>> GetClanAsync(int clanId, CancellationToken cancellationToken = default)
    {
        return Get<CrpgClan>("games/clans/" + clanId, null, cancellationToken);
    }

    public Task<CrpgResult<CrpgUsersUpdateResponse>> UpdateUsersAsync(CrpgGameUsersUpdateRequest req, CancellationToken cancellationToken = default)
    {
        return Put<CrpgGameUsersUpdateRequest, CrpgUsersUpdateResponse>("games/users", req, cancellationToken);
    }

    public Task<CrpgResult<CrpgRestriction>> RestrictUserAsync(CrpgRestrictionRequest req, CancellationToken cancellationToken = default)
    {
        return Post<CrpgRestrictionRequest, CrpgRestriction>("games/restrictions", req, cancellationToken);
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

            Debug.Print($"Sending {msg.Method} {msg.RequestUri}");
            var res = await _httpClient.SendAsync(msg, cancellationToken);
            string json = await res.Content.ReadAsStringAsync();

            if (res.StatusCode == HttpStatusCode.Unauthorized)
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                continue;
            }

            if (!res.IsSuccessStatusCode)
            {
                throw new Exception($"{res.StatusCode}: {json}");
            }

            return JsonConvert.DeserializeObject<CrpgResult<TResponse>>(json, _serializerSettings)!;
        }

        throw new Exception("Couldn't send request even after refreshing access token");
    }

    private async Task RefreshAccessToken()
    {
        Debug.Print("Refreshing access token");
        var tokenRequest = new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "game_api"),
            new KeyValuePair<string, string>("client_id", "crpg-game-server"),
            new KeyValuePair<string, string>("client_secret", _apiKey),
        };

        var tokenResponse = await _httpClient.PostAsync("connect/token", new FormUrlEncodedContent(tokenRequest));
        string tokenResponseBody = await tokenResponse.Content.ReadAsStringAsync();
        if (!tokenResponse.IsSuccessStatusCode)
        {
            throw new Exception("Couldn't get token: " + tokenResponseBody);
        }

        string accessToken = JObject.Parse(tokenResponseBody)["access_token"]!.ToString();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        Debug.Print("Access token successfully refreshed");
    }
}
