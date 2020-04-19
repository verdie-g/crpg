using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crpg.GameMod.Api.Requests;
using Crpg.GameMod.Api.Responses;
using Newtonsoft.Json;

namespace Crpg.GameMod.Api
{
    /// <summary>
    /// Client for Crpg.WebApi.Controllers.GamesController.
    /// </summary>
    internal class CrpgClient : ICrpgClient
    {
        private readonly HttpClient _httpClient;

        public CrpgClient(string jwt)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8000/api/games/") };
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);
        }

        public Task<GetUserResponse> GetOrCreateUser(GetUserRequest req, CancellationToken cancellationToken = default)
        {
            return Put<GetUserRequest, GetUserResponse>("users", req, cancellationToken);
        }

        public Task<TickResponse> Tick(TickRequest req, CancellationToken cancellationToken = default)
        {
            return Post<TickRequest, TickResponse>("ticks", req, cancellationToken);
        }

        private Task<TResponse> Put<TRequest, TResponse>(string requestUri, TRequest payload, CancellationToken cancellationToken)
        {
            var msg = new HttpRequestMessage(HttpMethod.Put, requestUri)
            {
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };

            return Send<TResponse>(msg, cancellationToken);
        }

        private Task<TResponse> Post<TRequest, TResponse>(string requestUri, TRequest payload, CancellationToken cancellationToken)
        {
            var msg = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };

            return Send<TResponse>(msg, cancellationToken);
        }

        private async Task<TResponse> Send<TResponse>(HttpRequestMessage msg, CancellationToken cancellationToken)
        {
            var res = await _httpClient.SendAsync(msg, cancellationToken);
            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                if (res.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new Exception("Invalid or expired token");
                }

                throw new Exception(json);
            }

            return JsonConvert.DeserializeObject<TResponse>(json);
        }
    }
}