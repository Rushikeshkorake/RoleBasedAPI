using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RoleBasedAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViewerController : ControllerBase
    {
        //[Authorize(Roles = "Viewer")]
        [Authorize(Policy = "ViewerPolicy")]

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("You Have Accessed viewer");
        }
    }
}
