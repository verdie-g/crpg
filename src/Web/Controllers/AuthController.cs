using AspNet.Security.OpenId.Steam;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.Web.Controllers
{
    public class AuthController : BaseController
    {
        /// <summary>
        /// Signs in through Steam.
        /// </summary>
        /// <returns></returns>
        [HttpGet("signIn"), AllowAnonymous] // TODO: HttpPost
        public IActionResult SignIn([FromQuery] string redirectUri)
        {
            return Challenge(new AuthenticationProperties {RedirectUri = redirectUri}, SteamAuthenticationDefaults.AuthenticationScheme);
        }
    }
}