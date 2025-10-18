using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IRepositories
{

    public interface IRoleRepository
    {

        Task CreateAsync(Role role, CancellationToken cancellationToken);

        Task UpdateAsync(Role role, CancellationToken cancellationToken);

       Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken);

        Task<int?> GetAsync(string role, CancellationToken cancellationToken);

        Task DeleteAsync(Role role, CancellationToken cancellationToken);
    }
}
