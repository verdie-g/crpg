using System;
using System.Collections.Generic;
using Crpg.Domain.Common;

namespace Crpg.Domain.Entities
{
    public class Item : AuditableEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// Item id in Mount and Blade.
        /// </summary>
        public string MbId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public ItemType Type { get; set; }
        public int Value { get; set; }
        public float Weight { get; set; }
        // TODO: Looming

        public ItemArmorComponent? Armor { get; set; }
        public ItemHorseComponent? Horse { get; set; }
        public ItemWeaponComponent? PrimaryWeapon { get; set; }
        public ItemWeaponComponent? SecondaryWeapon { get; set; }
        public ItemWeaponComponent? TertiaryWeapon { get; set; }

        public List<UserItem> UserItems { get; set; } = new List<UserItem>();

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