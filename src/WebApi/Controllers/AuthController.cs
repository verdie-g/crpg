using AspNet.Security.OpenId.Steam;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Trpg.WebApi.Controllers
{
    public class AuthController : BaseController
    {
        [HttpGet("signIn")]
        [AllowAnonymous] // TODO: HttpPost
        public IActionResult SignIn()
        {
            string redirectUri = Url.Action(nameof(AuthenticationCallback), ControllerContext.ActionDescriptor.ControllerName, null);
            return Challenge(new AuthenticationProperties {RedirectUri = redirectUri}, SteamAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpGet("callback")]
        public IActionResult AuthenticationCallback()
        {
            var authorization = Request.Headers[HeaderNames.Authorization][0];
            return Ok(authorization.Substring(7)); // Remove Bearer prefix
        }
    }
}