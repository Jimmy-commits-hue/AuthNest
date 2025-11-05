using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IRepositories
{

    public interface IRoleRepository
    {

        Task CreateAsync(Role role, CancellationToken cancellationToken);

        Task<int?> GetAsync(string role, CancellationToken cancellationToken);

        Task<Role> GetRole(int roleRoleId, CancellationToken cancellationToken);

        Task DeleteAsync(Role role, CancellationToken cancellationToken);
    }
}
