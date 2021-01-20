namespace Crpg.Application.Common.Results
{
    /// <summary>
    /// A machine-readable error code.
    /// </summary>
    public enum ErrorCode
    {
        CharacterLevelRequirementNotMet,
        CharacterNotFound,
        ClanNameAlreadyUsed,
        ClanNotFound,
        ClanTagAlreadyUsed,
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
        NotEnoughWeaponProficiencyPoints,
        SkillRequirementNotMet,
        StatisticDecreased,
        UserAlreadyInAClan,
        UserNotFound,
    }
}
