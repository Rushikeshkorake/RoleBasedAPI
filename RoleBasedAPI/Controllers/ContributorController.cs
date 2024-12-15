using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RoleBasedAPI.Controllers
{
    [Authorize(Roles = "Contributor,Admin")]

    [Route("api/[controller]")]
    [ApiController]
    public class ContributorController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("You Have Accessed Contributor or admin");
        }
    }
}
