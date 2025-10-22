using AuthApiBackend.DTOs;
using AuthApiBackend.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;


namespace AuthApiBackend.Controllers.V2
{

    [Route("api/v{version:apiversion}[controller]")]
    [ApiController]
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

    }

}

