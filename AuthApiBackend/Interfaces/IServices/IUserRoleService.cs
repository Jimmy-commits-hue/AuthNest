using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IServices
{
    public interface IUserRoleService
    {

        Task CreateUserRoleAsync(int roleId, string userId, CancellationToken cancellationToken);

    }
}
