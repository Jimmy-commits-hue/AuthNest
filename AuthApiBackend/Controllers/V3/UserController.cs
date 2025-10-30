using AuthApiBackend.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Security.Claims;

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

        [HttpPatch]
        public async Task<IActionResult> UpdateDetails()
        {
            return Ok("Details Updated Successfully");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccount()
        {

            return Ok("Account deleted Successfully");
        }

        [HttpPatch]
        public async Task<IActionResult> DeactivateAccount()
        {

            return Ok("Account deactivated successfully");
        }

        [HttpPatch]
        public async Task<ActionResult> ActivateAccount()
        {

            return Ok("Welcome back");
        }
    }
}
