using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IRepositories
{

    public interface IUserRepository
    {

        Task CreateAsync(User user, CancellationToken cancellationToken);

        Task<UserResponse?> GetAsync(string idNumber, CancellationToken cancellationToken);

        Task UpdateAsync(User user, CancellationToken cancellationToken);

        Task DeleteAsync(User user, CancellationToken cancellationToken);

    }

}
