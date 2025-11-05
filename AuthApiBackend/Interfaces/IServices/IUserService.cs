using AuthApiBackend.DTOs;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace AuthApiBackend.Interfaces.IServices
{

    public interface IUserService 
    {

        Task<string> CreateUserAsync(RegisterDto user, CancellationToken cancellationToken);

        Task<UserResponse> GetUserIdAsync(string idNumber, CancellationToken cancellationToken);

        Task FindUserLoginNumberById(string nationalId, CancellationToken cancellationToken);

        Task UpdateUserPartially(string Id, JsonPatchDocument<UserPatchDetails> userPatch, UserPatchDetails patch,
            CancellationToken cancellationToken);

        Task<string> GetUserPkById(string userId, CancellationToken cancellationToken);

        Task DeleteUser(User user, CancellationToken cancellationToken);

    }

}
