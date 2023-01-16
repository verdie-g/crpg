using Crpg.Domain.Entities.ActivityLogs;

namespace Crpg.Application.Common.Services;

internal interface IActivityLogService
{
    ActivityLog CreateUserCreatedLog(int userId);
    ActivityLog CreateUserDeletedLog(int userId);
    ActivityLog CreateUserRenamedLog(int userId, string oldName, string newName);
    ActivityLog CreateItemBoughtLog(int userId, string itemId, int price);
    ActivityLog CreateItemSoldLog(int userId, string itemId, int price);
    ActivityLog CreateCharacterCreatedLog(int userId, int characterId);
    ActivityLog CreateCharacterDeletedLog(int userId, int characterId, int generation, int level);
    ActivityLog CreateCharacterRespecializedLog(int userId, int characterId);
    ActivityLog CreateCharacterRetiredLog(int userId, int characterId, int level);
}

internal class ActivityLogService : IActivityLogService
{
    public ActivityLog CreateUserCreatedLog(int userId)
    {
        return CreateLog(ActivityLogType.UserCreated, userId);
    }

    public ActivityLog CreateUserDeletedLog(int userId)
    {
        return CreateLog(ActivityLogType.UserDeleted, userId);
    }

    public ActivityLog CreateUserRenamedLog(int userId, string oldName, string newName)
    {
        return CreateLog(ActivityLogType.UserRenamed, userId, new ActivityLogMetadata[]
        {
            new("oldName", oldName),
            new("newName", newName),
        });
    }

    public ActivityLog CreateItemBoughtLog(int userId, string itemId, int price)
    {
        return CreateLog(ActivityLogType.ItemBought, userId, new ActivityLogMetadata[]
        {
            new("itemId", itemId),
            new("price", price.ToString()),
        });
    }

    public ActivityLog CreateItemSoldLog(int userId, string itemId, int price)
    {
        return CreateLog(ActivityLogType.ItemSold, userId, new ActivityLogMetadata[]
        {
            new("itemId", itemId),
            new("price", price.ToString()),
        });
    }

    public ActivityLog CreateCharacterCreatedLog(int userId, int characterId)
    {
        return CreateLog(ActivityLogType.CharacterCreated, userId, new ActivityLogMetadata[]
        {
            new("characterId", characterId.ToString()),
        });
    }

    public ActivityLog CreateCharacterDeletedLog(int userId, int characterId, int generation, int level)
    {
        return CreateLog(ActivityLogType.CharacterDeleted, userId, new ActivityLogMetadata[]
        {
            new("characterId", characterId.ToString()),
            new("generation", generation.ToString()),
            new("level", level.ToString()),
        });
    }

    public ActivityLog CreateCharacterRespecializedLog(int userId, int characterId)
    {
        return CreateLog(ActivityLogType.CharacterRespecialized, userId, new ActivityLogMetadata[]
        {
            new("characterId", characterId.ToString()),
        });
    }

    public ActivityLog CreateCharacterRetiredLog(int userId, int characterId, int level)
    {
        return CreateLog(ActivityLogType.CharacterRetired, userId, new ActivityLogMetadata[]
        {
            new("characterId", characterId.ToString()),
            new("level", level.ToString()),
        });
    }

    private ActivityLog CreateLog(ActivityLogType type, int userId, params ActivityLogMetadata[] metadata)
    {
        return new ActivityLog
        {
            Type = type,
            UserId = userId,
            Metadata = metadata.ToList(),
        };
    }
}
