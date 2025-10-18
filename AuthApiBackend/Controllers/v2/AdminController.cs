using AuthApiBackend.DTOs;
using AuthApiBackend.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;

namespace AuthApiBackend.Controllers.v2
{
    public class AdminController : Controller
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

            return Ok();
        }
    }
}
