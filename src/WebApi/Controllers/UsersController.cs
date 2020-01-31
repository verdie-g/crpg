using System;
using Microsoft.AspNetCore.Mvc;

namespace Trpg.WebApi.Controllers
{
    [ApiController]
    public class UsersController : BaseController
    {
        [HttpGet("self")]
        public IActionResult GetSelfUser()
        {
            return Ok(string.Join(Environment.NewLine, HttpContext.User.Claims));
        }
    }
}
