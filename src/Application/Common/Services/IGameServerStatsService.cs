using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Crpg.Application.Games.Models;
using Crpg.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Common.Services;

public interface IGameServerStatsService
{
    Task<GameServerStats?> GetGameServerStatsAsync(CancellationToken cancellationToken);
}

public class DatadogGameServerStatsService : IGameServerStatsService
{
    private static readonly ILogger Logger = LoggerFactory.CreateLogger<DatadogGameServerStatsService>();
    private readonly HttpClient? _ddHttpClient;

    private DateTime _lastUpdate = DateTime.MinValue;
    private GameServerStats? _serverStats;

    public DatadogGameServerStatsService(IConfiguration configuration)
    {
        string? ddApiKey = configuration["Datadog:ApiKey"];
        string? ddApplicationKey = configuration["Datadog:ApplicationKey"];
        if (ddApiKey != null && ddApplicationKey != null)
        {
            _ddHttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.datadoghq.com/"),
                DefaultRequestHeaders =
                {
                    { "DD-API-KEY", ddApiKey },
                    { "DD-APPLICATION-KEY", ddApplicationKey },
                },
            };
        }
    }

    public async Task<GameServerStats?> GetGameServerStatsAsync(CancellationToken cancellationToken)
    {
        if (_ddHttpClient == null)
        {
            return null;
        }

        if (DateTime.UtcNow < _lastUpdate + TimeSpan.FromMinutes(2))
        {
            return _serverStats;
        }

        var to = DateTimeOffset.UtcNow;
        var from = to - TimeSpan.FromMinutes(5);
        FormUrlEncodedContent query = new(new[]
        {
            KeyValuePair.Create("from", from.ToUnixTimeSeconds().ToString()),
            KeyValuePair.Create("to", to.ToUnixTimeSeconds().ToString()),
            KeyValuePair.Create("query", "sum:crpg.users.playing.count{*} by {region}"),
        });
        string queryStr = await query.ReadAsStringAsync(cancellationToken);

        GameServerStats? serverStats;
        try
        {
            serverStats = new()
            {
                Total = new GameStats { PlayingCount = 0 },
                Regions = new Dictionary<Region, GameStats>(),
            };

            var res = await _ddHttpClient.GetFromJsonAsync<DatadogQueryResponse>("api/v1/query?" + queryStr, cancellationToken);

            foreach (var series in res!.Series)
            {
                string regionStr = series.Scope[^2..];
                var region = Enum.Parse<Region>(regionStr, ignoreCase: true);

                int playingCount = series.PointList.Length > 0 ? (int)series.PointList[^1][^1] : 0;
                serverStats.Total.PlayingCount += playingCount;
                serverStats.Regions[region] = new GameStats { PlayingCount = playingCount };
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Could not get server stats");
            serverStats = null;
        }

        // Both fields can be updated by several threads but the results is the same.
        _lastUpdate = DateTime.UtcNow;
        _serverStats = serverStats;
        return _serverStats;
    }

    private class DatadogQueryResponse
    {
        public string Status { get; set; } = string.Empty;
        [JsonPropertyName("res_type")]
        public string ResType { get; set; } = string.Empty;
        [JsonPropertyName("resp_version")]
        public int RespVersion { get; set; }
        public string Query { get; set; } = string.Empty;
        [JsonPropertyName("from_date")]
        public long FromDate { get; set; }
        [JsonPropertyName("to_date")]
        public long ToDate { get; set; }
        public DatadogSeries[] Series { get; set; } = Array.Empty<DatadogSeries>();
        public object[] Values { get; set; } = Array.Empty<object>();
        public object[] Times { get; set; } = Array.Empty<object>();
        public string Message { get; set; } = string.Empty;
        [JsonPropertyName("group_by")]
        public string[] GroupBy { get; set; } = Array.Empty<string>();
    }

    private class DatadogSeries
    {
        public object Unit { get; set; } = string.Empty;
        [JsonPropertyName("query_index")]
        public int QueryIndex { get; set; }
        public string Aggr { get; set; } = string.Empty;
        public string Metric { get; set; } = string.Empty;
        [JsonPropertyName("tag_set")]
        public string[] TagSet { get; set; } = Array.Empty<string>();
        public string Expression { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public int Interval { get; set; }
        public int Length { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public double[][] PointList { get; set; } = Array.Empty<double[]>();
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;
        public Dictionary<string, object> Attributes { get; set; } = new();
    }
}
