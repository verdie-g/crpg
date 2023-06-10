using Crpg.Domain.Common;

namespace Crpg.Domain.Entities.Items;

/// <summary>
/// cRPG representation of an item. Very similar to the representation of an item in Bannerlord.
/// </summary>
public class Item : AuditableEntity, ICloneable
{
    /// <summary>
    /// Same id as the Mount & Blade item.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    public string BaseId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Culture Culture { get; set; }
    public ItemType Type { get; set; }
    public int Price { get; set; }
    public float Tier { get; set; }
    public int Rank { get; set; }
    public int Requirement { get; set; }
    public float Weight { get; set; }
    public ItemFlags Flags { get; set; }

    /// <summary>
    /// True if the items can be played with. It can be false for example if the item is bugged or if it's an
    /// event-only item.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Armor component of an item. If not null, the item is an armor.
    /// </summary>
    public ItemArmorComponent? Armor { get; set; }

    /// <summary>
    /// Mount component of an item. If not null, the item is a mount.
    /// </summary>
    public ItemMountComponent? Mount { get; set; }

    /// <summary>
    /// Represents the first mode of a weapon. An item can have several modes for example a polearm that has a one
    /// handed and two handed mode. If not null the item is a weapon.
    /// </summary>
    public ItemWeaponComponent? PrimaryWeapon { get; set; }

    /// <summary>
    /// Represents the second mode a weapon. Can be null if the item is a weapon but has only one mode.
    /// </summary>
    public ItemWeaponComponent? SecondaryWeapon { get; set; }

    /// <summary>
    /// Represents the third mode a weapon. Can be null if the item is a weapon but has only one or modes.
    /// </summary>
    public ItemWeaponComponent? TertiaryWeapon { get; set; }

    public List<UserItem> UserItems { get; set; } = new();

    public IEnumerable<ItemWeaponComponent> GetWeapons()
    {
        if (PrimaryWeapon != null)
        {
            yield return PrimaryWeapon;
        }

        if (SecondaryWeapon != null)
        {
            yield return SecondaryWeapon;
        }

        if (TertiaryWeapon != null)
        {
            yield return TertiaryWeapon;
        }
    }

    /// <summary>
    /// Clone <see cref="Item"/> without relations.
    /// </summary>
    public object Clone() => new Item
    {
        Id = Id,
        BaseId = BaseId,
        Name = Name,
        Culture = Culture,
        Type = Type,
        Tier = Tier,
        Rank = Rank,
        Requirement = Requirement,
        Price = Price,
        Weight = Weight,
        Flags = Flags,
        Enabled = Enabled,
        Armor = (ItemArmorComponent?)Armor?.Clone(),
        Mount = (ItemMountComponent?)Mount?.Clone(),
        PrimaryWeapon = (ItemWeaponComponent?)PrimaryWeapon?.Clone(),
        SecondaryWeapon = (ItemWeaponComponent?)SecondaryWeapon?.Clone(),
        TertiaryWeapon = (ItemWeaponComponent?)TertiaryWeapon?.Clone(),
    };
}
