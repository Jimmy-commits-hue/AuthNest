using AuthApiBackend.DTOs;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace AuthApiBackend.Interfaces.IRepositories
{

    public interface IUserRepository
    {

        Task CreateAsync(User user, CancellationToken cancellationToken);

        Task<UserResponse?> GetAsync(string idNumber, CancellationToken cancellationToken);

        Task UpdateAsync(User user, CancellationToken cancellationToken);

        Task DeleteAsync(User user, CancellationToken cancellationToken);

        Task<ForgottenLoginNumber?> GetUserId(string nationalId, CancellationToken cancellationToken);

        Task PatchUserDetails(string id, JsonPatchDocument<UserPatchDetails> patchDetails, UserPatchDetails user,
            CancellationToken cancellationToken);

        Task<string?> GetUserPkById(string nationalId, CancellationToken cancellationToken);

        Task<bool> GetUser(string idNumber, CancellationToken cancellationToken);

        Task DeleteUser(User user, CancellationToken cancellationToken);

    }

}
