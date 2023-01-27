using System.Security.Claims;
using System.Text.Json;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities.Users;
using IdentityServer4.Extensions;

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
                string claimsStr = JsonSerializer.Serialize(claimsPrincipal.Claims.Select(c => $"{c.Type}: {c.Value}"));
                Logger.Log(LogLevel.Warning,
                    "User id ({0}) or role ({1}) in request was null. IsAuthenticated: {2}. AutenticationType: {3}. Claims: {4}. JWT: {5}. Path: {6}",
                    idStr, roleStr, claimsPrincipal.IsAuthenticated(), claimsPrincipal.Identity?.AuthenticationType,
                    claimsStr, authorizationHeader, httpContext.Request.Path);
            }

            return;
        }

        int id = int.Parse(idStr);
        Role role = Enum.Parse<Role>(roleStr);
        User = new UserClaims(id, role);
    }

    public UserClaims? User { get; }
}
