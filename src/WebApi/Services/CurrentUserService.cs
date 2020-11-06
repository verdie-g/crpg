using System.Security.Claims;
using Crpg.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Crpg.WebApi.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            string? idStr = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            UserId = idStr == null ? -1 : int.Parse(idStr);
        }

        public int UserId { get; }
    }
}
