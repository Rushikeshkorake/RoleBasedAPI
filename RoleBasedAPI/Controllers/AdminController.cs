using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RoleBasedAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AdminController : ControllerBase
    {

        [Authorize(Policy = "AdminPolicy")]
        [HttpGet("admin-endpoint")]
        public IActionResult Get()
        {
            return Ok("You Have Accessed Admin");
        }
    }
}
