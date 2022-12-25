using System.Net.Http.Json;
using System.Text.Json;
using Crpg.Application.Games.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Common.Services;

public interface IGameServerStatsService
{
    Task<GameServerStats> GetGameServerStatsAsync(CancellationToken cancellationToken);
}

public class DatadogGameServerStatsService : IGameServerStatsService
{
    private static readonly ILogger Logger = LoggerFactory.CreateLogger<DatadogGameServerStatsService>();
    private readonly HttpClient? _ddHttpClient;

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

    public async Task<GameServerStats> GetGameServerStatsAsync(CancellationToken cancellationToken)
    {
        if (_ddHttpClient == null)
        {
            return new GameServerStats
            {
                PlayingCount = 0,
            };
        }

        var to = DateTimeOffset.UtcNow;
        var from = to - TimeSpan.FromMinutes(5);
        FormUrlEncodedContent query = new(new[]
        {
            KeyValuePair.Create("from", from.ToUnixTimeSeconds().ToString()),
            KeyValuePair.Create("to", to.ToUnixTimeSeconds().ToString()),
            KeyValuePair.Create("query", "sum:crpg.users.playing.count{*}"),
        });
        string queryStr = await query.ReadAsStringAsync(cancellationToken);

        try
        {
            var doc = await _ddHttpClient.GetFromJsonAsync<JsonDocument>("api/v1/query?" + queryStr, cancellationToken);
            var pointListEl = doc!.RootElement.GetProperty("series").EnumerateArray().First().GetProperty("pointlist");
            if (pointListEl.GetArrayLength() > 0)
            {
                int playingCount = (int)pointListEl.EnumerateArray().Last().EnumerateArray().Last().GetDouble();
                return new GameServerStats
                {
                    PlayingCount = playingCount,
                };
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Could not get server stats");
        }

        return new GameServerStats
        {
            PlayingCount = 0,
        };
    }
}
