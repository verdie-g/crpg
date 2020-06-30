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

        /// <summary>
        /// Id of the item of rank 0 (not broken nor loomed).
        /// </summary>
        public int BaseItemId { get; set; }

        public ItemType Type { get; set; }
        public int Value { get; set; }
        public float Weight { get; set; }

        /// <summary>
        /// 0 by default. { -1, -2, -3 } for levels of broken. { 1, 2, 3 } for levels of looming.
        /// </summary>
        public int Rank { get; set; }

        public ItemArmorComponent? Armor { get; set; }
        public ItemHorseComponent? Horse { get; set; }
        public ItemWeaponComponent? PrimaryWeapon { get; set; }
        public ItemWeaponComponent? SecondaryWeapon { get; set; }
        public ItemWeaponComponent? TertiaryWeapon { get; set; }

        public Item? BaseItem { get; set; }
        public IList<UserItem> UserItems { get; set; } = new List<UserItem>();

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