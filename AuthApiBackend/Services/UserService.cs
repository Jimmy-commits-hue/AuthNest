using AuthApiBackend.DTOs;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Exceptions.ExceptionTypes;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Models;
using AuthApiBackend.Utilities;

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
                    FirstName = user.FirstName,
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
    }
}
