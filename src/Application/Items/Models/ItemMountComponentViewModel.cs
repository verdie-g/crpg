using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record ItemMountComponentViewModel : IMapFrom<ItemMountComponent>
{
    public int BodyLength { get; init; }
    public int ChargeDamage { get; init; }
    public int Maneuver { get; init; }
    public int Speed { get; init; }
    public int HitPoints { get; init; }
    public int FamilyType { get; init; }
}
