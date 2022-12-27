namespace Crpg.Module.Api;

internal static class CrpgClient
{
    public static ICrpgClient Create()
    {
        string? apiUrl = Environment.GetEnvironmentVariable("CRPG_API_BASE_URL");
        string? apiKey = Environment.GetEnvironmentVariable("CRPG_API_KEY");
        return apiUrl == null || apiKey == null ? new StubCrpgClient() : new HttpCrpgClient(apiUrl, apiKey);
    }
}
