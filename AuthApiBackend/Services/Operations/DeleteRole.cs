using AuthApiBackend.Interfaces.IOperations;
using AuthApiBackend.Interfaces.IServices;
using Serilog.Context;

namespace AuthApiBackend.Services.Operations
{
    public class DeleteRole : IDeleteRole
    {

        private readonly IRoleService roleService;
        private readonly ILogger<DeleteRole> logger;

        public DeleteRole(IRoleService roleService, ILogger<DeleteRole> logger)
        {
            this.roleService = roleService;
            this.logger = logger;
        }
        public async Task Delete(string roleName, CancellationToken cancellationToken)
        {
            using(LogContext.PushProperty("Operation", nameof(DeleteRole)))
            {
                var roleId = await roleService.GetRoleId(roleName, cancellationToken);
                var role = await roleService.GetRole(roleId, cancellationToken);

                await roleService.DeleteRole(role, cancellationToken);

                logger.LogInformation("Deleted Role: {RoleName} with Id: {Id}",roleName, roleId);
            }
        }
    }
}
