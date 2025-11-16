using AuthApiBackend.DTOs;
using AuthApiBackend.Interfaces.IOperations;
using AuthApiBackend.Interfaces.IServices;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;


namespace AuthApiBackend.Controllers.V2
{
    [Authorize(Policy = "Admin")]
    [Route("api/v{version:apiversion}/[controller]")]
    [ApiController]
    [EnableRateLimiting("AfterLogin")]
    public class AdminController : ControllerBase
    {

        private readonly IRoleService roleService;
        private readonly IAntiforgery antiForgery;

        public AdminController(IRoleService roleService, IAntiforgery antiForgery)
        {
            this.roleService = roleService;
            this.antiForgery = antiForgery;
        }

        [HttpPost("register-role")]
        public async Task<IActionResult> RegisterRole([FromBody] RoleDto role, CancellationToken cancellationToken)
        {
            await antiForgery.ValidateRequestAsync(HttpContext);

            await roleService.CreateRoleAsync(role, cancellationToken);

            return Created();
        }

        [HttpDelete("Delete-role")]
        public async Task<IActionResult> DeleteRole([FromQuery] string roleName, IDeleteRole role, CancellationToken cancellationToken)
        {

            await antiForgery.ValidateRequestAsync(HttpContext);

            await role.Delete(roleName, cancellationToken);

            return Ok("Role deleted successfully");
        }

    }

}

