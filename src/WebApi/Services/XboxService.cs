using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Crpg.WebApi.Services;

public class XboxService
{
    private const string GamerTagSetting = "Gamertag";
    private const string RawAvatarSetting = "GameDisplayPicRaw";

    private static readonly Uri XasuEndpoint = new("https://user.auth.xboxlive.com/user/authenticate");
    private static readonly Uri XstsEndpoint = new("https://xsts.auth.xboxlive.com/xsts/authorize");
    private static readonly Uri XboxProfileEndpoint = new("https://profile.xboxlive.com/users/me/profile/settings");
    private static readonly HttpClient HttpClient = new();
    private static readonly JsonSerializerOptions CamelCaseJsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task<XboxUser> GetXboxUserAsync(string microsoftAccessToken)
    {
        var xasuTokenResponse = await GetTokenAsync(XasuEndpoint, new XasTokenRequest(
            "JWT",
            "http://auth.xboxlive.com",
            new Dictionary<string, object>
            {
                ["AuthMethod"] = "RPS",
                ["SiteName"] = "user.auth.xboxlive.com",
                ["RpsTicket"] = $"d={microsoftAccessToken}",
            }));

        var xstsTokenResponse = await GetTokenAsync(XstsEndpoint, new XasTokenRequest(
            "JWT",
            "http://xboxlive.com",
            new Dictionary<string, object>
            {
                ["UserTokens"] = new[] { xasuTokenResponse.Token }, ["SandboxId"] = "RETAIL",
            }));

        string userHash = xstsTokenResponse.DisplayClaims["xui"][0]["uhs"];
        var xboxProfile = await GetXboxProfileAsync(userHash, xstsTokenResponse.Token);

        string id = xboxProfile.Id;
        string name = xboxProfile.Settings.First(s => s.Id == GamerTagSetting).Value;
        string rawAvatar = xboxProfile.Settings.First(s => s.Id == RawAvatarSetting).Value;
        Uri avatar = ResolveAvatarUri(rawAvatar);

        return new XboxUser(id, name, avatar);
    }

    private async Task<XasTokenResponse> GetTokenAsync(Uri uri, XasTokenRequest tokenRequest)
    {
        string tokenRequestJson = JsonSerializer.Serialize(tokenRequest);

        var res = await HttpClient.PostAsync(uri,
            new StringContent(tokenRequestJson, Encoding.UTF8, MediaTypeNames.Application.Json));
        res.EnsureSuccessStatusCode();

        string tokenResponseJson = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<XasTokenResponse>(tokenResponseJson)!;
    }

    private async Task<XboxProfile> GetXboxProfileAsync(string userHash, string xstsToken)
    {
        string authorization = $"XBL3.0 x={userHash};{xstsToken}";

        UriBuilder uriBuilder = new(XboxProfileEndpoint) { Query = $"settings={GamerTagSetting},{RawAvatarSetting}" };
        var res = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri)
        {
            Headers =
            {
                { "Authorization", authorization },
                { "x-xbl-contract-version", "3" },
            },
        });
        res.EnsureSuccessStatusCode();

        string xboxProfileResponseJson = await res.Content.ReadAsStringAsync();
        var xboxProfileResponse = JsonSerializer.Deserialize<XboxProfileResponse>(xboxProfileResponseJson, CamelCaseJsonSerializerOptions)!;
        return xboxProfileResponse.ProfileUsers[0];
    }

    private Uri ResolveAvatarUri(string rawAvatar)
    {
        // https://learn.microsoft.com/en-us/gaming/gdk/_content/gc/reference/live/rest/json/json-profile
        return new Uri(rawAvatar + "&w=208&h=208");
    }

    private record XasTokenRequest(string TokenType, string RelyingParty, Dictionary<string, object> Properties);
    private record XasTokenResponse(DateTime IssueInstant, DateTime NotAfter, string Token,
        Dictionary<string, Dictionary<string, string>[]> DisplayClaims);
    private record XboxProfileResponse(XboxProfile[] ProfileUsers, bool IsSponsoredUser);
    private record XboxProfile(string Id, string HostId, IdValuePair[] Settings);
    private record IdValuePair(string Id, string Value);
}

public record XboxUser(string Id, string Name, Uri Avatar);
