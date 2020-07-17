using System;
using System.Collections.Generic;
using System.Linq;
using Crpg.Domain.Entities;

namespace Crpg.Application.Items.Models
{
    public class ItemCreation : ICloneable
    {
        public string MbId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public ItemType Type { get; set; }
        public float Weight { get; set; }
        public int Rank { get; set; }

        public ItemArmorComponentViewModel? Armor { get; set; }
        public ItemHorseComponentViewModel? Horse { get; set; }
        public IList<ItemWeaponComponentViewModel> Weapons { get; set; } = Array.Empty<ItemWeaponComponentViewModel>();

        public object Clone()
        {
            return new ItemCreation
            {
                MbId = MbId,
                Name = Name,
                Value = Value,
                Type = Type,
                Weight = Weight,
                Rank = Rank,
                Armor = (ItemArmorComponentViewModel?)Armor?.Clone(),
                Horse = (ItemHorseComponentViewModel?)Horse?.Clone(),
                Weapons = Weapons.Select(w => (ItemWeaponComponentViewModel)w.Clone()).ToArray(),
            };
        }
    }
}
