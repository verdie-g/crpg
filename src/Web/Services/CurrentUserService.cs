using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Trpg.Application.Common.Interfaces;

namespace Trpg.Web.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            string idStr = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            UserId = idStr == null ? null : (int?) int.Parse(idStr);
        }

        public int? UserId { get; }
    }
}