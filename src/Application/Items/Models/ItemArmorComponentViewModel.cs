using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record ItemArmorComponentViewModel : IMapFrom<ItemArmorComponent>
{
    public int HeadArmor { get; init; }
    public int BodyArmor { get; init; }
    public int ArmArmor { get; init; }
    public int LegArmor { get; init; }
    public ArmorMaterialType MaterialType { get; init; }
}
