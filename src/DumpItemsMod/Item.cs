using System;
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
        /// Integer value of Crpg.Domain.Entities.DamageType.
        /// </summary>
        public int Type { get; set; }
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
        public int Accuracy { get; set; }
        public int MissileSpeed { get; set; }
        public int StackAmount { get; set; }
        public int Length { get; set; }
        public int Handling { get; set; }
        public int BodyArmor { get; set; }
        public ulong Flags { get; set; }

        public int ThrustDamage { get; set; }
        public int ThrustDamageType { get; set; }
        public int ThrustSpeed { get; set; }

        public int SwingDamage { get; set; }
        public int SwingDamageType { get; set; }
        public int SwingSpeed { get; set; }
    }
}
