using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crpg.GameMod.Api.Models;
using Crpg.GameMod.Api.Models.Users;
using Crpg.GameMod.Helpers.Json;
using IdentityModel.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:8000") };
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

        public async Task Initialize()
        {
            DiscoveryDocumentResponse discoResponse = await _httpClient.GetDiscoveryDocumentAsync();
            if (discoResponse.IsError)
            {
                throw new Exception("Couldn't get discovery document: " + discoResponse.Error);
            }

            // request token
            var tokenResponse = await _httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = discoResponse.TokenEndpoint,
                ClientId = "crpg_game_server",
                ClientSecret = "tototo",
                Scope = "game_api"
            });

            if (tokenResponse.IsError)
            {
                throw new Exception("Couldn't get token: " + tokenResponse.Error);
            }

            _httpClient.SetBearerToken(tokenResponse.AccessToken);
        }

        public Task<CrpgResult<CrpgUser>> GetUser(Platform platform, string platformUserId,
            string characterName, CancellationToken cancellationToken = default)
        {
            var queryParameters = new Dictionary<string, string>
            {
                ["platform"] = platform.ToString(),
                ["platformUserId"] = platformUserId,
                ["userName"] = characterName
            };
            return Get<CrpgUser>("games/users", queryParameters, cancellationToken);
        }

        public Task<CrpgResult<CrpgUsersUpdateResponse>> Update(CrpgGameUsersUpdateRequest req, CancellationToken cancellationToken = default)
        {
            return Put<CrpgGameUsersUpdateRequest, CrpgUsersUpdateResponse>("games/users", req, cancellationToken);
        }

        private Task<CrpgResult<TResponse>> Get<TResponse>(string requestUri, Dictionary<string, string> queryParameters,
            CancellationToken cancellationToken) where TResponse : class
        {
            var urlEncodedContent = new FormUrlEncodedContent(queryParameters);
            string query = urlEncodedContent.ReadAsStringAsync().Result;
            var msg = new HttpRequestMessage(HttpMethod.Get, requestUri + '?' + query);
            return Send<TResponse>(msg, cancellationToken);
        }

        private Task<CrpgResult<TResponse>> Put<TRequest, TResponse>(string requestUri, TRequest payload, CancellationToken cancellationToken) where TResponse : class
        {
            var msg = new HttpRequestMessage(HttpMethod.Put, requestUri)
            {
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };

            return Send<TResponse>(msg, cancellationToken);
        }

        private Task<CrpgResult<TResponse>> Post<TRequest, TResponse>(string requestUri, TRequest payload, CancellationToken cancellationToken) where TResponse : class
        {
            var msg = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };

            return Send<TResponse>(msg, cancellationToken);
        }

        private async Task<CrpgResult<TResponse>> Send<TResponse>(HttpRequestMessage msg, CancellationToken cancellationToken) where TResponse : class
        {
            var res = await _httpClient.SendAsync(msg, cancellationToken);
            string json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                if (res.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new Exception("Invalid or expired token");
                }

                throw new Exception(json);
            }

            return JsonConvert.DeserializeObject<CrpgResult<TResponse>>(json, _serializerSettings);
        }

        public void Dispose() => _httpClient.Dispose();
    }
}
