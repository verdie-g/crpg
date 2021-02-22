namespace Crpg.Application.Common.Results
{
    /// <summary>
    /// A machine-readable error code.
    /// </summary>
    public enum ErrorCode
    {
        CharacterLevelRequirementNotMet,
        CharacterNameAlreadyUsed,
        CharacterNotFound,
        ClanInvitationClosed,
        ClanInvitationNotFound,
        ClanMemberRoleNotMet,
        ClanNameAlreadyUsed,
        ClanNeedLeader,
        ClanNotFound,
        ClanTagAlreadyUsed,
        Conflict,
        InternalError,
        InvalidField,
        ItemAlreadyOwned,
        ItemBadSlot,
        ItemMaxRankReached,
        ItemNotBuyable,
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
        UserAlreadyInTheClan,
        UserNotAClanMember,
        UserNotFound,
        UserNotInAClan,
    }
}
