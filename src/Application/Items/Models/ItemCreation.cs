using System;
using System.Collections.Generic;
using Crpg.Domain.Entities;

namespace Crpg.Application.Items.Models
{
    public class ItemCreation
    {
        public string MbId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public ItemType Type { get; set; }
        public float Weight { get; set; }

        public ItemArmorComponentViewModel? Armor { get; set; }
        public ItemHorseComponentViewModel? Horse { get; set; }
        public IList<ItemWeaponComponentViewModel> Weapons { get; set; } = Array.Empty<ItemWeaponComponentViewModel>();
    }
}