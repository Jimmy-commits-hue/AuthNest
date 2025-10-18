using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IRepositories
{

    public interface IUserRoleRepository
    {

        Task CreateAsync(UserRole userRole, CancellationToken cancellationToken);

        Task UpdateAsync(UserRole userRole, CancellationToken cancellationToken);

        Task<int?> GetAsync(string userId, CancellationToken cancellationToken);

        //Task<UserRole> GetAsync(int id, CancellationToken cancellationToken);
        
    }

}
