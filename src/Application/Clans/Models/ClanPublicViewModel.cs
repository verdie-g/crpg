using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Clans;

namespace Crpg.Application.Clans.Models;

public record ClanPublicViewModel : IMapFrom<Clan>
{
    public int Id { get; init; }
    public string Tag { get; init; } = string.Empty;
    public string PrimaryColor { get; init; } = string.Empty;
    public string SecondaryColor { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string BannerKey { get; init; } = string.Empty;
    public Region Region { get; set; }
}
