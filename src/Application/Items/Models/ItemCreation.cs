using System;
using System.Collections.Generic;
using System.Linq;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models
{
    public class ItemCreation : ICloneable
    {
        public string TemplateMbId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public ItemType Type { get; set; }
        public float Weight { get; set; }
        public int Rank { get; set; }

        public ItemArmorComponentViewModel? Armor { get; set; }
        public ItemMountComponentViewModel? Mount { get; set; }
        public IList<ItemWeaponComponentViewModel> Weapons { get; set; } = Array.Empty<ItemWeaponComponentViewModel>();

        public object Clone()
        {
            return new ItemCreation
            {
                TemplateMbId = TemplateMbId,
                Name = Name,
                Value = Value,
                Type = Type,
                Weight = Weight,
                Rank = Rank,
                Armor = (ItemArmorComponentViewModel?)Armor?.Clone(),
                Mount = (ItemMountComponentViewModel?)Mount?.Clone(),
                Weapons = Weapons.Select(w => (ItemWeaponComponentViewModel)w.Clone()).ToArray(),
            };
        }
    }
}
