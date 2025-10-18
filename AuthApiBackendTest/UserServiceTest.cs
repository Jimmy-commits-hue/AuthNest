using Moq;
using AuthApiBackend.Services;
using AuthApiBackend.Models;
using AuthApiBackend.Utilities;
using AuthApiBackend.DTOs;
using AuthApiBackend.Exceptions.ExceptionTypes;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.DTOs.ResponseDtos;

namespace AuthApiBackendTest
{
    public class EnvFixture
    {
        public EnvFixture()
        {
            DotNetEnv.Env.Load(@"C:\Users\jabul\source\repos\AuthNest\AuthApiBackend\.env");
        }
    }

    public class UnitTest1 : IClassFixture<EnvFixture>
    {

        private readonly Mock<IUserRepository> userRepo;
        private readonly UserService userService;

        public UnitTest1()
        {
            userRepo = new Mock<IUserRepository>();
            userService = new UserService(userRepo.Object) { };
        }

        [Fact]
        public async Task CreateUserAsync_ReturnUserId_IfUserCreated()
        {
            string idNumber = "1234567891012";
            string hashedIdNumber = HashHelper.HashId(idNumber);

            var newUser = new User
            {
                IdNumber = hashedIdNumber,
                FirstName = "Jimmy",
                Surname = "Khabana",
            };

            userRepo.Setup(userRepo => userRepo.CreateAsync(newUser, CancellationToken.None));

            var registerUser = new RegisterDto
            {
                IdNumber = "1234567891012",
                FirstName = "Jimmy",
                Surname = "Khabana",
            };

            var result = await userService.CreateUserAsync(registerUser, CancellationToken.None);

            Assert.NotNull(result);
            Assert.IsType<string>(result);
            Assert.Equal(hashedIdNumber, HashHelper.HashId(registerUser.IdNumber));

            userRepo.Verify(userRepo => userRepo.CreateAsync(It.IsAny<User>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_ThrowsUserAlreadyExistException_IfUserExist()
        {
            string idNumber = "1234567891012";
            string hashedIdNumber = HashHelper.HashId(idNumber);

            userRepo.Setup(userRepo => userRepo.GetAsync(hashedIdNumber, CancellationToken.None))
                    .ReturnsAsync(new UserResponse
                    {
                        UserId = Guid.NewGuid().ToString(),
                    });

            var registerUser = new RegisterDto
            {
                IdNumber = "1234567891012",
                FirstName = "Jimmy",
                Surname = "Khabana",
            };

            await Assert.ThrowsAsync<UserAlreadyExistException>(() =>
                  userService.CreateUserAsync(registerUser, CancellationToken.None));

            userRepo.Verify(userRepo => userRepo.GetAsync(hashedIdNumber, CancellationToken.None), Times.Once);
            userRepo.Verify(userRepo => userRepo.CreateAsync(It.IsAny<User>(), CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task GetUserAsync_ReturnUserId_IfUserExist()
        {
            string idNumber = "1234567891012";

            userRepo.Setup(userRepo => userRepo.GetAsync(HashHelper.HashId(idNumber), CancellationToken.None))
                    .ReturnsAsync(new UserResponse { UserId = Guid.NewGuid().ToString() });

            var result = await userService.GetUserIdAsync(idNumber, CancellationToken.None);

            Assert.NotNull(result);
            Assert.IsType<UserResponse>(result);

            userRepo.Verify(userRepo => userRepo.GetAsync(HashHelper.HashId(idNumber), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetUserIdAsync_ThrowsUserNotFoundException_IfUserDoesNotExist()
        {
            string idNumber = "1234567891012";
            userRepo.Setup(userRepo => userRepo.GetAsync(HashHelper.HashId(idNumber), CancellationToken.None))
                    .ReturnsAsync((UserResponse?)null);

            await Assert.ThrowsAsync<UserNotFoundException>(() =>
                  userService.GetUserIdAsync(idNumber, CancellationToken.None));

            userRepo.Verify(userRepo => userRepo.GetAsync(HashHelper.HashId(idNumber), CancellationToken.None), Times.Once);
        }

        
    }
}