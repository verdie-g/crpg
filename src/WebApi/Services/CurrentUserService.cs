using System.Security.Claims;
using System.Text;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities.Users;

namespace Crpg.WebApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private static readonly ILogger Logger = Logging.LoggerFactory.CreateLogger<CurrentUserService>();

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        var claimsPrincipal = httpContext.User;
        string? idStr = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        string? roleStr = claimsPrincipal.FindFirstValue(ClaimTypes.Role);
        if (idStr == null || roleStr == null)
        {
            string? authorizationHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
            if (authorizationHeader == null)
            {
                Logger.Log(LogLevel.Warning, "Authorization header was null ({0})", httpContext.Request.Path);
            }
            else
            {
                string decodedJwtPayload = Encoding.UTF8.GetString(Convert.FromBase64String(authorizationHeader.Split('.')[1]));
                Logger.Log(LogLevel.Warning, "User id or role in request was null. JWT payload: {0} ({1})", decodedJwtPayload, httpContext.Request.Path);
            }

            return;
        }

        int id = int.Parse(idStr);
        Role role = Enum.Parse<Role>(roleStr);
        User = new UserClaims(id, role);
    }

    public UserClaims? User { get; }
}
