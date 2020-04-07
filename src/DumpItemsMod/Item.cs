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
        /// Integer value of Crpg.Domain.Entities.ItemType.
        /// </summary>
        public int Type { get; set; }
        public float Weight { get; set; }

        public int? HeadArmor { get; set; }
        public int? BodyArmor { get; set; }
        public int? ArmArmor { get; set; }
        public int? LegArmor { get; set; }

        public int? BodyLength { get; set; }
        public int? ChargeDamage { get; set; }
        public int? Maneuver { get; set; }
        public int? Speed { get; set; }
        public int? HitPoints { get; set; }

        /// <summary>
        /// Integer value of Crpg.Domain.Entities.DamageType.
        /// </summary>
        public int? ThrustDamageType { get; set; }
        public int? SwingDamageType { get; set; }
        public int? Accuracy { get; set; }
        public int? MissileSpeed { get; set; }
        public int? StackAmount { get; set; }
        public int? WeaponLength { get; set; }

        public int? PrimaryThrustDamage { get; set; }
        public int? PrimaryThrustSpeed { get; set; }
        public int? PrimarySwingDamage { get; set; }
        public int? PrimarySwingSpeed { get; set; }
        public int? PrimaryHandling { get; set; }

        /// <summary>
        /// Integer value of Crpg.Domain.Entities.WeaponFlags.
        /// </summary>
        public ulong? PrimaryWeaponFlags { get; set; }

        public int? SecondaryThrustDamage { get; set; }
        public int? SecondaryThrustSpeed { get; set; }
        public int? SecondarySwingDamage { get; set; }
        public int? SecondarySwingSpeed { get; set; }
        public int? SecondaryHandling { get; set; }
        public ulong? SecondaryWeaponFlags { get; set; }
    }
}
