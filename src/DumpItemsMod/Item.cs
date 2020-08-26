using System.Collections.Generic;

namespace Crpg.DumpItemsMod
{
    /// <summary>
    /// Crpg item. Should be indentical to Crpg.Application.Items.Models.ItemCreation.
    /// This project cannot reference Crpg.Application because .NET framework assemblies
    /// cannot target .NET standard 2.1 ones and it's overkill to extract those models in
    /// their own assembly targetting .NET standard 2.0.
    /// </summary>
    internal class Item
    {
        public string MbId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }

        /// <summary>
        /// String representation of enum Crpg.Domain.Entities.ItemType.
        /// </summary>
        public string Type { get; set; } = default!;
        public float Weight { get; set; }

        public ItemArmorComponent? Armor { get; set; }
        public ItemHorseComponent? Horse { get; set; }
        public IList<ItemWeaponComponent>? Weapons { get; set; }
    }

    internal class ItemArmorComponent
    {
        public int HeadArmor { get; set; }
        public int BodyArmor { get; set; }
        public int ArmArmor { get; set; }
        public int LegArmor { get; set; }
    }

    internal class ItemHorseComponent
    {
        public int BodyLength { get; set; }
        public int ChargeDamage { get; set; }
        public int Maneuver { get; set; }
        public int Speed { get; set; }
        public int HitPoints { get; set; }
    }

    internal class ItemWeaponComponent
    {
        public string Class { get; set; } = default!;
        public int Accuracy { get; set; }
        public int MissileSpeed { get; set; }
        public int StackAmount { get; set; }
        public int Length { get; set; }
        public float Balance { get; set; }
        public int Handling { get; set; }
        public int BodyArmor { get; set; }
        public long Flags { get; set; }

        public int ThrustDamage { get; set; }

        /// <summary>
        /// String representation of enum Crpg.Domain.Entities.DamageType.
        /// </summary>
        public string ThrustDamageType { get; set; } = default!;
        public int ThrustSpeed { get; set; }

        public int SwingDamage { get; set; }
        public string SwingDamageType { get; set; } = default!;
        public int SwingSpeed { get; set; }
    }
}
