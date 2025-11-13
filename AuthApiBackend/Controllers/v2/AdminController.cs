using AuthApiBackend.DTOs;
using AuthApiBackend.Interfaces.IOperations;
using AuthApiBackend.Interfaces.IServices;
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

        public AdminController(IRoleService roleService)
        {
            this.roleService = roleService;
        }

        [HttpPost("register-role")]
        public async Task<IActionResult> RegisterRole([FromBody] RoleDto role, CancellationToken cancellationToken)
        {
            await roleService.CreateRoleAsync(role, cancellationToken);

            return Created();
        }

        [HttpDelete("Delete-role")]
        public async Task<IActionResult> DeleteRole([FromQuery] string roleName, IDeleteRole role, CancellationToken cancellationToken)
        {
            await role.Delete(roleName, cancellationToken);

            return Ok("Role deleted successfully");
        }

    }

}

