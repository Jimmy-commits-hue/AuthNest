using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Exceptions.ExceptionTypes;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Models;
using AuthApiBackend.Services;
using AuthApiBackend.Utilities;
using Microsoft.Extensions.Options;
using Moq;


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
            passwordService = new TemporaryPasswordService(passwordRepo.Object, Options.Create(
                             new AuthApiBackend.Configurations.MaxAttemptsConfig
                             {
                                 Max = "3"
                             }));
        }

        [Fact]
        public async Task CreateTemporaryPassword_CreatePassword()
        {
            var tempPassword = new TemporaryPassword
            {
                AccountId = Guid.NewGuid().ToString(),
                HashedPassword = EncryptData.Encrypt(GenerateCode.TemporaryPassword()),
                IsActive = true,
                AttemptCount = 1
            };

            var fakeDb = new List<TemporaryPassword>();

            passwordRepo.Setup(account => account.CreatePassword(It.IsAny<TemporaryPassword>(), It.IsAny<CancellationToken>())).
                         Callback<TemporaryPassword, CancellationToken>((tempPassword, cancellationToken) =>
                         {
                             fakeDb.Add(tempPassword);
                         }).
                         Returns(Task.CompletedTask);

            await passwordService.CreateTemporaryPassword(tempPassword.AccountId, tempPassword.AttemptCount, CancellationToken.None);

            Assert.Single(fakeDb);

            passwordRepo.Verify(verify => verify.CreatePassword(It.IsAny<TemporaryPassword>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyPassword_ShouldReturnAccountId_IfPasswordValid()
        {
            passwordRepo.Setup(tempPassword => tempPassword.GetPassword(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                         ReturnsAsync(new PasswordResetResponse(
                                          password: EncryptData.Encrypt("#JaBu@5."),
                                          accountId: Guid.NewGuid().ToString()
                                      ));

            var accountId = await passwordService.VerifyPassword(Guid.NewGuid().ToString(), "#JaBu@5.", CancellationToken.None);

            Assert.NotNull(accountId);
            Assert.IsType<string>(accountId);

            passwordRepo.Verify(c => c.GetPassword(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyPassword_ShouldThrowException_IfInvalid()
        {
            passwordRepo.Setup(tempPassword => tempPassword.GetPassword(It.IsAny<string>(), CancellationToken.None)).
                         ReturnsAsync(new PasswordResetResponse(
                                          password: EncryptData.Encrypt("#JaBu@5."),
                                          accountId: Guid.NewGuid().ToString()
                                      ));

            var ex = await Assert.ThrowsAsync<InvalidTempPassword>(async () => 
                     await passwordService.VerifyPassword(Guid.NewGuid().ToString(), "#JaBu@4.", CancellationToken.None));

            Assert.Equal("Incorrect temp password", ex.Message);

            passwordRepo.Verify(a => a.GetPassword(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

        }

        [Fact]
        public async Task UpdatePasswordStatus_WillUpdatePasswordStatus()
        {
            string accountId = Guid.NewGuid().ToString();
            bool passwordStatus = true;

            passwordRepo.Setup(c => c.UpdateStatus(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                         Callback<string, CancellationToken>((Id, cancellationToken) =>
                         {
                             passwordStatus = false;
                         }).
                         Returns(Task.CompletedTask);

            await passwordService.UpdatePasswordStatus(accountId, CancellationToken.None);

            Assert.False(passwordStatus);

            passwordRepo.Verify(p => p.UpdateStatus(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);   
        }

        [Fact]
        public async Task GetAllPendingPasswords_ReturnsPendingPasswords_IfAny()
        {
             passwordRepo.Setup(c => c.GetAllPendingPasswords(It.IsAny<CancellationToken>())).
                          ReturnsAsync(
                                     [
                                        new(
                                             "jimmyjabulani01@gmail.com",
                                              EncryptData.Encrypt("#JaBu@5."),
                                              Guid.NewGuid().ToString()
                                            )
                                      ]);

            var response = await passwordService.GetAllPendingPasswords(CancellationToken.None);

            #pragma warning disable CS8604
            Assert.IsType<ResetPasswordResponse>(response.First());
            #pragma warning restore CS8604

            passwordRepo.Verify(c => c.GetAllPendingPasswords(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAllPendingPassword_ReturnsNull_IfNoPendingPasswords()
        {
            passwordRepo.Setup(c => c.GetAllPendingPasswords(It.IsAny<CancellationToken>())).
                         ReturnsAsync((List<ResetPasswordResponse>?)null);

            var tempPassword = await passwordService.GetAllPendingPasswords(CancellationToken.None);

            Assert.Null(tempPassword);

            passwordRepo.Verify(c => c.GetAllPendingPasswords(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CheckAttemptNumber_ReturnsOne_IfAttemptIsZero()
        {
            passwordRepo.Setup(c => c.GetAttemptCount(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                         ReturnsAsync(0);

            var attemptCount = await passwordService.CheckAttemptNumber(Guid.NewGuid().ToString(), CancellationToken.None);

            Assert.Equal(1, attemptCount);

            passwordRepo.Verify(c => c.GetAttemptCount(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            passwordRepo.Verify(c => c.DeactivateOldCode(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            passwordRepo.Verify(c => c.GetTempCodeId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CheckAttemptNumber_DeactivateOldCode_IfAttemptIsMoreThanZero()
        {
            passwordRepo.Setup(c => c.GetAttemptCount(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                         ReturnsAsync(1);

            passwordRepo.Setup(c => c.GetTempCodeId(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                         ReturnsAsync(Guid.NewGuid().ToString());

            passwordRepo.Setup(c => c.DeactivateOldCode(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                         Returns(Task.CompletedTask);

            var attemptCount = await passwordService.CheckAttemptNumber(Guid.NewGuid().ToString(), CancellationToken.None);

            Assert.Equal(2, attemptCount);

            passwordRepo.Verify(c => c.GetAttemptCount(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            passwordRepo.Verify(c => c.DeactivateOldCode(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            passwordRepo.Verify(c => c.GetTempCodeId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CheckAttemptNumber_ThrowsException_IfAttemptIsMoreThanThree()
        {
            passwordRepo.Setup(c => c.GetAttemptCount(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                         ReturnsAsync(4);

            var ex = await Assert.ThrowsAnyAsync<DailyMaximumAttemptsReachedException>(async () => 
                     await passwordService.CheckAttemptNumber(Guid.NewGuid().ToString(), CancellationToken.None));

            Assert.Equal("Daily permitted maximum request attempts reached", ex.Message);

            passwordRepo.Verify(c => c.GetAttemptCount(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            passwordRepo.Verify(c => c.DeactivateOldCode(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            passwordRepo.Verify(c => c.GetTempCodeId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RetrieveExpiredCodes_ReturnsExpiredCodes_IfAny()
        {
            passwordRepo.Setup(c => c.GetExpiredCodes(It.IsAny<CancellationToken>())).
                         ReturnsAsync(
                                     [
                                        new(){
                                               AccountId = Guid.NewGuid().ToString(),
                                             }
                                     ]);

            var expiredCodes = await passwordService.RetrieveExpiredCodes(CancellationToken.None);

            #pragma warning disable CS8604
            Assert.True(expiredCodes.Any());
            #pragma warning restore CS8604

            passwordRepo.Verify(c => c.GetExpiredCodes(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RetrieveExpiredCodes_ReturnsNull_IfNone()
        {
            passwordRepo.Setup(c => c.GetExpiredCodes(It.IsAny<CancellationToken>())).
                         ReturnsAsync((List<TemporaryPassword>?)null);

            var expiredCodes = await passwordService.RetrieveExpiredCodes(CancellationToken.None);

            Assert.Null(expiredCodes);

            passwordRepo.Verify(c => c.GetExpiredCodes(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RetrieveUsedCodes_ReturnsUsedCode_IfAny()
        {
            passwordRepo.Setup(c => c.GetUsedCodes(It.IsAny<CancellationToken>())).
                         ReturnsAsync(
                                     [
                                        new()
                                            {
                                              AccountId = Guid.NewGuid().ToString(),
                                            }
                                     ]);

            var usedCodes = await passwordService.RetrieveUsedCodes(CancellationToken.None);

            #pragma warning disable CS8604
            Assert.IsType<TemporaryPassword>(usedCodes.First());
            #pragma warning restore CS8604

            passwordRepo.Verify(c => c.GetUsedCodes(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RetrieveUsedCodes_ReturnsNull_IfNone()
        {
            passwordRepo.Setup(c => c.GetUsedCodes(It.IsAny<CancellationToken>())).
                         ReturnsAsync((List<TemporaryPassword>?)null);

            var usedCodes = await passwordService.RetrieveUsedCodes(CancellationToken.None);

            Assert.Null(usedCodes);

            passwordRepo.Verify(c => c.GetUsedCodes(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RemoveCodes()
        {
            var tempPassword = new TemporaryPassword { AccountId = Guid.NewGuid().ToString() };

            var fakeDb = new List<TemporaryPassword>
            {
                tempPassword,
            };

            passwordRepo.Setup(c => c.DeleteCodes(It.IsAny<TemporaryPassword>(), It.IsAny<CancellationToken>())).
                         Callback<TemporaryPassword, CancellationToken>((tempPass, cancellationToken) =>
                         {
                             fakeDb.Remove(tempPass);
                         }).
                         Returns(Task.CompletedTask);

            await passwordService.RemoveCodes(tempPassword,CancellationToken.None);

            passwordRepo.Verify(c => c.DeleteCodes(It.IsAny<TemporaryPassword>(), It.IsAny<CancellationToken>()), Times.Once);
        }

    }

}