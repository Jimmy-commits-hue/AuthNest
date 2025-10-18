using AuthApiBackend.DTOs;
using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IServices
{

    public interface IRoleService
    {

        Task CreateRoleAsync(RoleDto roleDto, CancellationToken cancellationToken);
        
        Task UpdateRoleAsync(string role, string newRole, CancellationToken cancellationToken);

        Task<int> GetRoleAsync(string role, CancellationToken cancellationToken);

       // Task<IEnumerable<Role>> GetAllRolesAsync(CancellationToken cancellationToken);
        
       // Task DeleteRoleAsync(string role, CancellationToken cancellationToken);

    }

}
