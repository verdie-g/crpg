using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Crpg.Application.Users.Commands;
using MediatR;

namespace Crpg.WebApi.Workers;

internal class DonorSynchronizerWorker : IHostedService
{
    private const int MinAmountCentsForRewards = 500;

    private static readonly ILogger Logger = Logging.LoggerFactory.CreateLogger<DonorSynchronizerWorker>();

    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DonorSynchronizerWorker(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
    {
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string patreonAccessToken = _configuration.GetValue<string>("Patreon:AccessToken");
        if (patreonAccessToken == null)
        {
            Logger.LogInformation("No Patreon access token was provided. Disabling the donor synchronization");
            return;
        }

        int campaignId = _configuration.GetValue<int>("Patreon:CampaignId");
        HttpClient client = new()
        {
            BaseAddress = new Uri("https://www.patreon.com/api/oauth2/v2/"),
        };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", patreonAccessToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                string uri = $"campaigns/{campaignId}/members?fields%5Bmember%5D=currently_entitled_amount_cents,note&page%5Bcount%5D=1000";
                var res = await client.GetFromJsonAsync<PatreonResponse<PatreonCampaignMember>>(uri, cancellationToken);

                List<string> steamIds = new();
                foreach (var member in res!.Data)
                {
                    if (member.Attributes.CurrentlyEntitledAmountCents < MinAmountCentsForRewards)
                    {
                        continue;
                    }

                    string[] noteLines = member.Attributes.Note.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    foreach (string noteLine in noteLines)
                    {
                        string[] noteParts = noteLine.Split(':', StringSplitOptions.RemoveEmptyEntries);
                        if (noteParts.Length == 2 && noteParts[0] == "steam")
                        {
                            steamIds.Add(noteParts[1]);
                        }
                    }
                }

                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new UpdateUserDonorsCommand { PlatformUserIds = steamIds }, cancellationToken);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An error occured while updating donors");
            }

            await Task.Delay(TimeSpan.FromHours(1), cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private class PatreonCampaignMember
    {
        [JsonPropertyName("currently_entitled_amount_cents")]
        public int CurrentlyEntitledAmountCents { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    private class PatreonResponse<T>
    {
        public PatreonResponseData<T>[] Data { get; set; } = Array.Empty<PatreonResponseData<T>>();
    }

    private class PatreonResponseData<T>
    {
        public Guid Id { get; set; }
        public T Attributes { get; set; } = default!;
        public string Type { get; set; } = string.Empty;
    }
}
