using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWTExemplo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        [Route("get")]
        public IActionResult Get()
        {
            return Ok(new { message = "Hey i'm only for authorized users" });
        }

        [HttpGet]
        [Authorize]
        [Route("get-cliams")]
        public IActionResult GetUserClaims()
        {
            List<string> userClaims = new List<String>();
            foreach(var claim in HttpContext.User.Claims)
            {
                userClaims.Add(claim.Value);
            }

            return Ok(userClaims);
        }
    }
}
