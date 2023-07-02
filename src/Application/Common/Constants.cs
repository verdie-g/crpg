namespace Crpg.Application.Common;

// To synchronize with Crpg.Module.Common.CrpgConstants.
public class Constants
{
    public int WeaponProficiencyPointsForAgility { get; set; }
    public float[] WeaponProficiencyPointsForWeaponMasterCoefs { get; set; } = Array.Empty<float>();
    public float[] WeaponProficiencyPointsForLevelCoefs { get; set; } = Array.Empty<float>();
    public float[] WeaponProficiencyCostCoefs { get; set; } = Array.Empty<float>();
    public float DefaultExperienceMultiplier { get; set; }
    public float ExperienceMultiplierByGeneration { get; set; }
    public float MaxExperienceMultiplierForGeneration { get; set; }
    public int RespecializePriceForLevel30 { get; set; }
    public int FreeRespecializeIntervalDays { get; set; }
    public int MinimumRetirementLevel { get; set; }
    public float BaseExperienceGainPerSecond { get; set; }
    public float MultipliedExperienceGainPerSecond { get; set; }
    public float BaseGoldGainPerSecond { get; set; }
    public float MultipliedGoldGainPerSecond { get; set; }
    public float ItemRepairCostPerSecond { get; set; }
    public float ItemBreakChance { get; set; }
    public int BrokenItemRepairPenaltySeconds { get; set; }
    public float ItemSellCostPenalty { get; set; }
    public float[] ItemReforgeCostPerRank { get; set; } = Array.Empty<float>();
    public int MinimumLevel { get; set; }
    public int MaximumLevel { get; set; }
    public int TournamentLevel { get; set; }
    public float[] ExperienceForLevelCoefs { get; set; } = Array.Empty<float>();
    public int DefaultStrength { get; set; }
    public int DefaultAgility { get; set; }
    public int DefaultHealthPoints { get; set; }
    public int DefaultGeneration { get; set; }
    public int DefaultAttributePoints { get; set; }
    public int AttributePointsPerLevel { get; set; }
    public int DefaultSkillPoints { get; set; }
    public int SkillPointsPerLevel { get; set; }
    public int HealthPointsForStrength { get; set; }
    public int HealthPointsForIronFlesh { get; set; }
    public float DamageFactorForPowerStrike { get; set; }
    public float DamageFactorForPowerDraw { get; set; }
    public float DamageFactorForPowerThrow { get; set; }
    public float[] HandlingFactorForWeaponMaster { get; set; } = Array.Empty<float>();
    public float[] DurabilityFactorForShieldRecursiveCoefs { get; set; } = Array.Empty<float>();
    public float InfantryCoverageFactorForShieldCoef { get; set; }
    public float CavalryCoverageFactorForShieldCoef { get; set; }
    public float[] MountedRangedSkillInaccuracy { get; set; } = Array.Empty<float>();
    public float[] ShieldDefendStunMultiplierForSkillRecursiveCoefs { get; set; } = Array.Empty<float>();
    public float ArmorSetRequirementPowerMeanPValue { get; set; }
    public float DefaultRating { get; set; }
    public float DefaultRatingDeviation { get; set; }
    public float DefaultRatingVolatility { get; set; }
    public int DefaultGold { get; set; }
    public int DefaultHeirloomPoints { get; set; }
    public int ClanTagMinLength { get; set; }
    public int ClanTagMaxLength { get; set; }
    public string ClanTagRegex { get; set; } = string.Empty;
    public int ClanNameMinLength { get; set; }
    public int ClanNameMaxLength { get; set; }
    public int ClanDescriptionMinLength { get; set; }
    public int ClanDescriptionMaxLength { get; set; }
    public uint ClanColorMinValue { get; set; }
    public int ClanBannerKeyMaxLength { get; set; }
    public string ClanBannerKeyRegex { get; set; } = string.Empty;
    public double StrategusMapWidth { get; set; }
    public double StrategusMapHeight { get; set; }
    public double StrategusEquivalentDistance { get; set; }
    public double StrategusInteractionDistance { get; set; }
    public double StrategusViewDistance { get; set; }
    public double[] StrategusSpawningPositionCenter { get; set; } = Array.Empty<double>();
    public double StrategusSpawningPositionRadius { get; set; }
    public float StrategusTroopRecruitmentPerHour { get; set; }
    public int StrategusMinPartyTroops { get; set; }
    public int StrategusMaxPartyTroops { get; set; }
    public int StrategusBattleInitiationDurationHours { get; set; }
    public int StrategusBattleHiringDurationHours { get; set; }
    public int StrategusMercenaryMaxWage { get; set; }
    public int StrategusMercenaryNoteMaxLength { get; set; }
}
