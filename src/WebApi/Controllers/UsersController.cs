using System;
using AspNet.Security.OpenId.Steam;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Trpg.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    public class UsersController : BaseController
    {
        [AllowAnonymous]
        [HttpGet("signIn")] // TODO: HttpPost
        public IActionResult SignIn()
        {
            return Challenge(new AuthenticationProperties {RedirectUri = "/api/users/callback"},
                SteamAuthenticationDefaults.AuthenticationScheme);
        }

        [AllowAnonymous]
        [HttpGet("callback")]
        public IActionResult AuthenticationCallback()
        {
            return Ok(Request.Headers[HeaderNames.Authorization][0]);
        }

        [HttpGet("self")]
        public IActionResult GetSelfUser()
        {
            return Ok(string.Join(Environment.NewLine, HttpContext.User.Claims));
        }
    }
}
