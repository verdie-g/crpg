using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crpg.GameMod.Api.Models;
using Crpg.GameMod.Api.Models.Items;
using Crpg.GameMod.Api.Models.Users;
using Crpg.GameMod.Helpers.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Crpg.GameMod.Api
{
    /// <summary>
    /// Client for Crpg.WebApi.Controllers.GamesController.
    /// </summary>
    internal class CrpgHttpClient : ICrpgClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerSettings _serializerSettings;

        public CrpgHttpClient()
        {
            var httpClientHandler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip };
            _httpClient = new HttpClient(httpClientHandler) { BaseAddress = new Uri("https://localhost:8000") };
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy(),
                },
                Converters = new JsonConverter[] { new ArrayStringEnumFlagsConverter(), new StringEnumConverter() },
            };
        }

        public Task<CrpgResult<CrpgUser>> GetUser(Platform platform, string platformUserId,
            string characterName, CancellationToken cancellationToken = default)
        {
            var queryParameters = new Dictionary<string, string>
            {
                ["platform"] = platform.ToString(),
                ["platformUserId"] = platformUserId,
                ["userName"] = characterName,
            };
            return Get<CrpgUser>("games/users", queryParameters, cancellationToken);
        }

        public Task<CrpgResult<IList<CrpgItem>>> GetItems(CancellationToken cancellationToken = default)
        {
            return Get<IList<CrpgItem>>("games/items", null, cancellationToken);
        }

        public Task<CrpgResult<CrpgUsersUpdateResponse>> Update(CrpgGameUsersUpdateRequest req, CancellationToken cancellationToken = default)
        {
            return Put<CrpgGameUsersUpdateRequest, CrpgUsersUpdateResponse>("games/users", req, cancellationToken);
        }

        public void Dispose() => _httpClient.Dispose();

        private Task<CrpgResult<TResponse>> Get<TResponse>(string requestUri, Dictionary<string, string>? queryParameters,
            CancellationToken cancellationToken) where TResponse : class
        {
            if (queryParameters != null)
            {
                var urlEncodedContent = new FormUrlEncodedContent(queryParameters);
                string query = urlEncodedContent.ReadAsStringAsync().Result;
                requestUri += '?' + query;
            }

            var msg = new HttpRequestMessage(HttpMethod.Get, requestUri);
            return Send<TResponse>(msg, cancellationToken);
        }

        private Task<CrpgResult<TResponse>> Put<TRequest, TResponse>(string requestUri, TRequest payload, CancellationToken cancellationToken) where TResponse : class
        {
            var msg = new HttpRequestMessage(HttpMethod.Put, requestUri)
            {
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"),
            };

            return Send<TResponse>(msg, cancellationToken);
        }

        private Task<CrpgResult<TResponse>> Post<TRequest, TResponse>(string requestUri, TRequest payload, CancellationToken cancellationToken) where TResponse : class
        {
            var msg = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"),
            };

            return Send<TResponse>(msg, cancellationToken);
        }

        private async Task<CrpgResult<TResponse>> Send<TResponse>(HttpRequestMessage msg, CancellationToken cancellationToken) where TResponse : class
        {
            int retry = 0;
            while (retry < 2)
            {
                retry += 1;

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
                    throw new Exception(json);
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
}
