using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Clans;

namespace Crpg.Application.Clans.Models;

public record ClanViewModel : IMapFrom<Clan>
{
    public int Id { get; init; }
    public string Tag { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
