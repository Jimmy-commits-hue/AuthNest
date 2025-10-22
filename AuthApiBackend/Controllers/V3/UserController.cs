using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace AuthApiBackend.Controllers.V3
{
    [Route("api/v{version:apiversion}/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        [HttpGet("welcome")]
        public IActionResult UserDashboard()
        {
            return Ok("Welcome");
        }
    }
}
