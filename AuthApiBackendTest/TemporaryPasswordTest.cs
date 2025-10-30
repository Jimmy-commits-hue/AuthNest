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
    public class TemporaryPasswordTest
    {

        private readonly Mock<ITemporaryPasswordRepo> passwordRepo;
        private readonly TemporaryPasswordService passwordService;

        public TemporaryPasswordTest()
        {
            passwordRepo = new Mock<ITemporaryPasswordRepo>();
            passwordService = new TemporaryPasswordService(passwordRepo.Object);
        }

        [Fact]
        public async Task CreateTemporaryPassword_CreatePassword()
        {
            
            var tempPassword = new TemporaryPassword
            {
                AccountId = Guid.NewGuid().ToString(),
                HashedPassword = EncryptData.Encrypt(GenerateCode.TemporaryPassword()),
                IsActive = true
            };

            passwordRepo.Setup(account => account.CreatePassword(tempPassword, CancellationToken.None)).
                Returns(Task.CompletedTask);

            await passwordService.CreateTemporaryPassword(tempPassword.AccountId, CancellationToken.None);

            passwordRepo.Verify(verify => verify.CreatePassword(It.IsAny<TemporaryPassword>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task VerifyPassword_ShouldReturnAccountId_IfPasswordValid()
        {

            passwordRepo.Setup(tempPassword => tempPassword.GetPassword(It.IsAny<string>(), CancellationToken.None)).
                ReturnsAsync(new PasswordResetResponse(
                    password: EncryptData.Encrypt("#JaBu@5."),
                    accountId: Guid.NewGuid().ToString()
                    ));

            var accountId = await passwordService.VerifyPassword(It.IsAny<string>(), "#JaBu@5.", CancellationToken.None);

            Assert.NotNull(accountId);
            Assert.IsType<string>(accountId);

            passwordRepo.Verify(c => c.GetPassword(It.IsAny<string>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task VerifyPassword_ShouldThrowException_IfInvalid()
        {
            passwordRepo.Setup(tempPassword => tempPassword.GetPassword(It.IsAny<string>(), CancellationToken.None)).
                ReturnsAsync(new PasswordResetResponse(
                    password: EncryptData.Encrypt("#JaBu@5."),
                    accountId: Guid.NewGuid().ToString()
                    ));

            var ex = await Assert.ThrowsAsync<InvalidTempPassword>(async () => await passwordService
                          .VerifyPassword(It.IsAny<string>(), "#JaBu@4.", CancellationToken.None));

            Assert.Equal("Incorrect temp password", ex.Message);

            passwordRepo.Verify(a => a.GetPassword(It.IsAny<string>(), CancellationToken.None), Times.Once);

        }

        [Fact]
        public async Task UpdatePasswordStatus_WillUpdatePasswordStatus()
        {
            string accountId = Guid.NewGuid().ToString();

            passwordRepo.Setup(c => c.UpdateStatus(accountId, CancellationToken.None)).Returns(Task.CompletedTask);

            await passwordService.UpdatePasswordStatus(accountId, CancellationToken.None);

            passwordRepo.Verify(p => p.UpdateStatus(It.IsAny<string>(), CancellationToken.None), Times.Once);   
        }

        [Fact]
        public async Task GetAllPendingPasswords_ReturnsPendingPasswords_IfAny()
        {

             passwordRepo.Setup(c => c.GetAllPendingPasswords(CancellationToken.None)).ReturnsAsync(
                new List<ResetPasswordResponse> {
                 new ResetPasswordResponse("jimmyjabulani01@gmail.com",
                EncryptData.Encrypt("#JaBu@5."),
                Guid.NewGuid().ToString())});

            var response = await passwordService.GetAllPendingPasswords(CancellationToken.None);

            Assert.IsType<List<ResetPasswordResponse>>(response);

            passwordRepo.Verify(c => c.GetAllPendingPasswords(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetAllPendingPassword_ReturnsNull_IfNoPendingPasswords()
        {

            passwordRepo.Setup(c => c.GetAllPendingPasswords(CancellationToken.None)).ReturnsAsync(
                (List<ResetPasswordResponse>?)null);

            var tempPassword = await passwordService.GetAllPendingPasswords(CancellationToken.None);

            Assert.Null(tempPassword);

            passwordRepo.Verify(c => c.GetAllPendingPasswords(CancellationToken.None), Times.Once);
        }
    }
}
