using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Common.Results
{
    public static class CommonErrors
    {
        public static Error CharacterLevelRequirementNotMet(int requiredLevel, int actualLevel) => new Error(ErrorType.Validation, ErrorCode.CharacterLevelRequirementNotMet)
        {
            Title = "Unmet level requirement",
            Detail = $"Level {requiredLevel} is required but the character is {actualLevel}",
        };

        public static Error CharacterNameAlreadyUsed(string characterName) => new Error(ErrorType.Validation, ErrorCode.CharacterNameAlreadyUsed)
        {
            Title = "Character name is already used",
            Detail = $"Character name '{characterName}' is already used",
        };

        public static Error CharacterNotFound(int characterId, int userId) => new Error(ErrorType.NotFound, ErrorCode.CharacterNotFound)
        {
            Title = "Character was not found",
            Detail = $"Character with id '{characterId}' for user with id '{userId}' was not found",
        };

        public static Error ClanInvitationClosed(int clanInvitationId, ClanInvitationStatus status) => new Error(ErrorType.NotFound, ErrorCode.ClanInvitationClosed)
        {
            Title = "Clan invitation was closed",
            Detail = $"Clan invitation with id '{clanInvitationId}' has status '{status}'",
        };

        public static Error ClanInvitationNotFound(int clanInvitationId) => new Error(ErrorType.NotFound, ErrorCode.ClanInvitationNotFound)
        {
            Title = "Clan invitation was not found",
            Detail = $"Clan invitation with id '{clanInvitationId}' was not found",
        };

        public static Error ClanMemberRoleNotMet(int userId, ClanMemberRole expectedRole, ClanMemberRole actualRole) =>
            new Error(ErrorType.Forbidden, ErrorCode.ClanMemberRoleNotMet)
            {
                Title = "Unmet clan member role restriction",
                Detail = $"Role '{expectedRole}' was expected but member with id '{userId}' is '{actualRole}'",
            };

        public static Error ClanNameAlreadyUsed(string clanName) => new Error(ErrorType.Validation, ErrorCode.ClanNameAlreadyUsed)
        {
            Title = "Clan name is already used",
            Detail = $"Clan name '{clanName}' is already used",
        };

        public static Error ClanNeedLeader(int clanId) => new Error(ErrorType.Validation, ErrorCode.ClanNeedLeader)
        {
            Title = "A clan needs a leader",
            Detail = $"Clan with id '{clanId}' needs a leader",
        };

        public static Error ClanNotFound(int clanId) => new Error(ErrorType.NotFound, ErrorCode.ClanNotFound)
        {
            Title = "Clan was not found",
            Detail = $"Clan with id '{clanId}' was not found",
        };

        public static Error ClanTagAlreadyUsed(string clanTag) => new Error(ErrorType.Validation, ErrorCode.ClanTagAlreadyUsed)
        {
            Title = "Clan tag is already used",
            Detail = $"Clan tag '{clanTag}' is already used",
        };

        public static Error ItemAlreadyOwned(int itemId) => new Error(ErrorType.Validation, ErrorCode.ItemAlreadyOwned)
        {
            Title = "Item is already owned",
            Detail = $"Item with id '{itemId}' is already owned by the user",
        };

        public static Error ItemBadSlot(int itemId, ItemSlot slot) => new Error(ErrorType.Validation, ErrorCode.ItemBadSlot)
        {
            Title = "Item cannot be put in that slot",
            Detail = $"Item with id '{itemId}' cannot be put in the slot '{slot}'",
        };

        public static Error ItemMaxRankReached(int itemId, int userId, int maxRank) =>
            new Error(ErrorType.Validation, ErrorCode.ItemMaxRankReached)
        {
            Title = "User item has reached its max rank",
            Detail = $"Item with id '{itemId}' owned by user with id '{userId}' has reached its max rank ({maxRank})",
        };

        public static Error ItemNotBuyable(int itemId) => new Error(ErrorType.Validation, ErrorCode.ItemNotBuyable)
        {
            Title = "Item is not buyable",
            Detail = $"Item with id '{itemId}' is not buyable",
        };

        public static Error ItemNotFound(int itemId) => new Error(ErrorType.NotFound, ErrorCode.ItemNotFound)
        {
            Title = "Item was not found",
            Detail = $"Item with id '{itemId}' was not found",
        };

        public static Error ItemNotOwned(int itemId) => new Error(ErrorType.NotFound, ErrorCode.ItemNotOwned)
        {
            Title = "Item is not owned",
            Detail = $"Item with id '{itemId}' is not owned by the user",
        };

        public static Error NotEnoughAttributePoints(int requiredPoints, int actualPoints) => new Error(ErrorType.Validation, ErrorCode.NotEnoughAttributePoints)
        {
            Title = "Not enough attribute points",
            Detail = $"{requiredPoints} attribute points are required but only {actualPoints} are available",
        };

        public static Error NotEnoughGold(int requiredGold, int actualGold) => new Error(ErrorType.Validation, ErrorCode.NotEnoughGold)
        {
            Title = "Not enough gold",
            Detail = $"{requiredGold} gold is required but only {actualGold} is available",
        };

        public static Error NotEnoughHeirloomPoints(int requiredPoints, int actualPoints) => new Error(ErrorType.Validation, ErrorCode.NotEnoughHeirloomPoints)
        {
            Title = "Not enough heirloom points",
            Detail = $"{requiredPoints} points are required but only {actualPoints} are available",
        };

        public static Error NotEnoughSkillPoints(int requiredPoints, int actualPoints) => new Error(ErrorType.Validation, ErrorCode.NotEnoughSkillPoints)
        {
            Title = "Not enough skill points",
            Detail = $"{requiredPoints} skill points are required but only {actualPoints} are available",
        };

        public static Error NotEnoughWeaponProficiencyPoints(int requiredPoints, int actualPoints) => new Error(ErrorType.Validation, ErrorCode.NotEnoughWeaponProficiencyPoints)
        {
            Title = "Not enough weapon proficiency points",
            Detail = $"{requiredPoints} weapon proficiency points are required but only {actualPoints} are available",
        };

        public static Error SettlementNotFound(int settlementId) => new Error(ErrorType.NotFound, ErrorCode.SettlementNotFound)
        {
            Title = "Settlement was not found",
            Detail = $"Settlement with id '{settlementId}' was not found",
        };

        public static Error SkillRequirementNotMet() => new Error(ErrorType.Validation, ErrorCode.SkillRequirementNotMet)
        {
            Title = "Unmet skill requirement",
        };

        public static Error StatisticDecreased() => new Error(ErrorType.Validation, ErrorCode.StatisticDecreased)
        {
            Title = "A statistic was decreased when it is not allowed",
        };

        public static Error UserAlreadyInAClan(int userId) => new Error(ErrorType.Validation, ErrorCode.UserAlreadyInAClan)
        {
            Title = "User is already in a clan",
            Detail = $"User with id '{userId}' is already in a clan",
        };

        public static Error UserAlreadyInTheClan(int userId, int clanId) => new Error(ErrorType.Validation, ErrorCode.UserAlreadyInTheClan)
        {
            Title = "User is already in the clan",
            Detail = $"User with id '{userId}' is already in the clan with id '{clanId}'",
        };

        public static Error UserInBattle(int userId) => new Error(ErrorType.Validation, ErrorCode.UserInBattle)
        {
            Title = "User is in a battle",
            Detail = $"Cannot performed the requested action while user with id '{userId}' is in a battle",
        };

        public static Error UserAlreadyRegisteredToStrategus(int userId) => new Error(ErrorType.Validation, ErrorCode.UserAlreadyRegisteredToStrategus)
        {
            Title = "User has already registered to strategus",
            Detail = $"User with id '{userId}' has already registered to strategus",
        };

        public static Error UserNotAClanMember(int userId, int clanId) => new Error(ErrorType.Forbidden, ErrorCode.UserNotAClanMember)
        {
            Title = "User is not a member of the clan",
            Detail = $"User with id '{userId}' is not a member of the clan with id '{clanId}'",
        };

        public static Error UserNotFound(int userId) => new Error(ErrorType.NotFound, ErrorCode.UserNotFound)
        {
            Title = "User was not found",
            Detail = $"User with id '{userId}' was not found",
        };

        public static Error UserNotInAClan(int userId) => new Error(ErrorType.Forbidden, ErrorCode.UserNotInAClan)
        {
            Title = "User is not in a clan",
            Detail = $"User with id '{userId}' is not in a clan",
        };

        public static Error UserNotRegisteredToStrategus(int userId) => new Error(ErrorType.Validation, ErrorCode.UserNotRegisteredToStrategus)
        {
            Title = "User has not registered to strategus",
            Detail = $"User with id '{userId}' has not registered to strategus",
        };

        public static Error UserNotVisible(int userId) => new Error(ErrorType.Validation, ErrorCode.UserNotVisible)
        {
            Title = "User is not visible",
            Detail = $"User with id '{userId}' is too far to be visible",
        };
    }
}
