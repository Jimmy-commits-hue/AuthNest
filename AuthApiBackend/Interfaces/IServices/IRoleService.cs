using AuthApiBackend.DTOs;
using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IServices
{

    public interface IRoleService
    {

        Task CreateRoleAsync(RoleDto roleDto, CancellationToken cancellationToken);

        Task<int> GetRoleId(string role, CancellationToken cancellationToken);

        Task<Role> GetRole(int roleId, CancellationToken cancellationToken);

        Task DeleteRole(Role role, CancellationToken cancellationToken);

    }

}
