using System.Security.Claims;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities.Users;
using OpenIddict.Abstractions;

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
        string? idStr = claimsPrincipal.FindFirstValue(OpenIddictConstants.Claims.Subject);
        string? roleStr = claimsPrincipal.FindFirstValue(OpenIddictConstants.Claims.Role);
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
