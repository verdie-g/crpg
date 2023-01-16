namespace Crpg.Domain.Entities.ActivityLogs;

public enum ActivityLogType
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
