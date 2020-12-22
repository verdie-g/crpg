using System;
using System.Collections.Generic;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models
{
    public class ItemCreation
    {
        public string TemplateMbId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ItemType Type { get; set; }
        public float Weight { get; set; }
        public int Rank { get; set; }

        public ItemArmorComponentViewModel? Armor { get; set; }
        public ItemMountComponentViewModel? Mount { get; set; }
        public IList<ItemWeaponComponentViewModel> Weapons { get; set; } = Array.Empty<ItemWeaponComponentViewModel>();
    }
}
