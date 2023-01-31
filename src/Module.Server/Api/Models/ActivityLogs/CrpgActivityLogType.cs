namespace Crpg.Module.Api.Models.ActivityLogs;

internal enum CrpgActivityLogType
{
    UserCreated,
    UserDeleted,
    UserRenamed,
    ItemBought,
    ItemSold,
    CharacterCreated,
    CharacterDeleted,
    CharacterRespecialized,
    CharacterRetired,
    ServerJoined,
    ChatMessageSent,
    TeamHit,
}
