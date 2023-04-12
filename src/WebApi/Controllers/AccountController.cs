using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Route("[controller]")]
public class AccountController : ControllerBase
{
    [HttpGet("login")]
    public IActionResult Login([FromQuery] string returnUrl)
    {
        return new ChallengeResult("Steam", new AuthenticationProperties
        {
            RedirectUri = returnUrl,
        });
    }
}
