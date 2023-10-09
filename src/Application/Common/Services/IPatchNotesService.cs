using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Crpg.Sdk.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Common.Services;

public interface IPatchNotesService
{
    Task<IList<PatchNotes>> GetPatchNotesAsync(CancellationToken cancellationToken);
}

public record PatchNotes(string Id, string Title, Uri Url, DateTime CreatedAt);

internal class GithubPatchNotesService : IPatchNotesService
{
    private static readonly ILogger Logger = LoggerFactory.CreateLogger<DatadogGameServerStatsService>();

    private readonly HttpClient? _githubHttpClient;

    private DateTime _lastUpdate = DateTime.MinValue;
    private IList<PatchNotes>? _patchNotes;

    public GithubPatchNotesService(IConfiguration configuration, IApplicationEnvironment appEnv)
    {
        string? githubAccessToken = configuration["Github:AccessToken"];
        if (githubAccessToken != null)
        {
            _githubHttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.github.com"),
                DefaultRequestHeaders =
                {
                    { "Accept", "application/vnd.github+json" },
                    { "Authorization", "Bearer " + githubAccessToken },
                    { "User-Agent", appEnv.ServiceName },
                    { "X-GitHub-Api-Version", "2022-11-28" },
                },
            };
        }
    }

    public async Task<IList<PatchNotes>> GetPatchNotesAsync(CancellationToken cancellationToken)
    {
        if (_githubHttpClient == null)
        {
            return Array.Empty<PatchNotes>();
        }

        if (DateTime.UtcNow < _lastUpdate + TimeSpan.FromMinutes(2))
        {
            return _patchNotes!;
        }

        IList<PatchNotes>? patchNotes;
        try
        {
            var res = await _githubHttpClient.GetFromJsonAsync<GithubRelease[]>("repos/namidaka/crpg/releases",
                cancellationToken);
            patchNotes = res!
                .Select(r => new PatchNotes(r.Id.ToString(), r.Name, r.HtmlUrl, r.PublishedAt))
                .ToArray();
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Could not get patch notes");
            patchNotes = Array.Empty<PatchNotes>();
        }

        // Both fields can be updated by several threads but the results is the same.
        _lastUpdate = DateTime.UtcNow;
        _patchNotes = patchNotes;
        return _patchNotes;
    }

    private class GithubRelease
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("html_url")]
        public Uri HtmlUrl { get; set; } = default!;

        [JsonPropertyName("published_at")]
        public DateTime PublishedAt { get; set; }
    }
}
