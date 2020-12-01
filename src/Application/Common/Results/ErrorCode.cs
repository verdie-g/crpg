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
        ItemBadSlot,
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
