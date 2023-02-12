using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Clans;

namespace Crpg.Application.Clans.Models;

public record ClanViewModel : IMapFrom<Clan>
{
    public int Id { get; init; }
    public string Tag { get; init; } = string.Empty;
    public uint PrimaryColor { get; init; }
    public uint SecondaryColor { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string BannerKey { get; init; } = string.Empty;
    public Region Region { get; init; }
    public Uri? Discord { get; init; }
}
