using System.Security.Claims;
using System.Text.Json;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities.Users;
using Microsoft.IdentityModel.Tokens;

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
            if (authorizationHeader != null)
            {
                string decodedJwtPayload = Base64UrlEncoder.Decode(authorizationHeader.Split('.')[1]);
                string httpUser = JsonSerializer.Serialize(claimsPrincipal);
                Logger.Log(LogLevel.Warning, "User id or role in request was null. HTTP context user: {0}. JWT payload: {1} ({2})", httpUser, decodedJwtPayload, httpContext.Request.Path);
            }

            return;
        }

        int id = int.Parse(idStr);
        Role role = Enum.Parse<Role>(roleStr);
        User = new UserClaims(id, role);
    }

    public UserClaims? User { get; }
}
