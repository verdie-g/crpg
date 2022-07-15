using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Common.Interfaces;

public interface ICurrentUserService
{
    public UserClaims? User { get; }
}

public class UserClaims
{
    public UserClaims(int id, Role role)
    {
        Id = id;
        Role = role;
    }

    public int Id { get; }
    public Role Role { get; }
}
