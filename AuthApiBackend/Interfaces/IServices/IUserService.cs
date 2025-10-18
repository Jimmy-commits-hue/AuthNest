using AuthApiBackend.DTOs;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IServices
{
    public interface IUserService 
    {

        Task<string> CreateUserAsync(RegisterDto user, CancellationToken cancellationToken);

        Task<UserResponse> GetUserIdAsync(string idNumber, CancellationToken cancellationToken);


    }
}
