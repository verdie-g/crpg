using System.Security.Claims;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities.Users;

namespace Crpg.WebApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private static readonly ILogger Logger = Logging.LoggerFactory.CreateLogger<CurrentUserService>();

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext == null)
        {
            Logger.Log(LogLevel.Warning, $"{nameof(IHttpContextAccessor)}.{nameof(IHttpContextAccessor.HttpContext)} returned null: {Environment.StackTrace}");
            return;
        }

        var claimsPrincipal = httpContextAccessor.HttpContext.User;
        string? idStr = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        string? roleStr = claimsPrincipal.FindFirstValue(ClaimTypes.Role);
        if (idStr == null || roleStr == null)
        {
            Logger.Log(LogLevel.Warning, "User id or role in request was null");
            return;
        }

        int id = int.Parse(idStr);
        Role role = Enum.Parse<Role>(roleStr);
        User = new UserClaims(id, role);
    }

    public UserClaims? User { get; }
}
