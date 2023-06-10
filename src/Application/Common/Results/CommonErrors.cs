using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Common.Results;

internal static class CommonErrors
{
    public static Error ApplicationClosed(int applicationId) => new(ErrorType.NotFound, ErrorCode.ApplicationClosed)
    {
        Title = "Application is closed",
        Detail = $"Application with id '{applicationId}' is closed",
    };

    public static Error ApplicationNotFound(int applicationId) => new(ErrorType.Validation, ErrorCode.ApplicationNotFound)
    {
        Title = "Application was not found",
        Detail = $"Application with id '{applicationId}' was not found",
    };

    public static Error BattleInvalidPhase(int battleId, BattlePhase phase) => new(ErrorType.Validation, ErrorCode.BattleInvalidPhase)
    {
        Title = "Cannot perform action during this battle phase",
        Detail = $"Cannot perform action when battle with id '{battleId}' is in phase '{phase}'",
    };

    public static Error BattleNotFound(int battleId) => new(ErrorType.NotFound, ErrorCode.BattleNotFound)
    {
        Title = "Battle was not found",
        Detail = $"Battle with id '{battleId}' was not found",
    };

    public static Error BattleTooFar(int battleId) => new(ErrorType.Validation, ErrorCode.BattleTooFar)
    {
        Title = "Battle is too far",
        Detail = $"Battle with id '{battleId}' is too far to perform the requested action",
    };

    public static Error CharacterForTournament(int characterId) => new(ErrorType.Validation, ErrorCode.CharacterForTournament)
    {
        Title = "Character is for tournament",
        Detail = $"Cannot perform this action on character with id '{characterId}' as it's only a character for tournaments",
    };

    public static Error CharacterForTournamentNotFound() => new(ErrorType.Validation, ErrorCode.CharacterForTournamentNotFound)
    {
        Title = "Character for tournament was not found",
        Detail = "The user has not set any character for tournament",
    };

    public static Error CharacterGenerationRequirement(int characterId, int userId, int req) => new(ErrorType.NotFound, ErrorCode.CharacterGenerationRequirement)
    {
        Title = "Character generation requirement not met",
        Detail = $"Character with id '{characterId}' for user with id '{userId}' should be of generation {req} to perform this action",
    };

    public static Error CharacterLevelRequirementNotMet(int requiredLevel, int actualLevel) => new(ErrorType.Validation, ErrorCode.CharacterLevelRequirementNotMet)
    {
        Title = "Unmet level requirement",
        Detail = $"Level {requiredLevel} is required but the character is {actualLevel}",
    };

    public static Error CharacterNotFound(int characterId, int userId) => new(ErrorType.NotFound, ErrorCode.CharacterNotFound)
    {
        Title = "Character was not found",
        Detail = $"Character with id '{characterId}' for user with id '{userId}' was not found",
    };

    public static Error CharacterRecentlyCreated(int userId) => new(ErrorType.Forbidden, ErrorCode.CharacterRecentlyCreated)
    {
        Title = "A character was already recently created",
        Detail = $"User {userId} created another character recently and can't create a new one after some time",
    };

    public static Error CharacteristicDecreased() => new(ErrorType.Validation, ErrorCode.CharacteristicDecreased)
    {
        Title = "A characteristic was decreased when it is not allowed",
    };

    public static Error ClanInvitationClosed(int clanInvitationId, ClanInvitationStatus status) => new(ErrorType.NotFound, ErrorCode.ClanInvitationClosed)
    {
        Title = "Clan invitation was closed",
        Detail = $"Clan invitation with id '{clanInvitationId}' has status '{status}'",
    };

    public static Error ClanInvitationNotFound(int clanInvitationId) => new(ErrorType.NotFound, ErrorCode.ClanInvitationNotFound)
    {
        Title = "Clan invitation was not found",
        Detail = $"Clan invitation with id '{clanInvitationId}' was not found",
    };

    public static Error ClanMemberRoleNotMet(int userId, ClanMemberRole expectedRole, ClanMemberRole actualRole) =>
        new(ErrorType.Forbidden, ErrorCode.ClanMemberRoleNotMet)
        {
            Title = "Unmet clan member role restriction",
            Detail = $"Role '{expectedRole}' was expected but member with id '{userId}' is '{actualRole}'",
        };

    public static Error ClanNameAlreadyUsed(string clanName) => new(ErrorType.Validation, ErrorCode.ClanNameAlreadyUsed)
    {
        Title = "Clan name is already used",
        Detail = $"Clan name '{clanName}' is already used",
    };

    public static Error ClanNeedLeader(int clanId) => new(ErrorType.Validation, ErrorCode.ClanNeedLeader)
    {
        Title = "A clan needs a leader",
        Detail = $"Clan with id '{clanId}' needs a leader",
    };

    public static Error ClanNotFound(int clanId) => new(ErrorType.NotFound, ErrorCode.ClanNotFound)
    {
        Title = "Clan was not found",
        Detail = $"Clan with id '{clanId}' was not found",
    };

    public static Error ClanTagAlreadyUsed(string clanTag) => new(ErrorType.Validation, ErrorCode.ClanTagAlreadyUsed)
    {
        Title = "Clan tag is already used",
        Detail = $"Clan tag '{clanTag}' is already used",
    };

    public static Error FighterNotACommander(int fighterId, int battleId) => new(ErrorType.Validation, ErrorCode.FighterNotACommander)
    {
        Title = "Fighter is not a commander in the battle",
        Detail = $"Fighter with id '{fighterId} is not a commander of the battle with id '{battleId}'",
    };

    public static Error PartyFighter(int partyId, int battleId) => new(ErrorType.Validation, ErrorCode.PartyFighter)
    {
        Title = "Party is a fighter in this battle",
        Detail = $"Cannot performed the requested action because the party with id '{partyId}' is a fighter in" +
                 $" the battle with id '{battleId}'",
    };

    public static Error PartyInBattle(int partyId) => new(ErrorType.Validation, ErrorCode.PartyInBattle)
    {
        Title = "Party is in a battle",
        Detail = $"Cannot performed the requested action while party with id '{partyId}' is in a battle",
    };

    public static Error PartyNotAFighter(int partyId, int battleId) => new(ErrorType.Validation, ErrorCode.PartyNotAFighter)
    {
        Title = "Party is not a fighter in the battle",
        Detail = $"Party with id '{partyId} is not a fighter of the battle with id '{battleId}'",
    };

    public static Error PartyNotEnoughTroops(int partyId) => new(ErrorType.Validation, ErrorCode.PartyNotEnoughTroops)
    {
        Title = "Party doesn't have enough troops",
        Detail = $"Party with id '{partyId} doesn't have enough troops",
    };

    public static Error PartyNotFound(int partyId) => new(ErrorType.NotFound, ErrorCode.PartyNotFound)
    {
        Title = "Party was not found",
        Detail = $"Party with id '{partyId}' was not found",
    };

    public static Error PartyNotInASettlement(int partyId) => new(ErrorType.Validation, ErrorCode.PartyNotInASettlement)
    {
        Title = "Party is not in a settlement",
        Detail = $"Party with id '{partyId}' is not in a settlement",
    };

    public static Error PartyNotInSight(int partyId) => new(ErrorType.Validation, ErrorCode.PartyNotInSight)
    {
        Title = "Party is not in sight",
        Detail = $"Party with id '{partyId}' is too far to be in sight",
    };

    public static Error PartyNotSettlementOwner(int partyId, int settlementId) => new(ErrorType.Forbidden, ErrorCode.PartyNotSettlementOwner)
    {
        Title = "Party is not the settlement owner",
        Detail = $"Party with id '{partyId}' is not of the owner of settlement with id '{settlementId}",
    };

    public static Error PartiesNotOnTheSameSide(int partyId1, int partyId2, int battleId) =>
        new(ErrorType.Validation, ErrorCode.PartiesNotOnTheSameSide)
        {
            Title = "Parties are not on the same side of the battle",
            Detail = $"Parties with ids '{partyId1}' and '{partyId2}' are not in the side in the battle with id '{battleId}'",
        };

    public static Error ItemAlreadyOwned(string itemId) => new(ErrorType.Validation, ErrorCode.ItemAlreadyOwned)
    {
        Title = "Item is already owned",
        Detail = $"Item with id '{itemId}' is already owned by the user",
    };

    public static Error ItemBadSlot(string itemId, ItemSlot slot) => new(ErrorType.Validation, ErrorCode.ItemBadSlot)
    {
        Title = "Item cannot be put in that slot",
        Detail = $"Item with id '{itemId}' cannot be put in the slot '{slot}'",
    };

    public static Error ItemBroken(string itemId) => new(ErrorType.Validation, ErrorCode.ItemBroken)
    {
        Title = "Item is broken",
        Detail = $"Item with id '{itemId}' is broken so the action cannot be performed",
    };

    public static Error ItemDisabled(string itemId) => new(ErrorType.Validation, ErrorCode.ItemDisabled)
    {
        Title = "Item is disabled",
        Detail = $"Item with id '{itemId}' is disabled so the action cannot be performed",
    };

    public static Error ItemNotBuyable(string itemId) => new(ErrorType.Validation, ErrorCode.ItemNotBuyable)
    {
        Title = "Item is not buyable",
        Detail = $"Item with id '{itemId}' is not buyable",
    };

    public static Error ItemIsNotSellable(string itemId) => new(ErrorType.Validation, ErrorCode.ItemNotSellable)
    {
        Title = "Item is not sellable",
        Detail = $"Item with id '{itemId}' is not selleable",
    };

    public static Error ItemNotFound(string itemId) => new(ErrorType.NotFound, ErrorCode.ItemNotFound)
    {
        Title = "Item was not found",
        Detail = $"Item with id '{itemId}' was not found",
    };

    public static Error ItemNotOwned(string itemId) => new(ErrorType.NotFound, ErrorCode.ItemNotOwned)
    {
        Title = "Item is not owned",
        Detail = $"Item with id '{itemId}' is not owned by the user",
    };

    public static Error NotEnoughAttributePoints(int requiredPoints, int actualPoints) => new(ErrorType.Validation, ErrorCode.NotEnoughAttributePoints)
    {
        Title = "Not enough attribute points",
        Detail = $"{requiredPoints} attribute points are required but only {actualPoints} are available",
    };

    public static Error NotEnoughGold(int requiredGold, int actualGold) => new(ErrorType.Validation, ErrorCode.NotEnoughGold)
    {
        Title = "Not enough gold",
        Detail = $"{requiredGold} gold is required but only {actualGold} is available",
    };

    public static Error NotEnoughHeirloomPoints(int requiredPoints, int actualPoints) => new(ErrorType.Validation, ErrorCode.NotEnoughHeirloomPoints)
    {
        Title = "Not enough heirloom points",
        Detail = $"{requiredPoints} points are required but only {actualPoints} are available",
    };

    public static Error NotEnoughSkillPoints(int requiredPoints, int actualPoints) => new(ErrorType.Validation, ErrorCode.NotEnoughSkillPoints)
    {
        Title = "Not enough skill points",
        Detail = $"{requiredPoints} skill points are required but only {actualPoints} are available",
    };

    public static Error NotEnoughWeaponProficiencyPoints(int requiredPoints, int actualPoints) => new(ErrorType.Validation, ErrorCode.NotEnoughWeaponProficiencyPoints)
    {
        Title = "Not enough weapon proficiency points",
        Detail = $"{requiredPoints} weapon proficiency points are required but only {actualPoints} are available",
    };

    public static Error SettlementNotFound(int settlementId) => new(ErrorType.NotFound, ErrorCode.SettlementNotFound)
    {
        Title = "Settlement was not found",
        Detail = $"Settlement with id '{settlementId}' was not found",
    };

    public static Error SettlementTooFar(int settlementId) => new(ErrorType.Validation, ErrorCode.SettlementTooFar)
    {
        Title = "Settlement is too far",
        Detail = $"Settlement with id '{settlementId}' is too far to perform the requested action",
    };

    public static Error SkillRequirementNotMet() => new(ErrorType.Validation, ErrorCode.SkillRequirementNotMet)
    {
        Title = "Unmet skill requirement",
    };

    public static Error UserAlreadyInAClan(int userId) => new(ErrorType.Validation, ErrorCode.UserAlreadyInAClan)
    {
        Title = "User is already in a clan",
        Detail = $"User with id '{userId}' is already in a clan",
    };

    public static Error UserAlreadyInTheClan(int userId, int clanId) => new(ErrorType.Validation, ErrorCode.UserAlreadyInTheClan)
    {
        Title = "User is already in the clan",
        Detail = $"User with id '{userId}' is already in the clan with id '{clanId}'",
    };

    public static Error UserAlreadyRegisteredToStrategus(int userId) => new(ErrorType.Validation, ErrorCode.UserAlreadyRegisteredToStrategus)
    {
        Title = "User has already registered to strategus",
        Detail = $"User with id '{userId}' has already registered to strategus",
    };

    public static Error UserItemIsNotBroken(int userItemId) =>
        new(ErrorType.Validation, ErrorCode.UserItemIsNotBroken)
        {
            Title = "User item is not broken",
            Detail = $"User item with id '{userItemId}' cannot be repaired as it is not broken",
        };

    public static Error UserItemMaxRankReached(int userItemId, int maxRank) =>
        new(ErrorType.Validation, ErrorCode.UserItemMaxRankReached)
        {
            Title = "User item has reached its max rank",
            Detail = $"User item with id '{userItemId}' has reached its max rank ({maxRank})",
        };

    public static Error UserItemNotFound(int userItemId) => new(ErrorType.NotFound, ErrorCode.UserItemNotFound)
    {
        Title = "User item was not found",
        Detail = $"User item with id '{userItemId}' was not found",
    };

    public static Error UserNotAClanMember(int userId, int clanId) => new(ErrorType.Forbidden, ErrorCode.UserNotAClanMember)
    {
        Title = "User is not a member of the clan",
        Detail = $"User with id '{userId}' is not a member of the clan with id '{clanId}'",
    };

    public static Error UserNotFound(int userId) => new(ErrorType.NotFound, ErrorCode.UserNotFound)
    {
        Title = "User was not found",
        Detail = $"User with id '{userId}' was not found",
    };

    public static Error UserNotFound(Platform platform, string platformUserId) => new(ErrorType.NotFound, ErrorCode.UserNotFound)
    {
        Title = "User was not found",
        Detail = $"User with '{platformUserId}' on platform '{platform}' was not found",
    };

    public static Error UserNotInAClan(int userId) => new(ErrorType.Forbidden, ErrorCode.UserNotInAClan)
    {
        Title = "User is not in a clan",
        Detail = $"User with id '{userId}' is not in a clan",
    };
}
