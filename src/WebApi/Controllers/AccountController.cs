using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers
{
    [AllowAnonymous]
    public class AccountController : BaseController
    {
        private readonly IIdentityServerInteractionService _interaction;

        public AccountController(IIdentityServerInteractionService interaction)
        {
            _interaction = interaction;
        }

        [HttpGet("login")]
        public IActionResult Login([FromQuery] string returnUrl)
        {
            if (!_interaction.IsValidReturnUrl(returnUrl))
            {
                return BadRequest();
            }

            const string provider = "Steam";
            AuthenticationProperties properties = new()
            {
                RedirectUri = returnUrl,
                Items = { [provider] = provider },
            };
            return new ChallengeResult(provider, properties);
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout([FromQuery] string logoutId)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            var logoutContext = await _interaction.GetLogoutContextAsync(logoutId);
            return Redirect(logoutContext.PostLogoutRedirectUri); // PostLogoutRedirectUri configured by client in IdentityServerConfig
        }
    }
}
