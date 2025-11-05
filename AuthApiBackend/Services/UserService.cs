using AuthApiBackend.DTOs;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Exceptions.ExceptionTypes;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Models;
using AuthApiBackend.Utilities;
using Microsoft.AspNetCore.JsonPatch;

namespace AuthApiBackend.Services
{

    public class UserService(IUserRepository userRepo) : IUserService
    {

        public async Task<string> CreateUserAsync(RegisterDto user, CancellationToken cancellationToken)
        {
            var IdNumber = HashHelper.HashId(user.IdNumber);

            var userExist = await userRepo.GetAsync(IdNumber, cancellationToken);

            if (userExist is not null)
                throw new UserAlreadyExistException("User already exist");

            var newUser = new User
            {
                IdNumber = IdNumber,
                FirstName = user.FirstName.ToString().Split(' ')[0],
                Surname = user.Surname,
            };

            await userRepo.CreateAsync(newUser, cancellationToken);

            return newUser.Id;
        }

        public async Task<UserResponse> GetUserIdAsync(string idNumber, CancellationToken cancellationToken)
        {
            var userInfo = await userRepo.GetAsync(HashHelper.HashId(idNumber), cancellationToken)
                           ?? throw new UserNotFoundException("User does not exist");

            return userInfo;
        }

        public async Task FindUserLoginNumberById(string nationalId, CancellationToken cancellationToken)
        {
            ForgottenLoginNumber data = await userRepo.GetUserId(HashHelper.HashId(nationalId), cancellationToken) ??
                                            throw new Exception("An email has been sent to ******@gmail.com");

            SendStudentNumberToClient.resendForgettedLoginNumber.Enqueue(data);
        }

        public async Task UpdateUserPartially(string Id, JsonPatchDocument<UserPatchDetails> userPatch, UserPatchDetails patch, 
            CancellationToken cancellationToken)
        {
            await userRepo.PatchUserDetails(Id, userPatch, patch, cancellationToken);
        }

        public async Task<string> GetUserPkById(string userId,  CancellationToken cancellationToken)
        {
            return await userRepo.GetUserPkById(HashHelper.HashId(userId), cancellationToken) ?? 
                     throw new NoAccountMatchException("No account associated with the user");
        }

        public async Task DeleteUser(User user, CancellationToken cancellationToken)
        {
           await userRepo.DeleteUser(user, cancellationToken);   
        }

    }

}