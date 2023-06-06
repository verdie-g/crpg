﻿namespace Crpg.Domain.Entities.ActivityLogs;

public enum ActivityLogType
{
    UserCreated,
    UserDeleted,
    UserRenamed,
    UserRewarded,
    ItemBought,
    ItemSold,
    ItemBroke,
    ItemRepaired,
    ItemUpgraded,
    CharacterCreated,
    CharacterDeleted,
    CharacterRespecialized,
    CharacterRetired,
    CharacterRewarded,
    ServerJoined,
    ChatMessageSent,
    TeamHit,
}
