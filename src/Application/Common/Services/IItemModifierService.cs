using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Common.Services;

/// <summary>
/// Service for looming and repair <see cref="Item"/>.
/// </summary>
internal interface IItemModifierService
{
    /// <summary>
    /// Create a new item from an rank 0 item and given rank.
    /// </summary>
    /// <param name="Item">Rank 0 item.</param>
    /// <param name="rank">Result rank.</param>
    /// <returns>A new item instance of rank <paramref name="rank"/>.</returns>
    Item ModifyItem(Item Item, int rank);
}

/// <inheritdoc />
internal class ItemModifierService : IItemModifierService
{
    private readonly Dictionary<ItemType, ItemModifier[]> _itemModifiers;

    public ItemModifierService(ItemModifiers itemModifiers)
    {
        _itemModifiers = new Dictionary<ItemType, ItemModifier[]>
        {
            // ReSharper disable CoVariantArrayConversion
            [ItemType.HeadArmor] = itemModifiers.Armor,
            [ItemType.ShoulderArmor] = itemModifiers.Armor,
            [ItemType.BodyArmor] = itemModifiers.Armor,
            [ItemType.HandArmor] = itemModifiers.Armor,
            [ItemType.LegArmor] = itemModifiers.Armor,
            [ItemType.MountHarness] = itemModifiers.Armor,
            [ItemType.Mount] = itemModifiers.Mount,
            [ItemType.Shield] = itemModifiers.Shield,
            [ItemType.Bow] = itemModifiers.Bow,
            [ItemType.Crossbow] = itemModifiers.Crossbow,
            [ItemType.OneHandedWeapon] = itemModifiers.Weapon,
            [ItemType.TwoHandedWeapon] = itemModifiers.Weapon,
            [ItemType.Polearm] = itemModifiers.Polearm,
            [ItemType.Thrown] = itemModifiers.Thrown,
            [ItemType.Arrows] = itemModifiers.Missile,
            [ItemType.Bolts] = itemModifiers.Missile,
            // ReSharper restore CoVariantArrayConversion
        };
    }

    /// <inheritdoc />
    public Item ModifyItem(Item Item, int rank)
    {
        if (rank < -3 || rank > 3)
        {
            throw new ArgumentException("Rank should be between -3 and 3");
        }

        var clone = (Item)Item.Clone();
        if (rank == 0)
        {
            return clone;
        }

        if (!_itemModifiers.TryGetValue(Item.Type, out ItemModifier[]? typeItemModifiers))
        {
            // For banners and firearms use a random type for now.
            typeItemModifiers = _itemModifiers[ItemType.OneHandedWeapon];
        }

        Index idx = rank < 0 ? rank + 3 : rank + 2;
        ItemModifier modifier = typeItemModifiers[idx];

        modifier.Apply(clone);
        return clone;
    }
}
