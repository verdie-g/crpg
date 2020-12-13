using System.Collections.Generic;
using Crpg.Domain.Common;

namespace Crpg.Domain.Entities.Items
{
    /// <summary>
    /// cRPG representation of an item. Very similar to the representation of an item in Bannerlord.
    /// </summary>
    public class Item : AuditableEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// Id of the Mount & Blade item that will be used as a template. All other fields in the class <see cref="Item"/>
        /// will override the corresponding ones in the template.
        /// </summary>
        public string TemplateMbId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Id of the item of rank 0 (not broken nor loomed).
        /// </summary>
        public int? BaseItemId { get; set; }

        public ItemType Type { get; set; }
        public int Value { get; set; }
        public float Weight { get; set; }

        /// <summary>
        /// 0 by default. { -1, -2, -3 } for levels of broken. { 1, 2, 3 } for levels of looming.
        /// </summary>
        public int Rank { get; set; }

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

        public Item? BaseItem { get; set; }
        public IList<OwnedItem> OwnedItems { get; set; } = new List<OwnedItem>();

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
    }
}
