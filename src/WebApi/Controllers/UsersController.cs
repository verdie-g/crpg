using System;
using AspNet.Security.OpenId.Steam;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Trpg.WebApi.Controllers
{
    [ApiController]
    public class UsersController : BaseController
    {
        [HttpGet("signIn")]
        [AllowAnonymous] // TODO: HttpPost
        public IActionResult SignIn()
        {
            return Challenge(new AuthenticationProperties {RedirectUri = "/api/users/callback"},
                SteamAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpGet("callback")]
        public IActionResult AuthenticationCallback()
        {
            var authorization = Request.Headers[HeaderNames.Authorization][0];
            return Ok(authorization.Substring(7)); // Remove Bearer prefix
        }

        [HttpGet("self")]
        public IActionResult GetSelfUser()
        {
            return Ok(string.Join(Environment.NewLine, HttpContext.User.Claims));
        }
    }
}
