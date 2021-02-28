using System;

namespace Crpg.GameMod.Common
{
    // Copy of Crpg.Application.Common.Constants
    internal class CrpgConstants
    {
        public float[] WeaponProficiencyPointsForAgilityCoefs { get; set; } = Array.Empty<float>();
        public float[] WeaponProficiencyPointsForWeaponMasterCoefs { get; set; } = Array.Empty<float>();
        public float[] WeaponProficiencyPointsForLevelCoefs { get; set; } = Array.Empty<float>();
        public float[] WeaponProficiencyCostCoefs { get; set; } = Array.Empty<float>();
        public float DefaultExperienceMultiplier { get; set; }
        public float[] ExperienceMultiplierForGenerationCoefs { get; set; } = Array.Empty<float>();
        public float[] RespecializeExperiencePenaltyCoefs { get; set; } = Array.Empty<float>();
        public int MinimumRetirementLevel { get; set; }
        public float[] ItemRepairCostCoefs { get; set; } = Array.Empty<float>();
        public float ItemBreakChance { get; set; }
        public float[] ItemSellCostCoefs { get; set; } = Array.Empty<float>();
        public int MinimumLevel { get; set; }
        public int MaximumLevel { get; set; }
        public float[] ExperienceForLevelCoefs { get; set; } = Array.Empty<float>();
        public int DefaultStrength { get; set; }
        public int DefaultAgility { get; set; }
        public int DefaultHealthPoints { get; set; }
        public int DefaultGeneration { get; set; }
        public bool DefaultAutoRepair { get; set; }
        public int AttributePointsPerLevel { get; set; }
        public int SkillPointsPerLevel { get; set; }
        public float[] HealthPointsForStrengthCoefs { get; set; } = Array.Empty<float>();
        public float[] HealthPointsForIronFleshCoefs { get; set; } = Array.Empty<float>();
        public float[] DamageFactorForPowerStrikeCoefs { get; set; } = Array.Empty<float>();
        public float[] DamageFactorForPowerDrawCoefs { get; set; } = Array.Empty<float>();
        public float[] DamageFactorForPowerThrowCoefs { get; set; } = Array.Empty<float>();
        public float[] DurabilityFactorForShieldCoefs { get; set; } = Array.Empty<float>();
        public float[] SpeedFactorForShieldCoefs { get; set; } = Array.Empty<float>();
        public string DefaultRole { get; set; } = string.Empty;
        public int DefaultGold { get; set; }
        public int DefaultHeirloomPoints { get; set; }
        public int StrategusMapWidth { get; set; }
        public int StrategusMapHeight { get; set; }
    }
}
