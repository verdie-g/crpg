﻿namespace Crpg.Application.Common.Results
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
        HeroInBattle,
        HeroNotFound,
        HeroNotInASettlement,
        HeroNotInSight,
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
        SettlementNotFound,
        SettlementTooFar,
        SkillRequirementNotMet,
        StatisticDecreased,
        UserAlreadyInAClan,
        UserAlreadyInTheClan,
        UserAlreadyRegisteredToStrategus,
        UserNotAClanMember,
        UserNotFound,
        UserNotInAClan,
    }
}
