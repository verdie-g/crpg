namespace Crpg.Application.Common.Results
{
    /// <summary>
    /// A machine-readable error code.
    /// </summary>
    public enum ErrorCode
    {
        CharacterLevelRequirementNotMet,
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
        UserNotAClanMember,
        UserNotFound,
        UserNotInAClan,
    }
}
