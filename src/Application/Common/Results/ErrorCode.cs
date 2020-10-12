namespace Crpg.Application.Common.Results
{
    /// <summary>
    /// A machine-readable error code.
    /// </summary>
    public enum ErrorCode
    {
        CharacterLevelRequirementNotMet,
        CharacterNotFound,
        Conflict,
        InternalError,
        InvalidField,
        ItemAlreadyOwned,
        ItemBadType,
        ItemMaxRankReached,
        ItemNotFound,
        ItemNotOwned,
        NotEnoughAttributePoints,
        NotEnoughGold,
        NotEnoughHeirloomPoints,
        NotEnoughSkillPoints,
        NotWeaponProficiencyPoints,
        SkillRequirementNotMet,
        StatisticDecreased,
        UserNotFound,
    }
}
