using System.Security.Claims;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities.Users;

namespace Crpg.WebApi.Services;

public class CurrentUserService : ICurrentUserService
{
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        string? idStr = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        string? roleStr = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
        if (idStr == null || roleStr == null)
        {
            return;
        }

        int id = int.Parse(idStr);
        Role role = Enum.Parse<Role>(roleStr);
        User = new UserClaims(id, role);
    }

    public UserClaims? User { get; }
}
