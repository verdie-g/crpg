using System.Threading.Tasks;
using Crpg.Application.Users.Models;
using IdentityServer4.AspNetIdentity;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Crpg.WebApi.Identity
{
    internal class CustomProfileService : ProfileService<UserViewModel>
    {
        public CustomProfileService(UserManager<UserViewModel> userManager, IUserClaimsPrincipalFactory<UserViewModel> claimsFactory,
            ILogger<ProfileService<UserViewModel>> logger)
            : base(userManager, claimsFactory, logger)
        {
        }

        // This method is called for authorization. The default ProfileService checks if the user exists but this
        // is not needed.
        public override Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.CompletedTask;
        }
    }
}
