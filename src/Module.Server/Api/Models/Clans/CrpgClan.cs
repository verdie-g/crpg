namespace Crpg.Module.Api.Models.Clans;

// Copy of Crpg.Application.Clans.Models.Clan.
internal class CrpgClan
{
    public int Id { get; set; }
    public string Tag { get; set; } = string.Empty;
    public uint PrimaryColor { get; set; }
    public uint SecondaryColor { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BannerKey { get; set; } = string.Empty;
}
