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

    [Collection("Env collection")]
    public class UnitTest1
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

        [Fact]
        public async Task FindUserLoginNumberById_throwsException_IfUserDoesNotExist()
        {
            string nationalId = "0130534589867";

            userRepo.Setup(c => c.GetUserId(HashHelper.HashId(nationalId), CancellationToken.None)).
                ReturnsAsync((ForgottenLoginNumber?)null);

            var ex = await Assert.ThrowsAsync<Exception>(async () =>
                     await userService.FindUserLoginNumberById(nationalId, CancellationToken.None));

            Assert.Equal("An email has been sent to ******@gmail.com", ex.Message);

            userRepo.Verify(c => c.GetUserId(It.IsAny<string>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task FindUserLoginNumberById_ReturnsUserLoginNumberAndEmail_IfUserExist()
        {

            string nationalId = "0130534589867";

            userRepo.Setup(c => c.GetUserId(HashHelper.HashId(nationalId), CancellationToken.None)).ReturnsAsync(
                new ForgottenLoginNumber(
                    UserEmail: "jimmyjabulani01@gmail.com",
                    LoginNumber: "250000001"
                    ));

            await userService.FindUserLoginNumberById(nationalId, CancellationToken.None);

            userRepo.Verify(c => c.GetUserId(It.IsAny<string>(), CancellationToken.None), Times.Once);
        }
    }
}