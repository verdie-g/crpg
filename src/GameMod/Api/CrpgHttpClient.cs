using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crpg.GameMod.Api.Models;
using Newtonsoft.Json;

namespace Crpg.GameMod.Api
{
    /// <summary>
    /// Client for Crpg.WebApi.Controllers.GamesController.
    /// </summary>
    internal class CrpgHttpClient : ICrpgClient
    {
        private readonly HttpClient _httpClient;

        public CrpgHttpClient(string jwt)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://api.c-rpg.eu/games/") };
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);
        }

        public Task<CrpgResult<CrpgGameUpdateResponse>> Update(CrpgGameUpdateRequest req, CancellationToken cancellationToken = default)
        {
            return Put<CrpgGameUpdateRequest, CrpgGameUpdateResponse>("update", req, cancellationToken);
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

            return JsonConvert.DeserializeObject<CrpgResult<TResponse>>(json);
        }
    }
}
