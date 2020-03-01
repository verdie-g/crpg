using System.Security.Claims;
using Crpg.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Crpg.Web.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            string idStr = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            UserId = idStr == null ? null : (int?)int.Parse(idStr);
        }

        public int? UserId { get; }
    }
}