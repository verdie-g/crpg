namespace Crpg.Domain.Entities.Users;

/// <summary>
/// Role of a user. Used to restrict access to specific functionalities (e.g. bans).
/// </summary>
public enum Role
{
    User,
    Moderator,
    GameAdmin,
    Admin,
}
