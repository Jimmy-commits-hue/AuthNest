using AuthApiBackend.DTOs;
using AuthApiBackend.DTOs.ResponseDtos;

namespace AuthApiBackend.Interfaces.IServices
{

    public interface IUserService 
    {

        Task<string> CreateUserAsync(RegisterDto user, CancellationToken cancellationToken);

        Task<UserResponse> GetUserIdAsync(string idNumber, CancellationToken cancellationToken);

        Task FindUserLoginNumberById(string nationalId, CancellationToken cancellationToken);

    }

}
