using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Crpg.Application.Users.Commands;
using MediatR;

namespace Crpg.WebApi.Workers;

internal class DonorSynchronizerWorker : BackgroundService
{
    private const int MinPatreonAmountCentsForRewards = 500;
    private const int MinAfdianAmountYuanForRewards = 25;

    private static readonly ILogger Logger = Logging.LoggerFactory.CreateLogger<DonorSynchronizerWorker>();

    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DonorSynchronizerWorker(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
    {
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var patreonDonors = await GetPatreonDonorsAsync(cancellationToken);
                var afdianDonors = await GetAfdianDonorsAsync(cancellationToken);

                string[] steamIds = patreonDonors.Concat(afdianDonors).ToArray();

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

    private async Task<IEnumerable<string>> GetPatreonDonorsAsync(CancellationToken cancellationToken)
    {
        string? patreonAccessToken = _configuration.GetValue<string>("Patreon:AccessToken");
        if (patreonAccessToken == null)
        {
            Logger.LogInformation("No Patreon access token was provided. Skipping the donor synchronization");
            return Array.Empty<string>();
        }

        int campaignId = _configuration.GetValue<int>("Patreon:CampaignId");
        using HttpClient client = new() { BaseAddress = new Uri("https://www.patreon.com/api/oauth2/v2/") };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", patreonAccessToken);

        string uri = $"campaigns/{campaignId}/members?fields%5Bmember%5D=currently_entitled_amount_cents,note&page%5Bcount%5D=1000";
        var res = await client.GetFromJsonAsync<PatreonResponse<PatreonCampaignMember>>(uri, cancellationToken);

        List<string> steamIds = new();
        foreach (var member in res!.Data)
        {
            if (member.Attributes.CurrentlyEntitledAmountCents < MinPatreonAmountCentsForRewards * 0.90) // Allow a little margin.
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

        return steamIds;
    }

    private async Task<IEnumerable<string>> GetAfdianDonorsAsync(CancellationToken cancellationToken)
    {
        string? accessToken = _configuration.GetValue<string>("Afdian:AccessToken");
        string? userId = _configuration.GetValue<string>("Afdian:UserId");
        if (accessToken == null || userId == null)
        {
            Logger.LogInformation("No Afdian access token was provided. Skipping the donor synchronization");
            return Array.Empty<string>();
        }

        Dictionary<string, string> afdianToSteamIds = new(StringComparer.Ordinal)
        {
            ["0328ddca7adb11ed84bd52540025c377"] = "76561198170025723",
            ["033f244c7b0611ed9d0452540025c377"] = "76561198833407894",
            ["0c2aed9abce611ecb6ff52540025c377"] = "76561198086189170",
            ["1a4e438e725f11edacfc52540025c377"] = "76561198136476759",
            ["2428127eb58a11ebbe6952540025c377"] = "76561198110700505",
            ["2473cdc48e4411edaf1652540025c377"] = "76561198003871990",
            ["33af69927c2911eda57b52540025c377"] = "76561198070808451",
            ["33c9f5986f7a11ed939352540025c377"] = "76561198146307767",
            ["4864f15290a311edaa405254001e7c00"] = "76561198060776765",
            ["651ee3be726411ed9b5f52540025c377"] = "76561198094460089",
            ["74bf9a867ac311edb57c52540025c377"] = "76561198188192042",
            ["769a7efe7ac111edb6c052540025c377"] = "76561198058883681",
            ["781c9df034d711edbaa452540025c377"] = "76561198812514278",
            ["7a8cf34a79d911ed82be52540025c377"] = "76561198334904020",
            ["7ada19727baf11eda2db52540025c377"] = "76561198185931475",
            ["925644027b9811ed8bfd52540025c377"] = "76561198043640600",
            ["956ba96c725f11ed846952540025c377"] = "76561198277953194",
            ["d4ebfc9a788611ed820252540025c377"] = "76561198140373269",
            ["d6250d3e7b5311edb74652540025c377"] = "76561198062994313",
        };

        using HttpClient httpClient = new() { BaseAddress = new Uri("https://afdian.net/"), };

        const string reqParams = "{\"page\":1}";
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string signature = $"{accessToken}params{reqParams}ts{timestamp}user_id{userId}";
        string sign = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(signature))).ToLowerInvariant();
        string reqContentStr = $"{{\"user_id\":\"{userId}\",\"params\":\"{reqParams.Replace("\"", "\\\"")}\",\"ts\":{timestamp},\"sign\":\"{sign}\"}}";
        StringContent reqContent = new(reqContentStr, Encoding.UTF8, MediaTypeNames.Application.Json);
        using var res = await httpClient.PostAsync("api/open/query-sponsor", reqContent, cancellationToken);
        string resContent = await res.Content.ReadAsStringAsync(cancellationToken);
        var sponsorsRes = JsonSerializer.Deserialize<AfdianResponse<AfdianSponsor>>(resContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        })!;

        List<string> steamIds = new();
        foreach (var sponsor in sponsorsRes.Data.List)
        {
            if (sponsor.CurrentPlan == null || !float.TryParse(sponsor.CurrentPlan.Price, out float price) || price < MinAfdianAmountYuanForRewards * 0.95) // Allow a little margin.
            {
                continue;
            }

            if (!afdianToSteamIds.TryGetValue(sponsor.User.UserId, out string? steamId))
            {
                continue;
            }

            steamIds.Add(steamId);
        }

        return steamIds;
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

    private class AfdianResponse<T>
    {
        public int Ec { get; set; }
        public string Em { get; set; } = string.Empty;
        public AfdianResponseData<T> Data { get; set; } = default!;
    }

    private class AfdianResponseData<T>
    {
        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }
        [JsonPropertyName("total_page")]
        public int TotalPage { get; set; }
        public T[] List { get; set; } = Array.Empty<T>();
    }

    private class AfdianSponsor
    {
        [JsonPropertyName("sponsor_plans")]
        public AfdianSponsorPlan[] SponsorPlans { get; set; } = Array.Empty<AfdianSponsorPlan>();
        [JsonPropertyName("current_plan")]
        public AfdianSponsorPlan? CurrentPlan { get; set; }
        [JsonPropertyName("all_sum_amount")]
        public string AllSumAmount { get; set; } = string.Empty;
        [JsonPropertyName("first_pay_time")]
        public long FirstPayTime { get; set; }
        [JsonPropertyName("last_pay_time")]
        public long LastPayTime { get; set; }
        [JsonPropertyName("user")]
        public AfdianUser User { get; set; } = default!;
    }

    private class AfdianSponsorPlan
    {
        [JsonPropertyName("plan_id")]
        public string PlanId { get; set; } = string.Empty;
        [JsonPropertyName("rank")]
        public int Rank { get; set; }
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;
        [JsonPropertyName("status")]
        public int Status { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("pic")]
        public string Pic { get; set; } = string.Empty;
        [JsonPropertyName("desc")]
        public string Desc { get; set; } = string.Empty;
        [JsonPropertyName("price")]
        public string Price { get; set; } = string.Empty;
        [JsonPropertyName("update_time")]
        public int UpdateTime { get; set; }
        // [JsonPropertyName("timing")]
        // public AfdianTiming Timing { get; set; }
        [JsonPropertyName("pay_month")]
        public int PayMonth { get; set; }
        [JsonPropertyName("show_price")]
        public string ShowPrice { get; set; } = string.Empty;
        [JsonPropertyName("show_price_after_adjust")]
        public string ShowPriceAfterAdjust { get; set; } = string.Empty;
        [JsonPropertyName("has_coupon")]
        public int HasCoupon { get; set; }
        // [JsonPropertyName("coupon")]
        // public List<AfdianCoupon> Coupon { get; set; }
        [JsonPropertyName("favorable_price")]
        public int FavorablePrice { get; set; }
        [JsonPropertyName("independent")]
        public int Independent { get; set; }
        [JsonPropertyName("permanent")]
        public int Permanent { get; set; }
        [JsonPropertyName("can_buy_hide")]
        public int CanBuyHide { get; set; }
        [JsonPropertyName("need_address")]
        public int NeedAddress { get; set; }
        [JsonPropertyName("product_type")]
        public int ProductType { get; set; }
        [JsonPropertyName("sale_limit_count")]
        public int SaleLimitCount { get; set; }
        [JsonPropertyName("need_invite_code")]
        public bool NeedInviteCode { get; set; }
        [JsonPropertyName("bundle_stock")]
        public int BundleStock { get; set; }
        [JsonPropertyName("bundle_sku_select_count")]
        public int BundleSkuSelectCount { get; set; }
        // [JsonPropertyName("config")]
        // public AfdianConfig Config { get; set; }
        [JsonPropertyName("has_plan_config")]
        public int HasPlanConfig { get; set; }
        [JsonPropertyName("expire_time")]
        public int ExpireTime { get; set; }
        // [JsonPropertyName("sku_processed")]
        // public AfdianSkuProcessed[] SkuProcessed
    }

    private class AfdianUser
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; } = string.Empty;

        [JsonPropertyName("user_private_id")]
        public string UserPrivateId { get; set; } = string.Empty;
    }
}
