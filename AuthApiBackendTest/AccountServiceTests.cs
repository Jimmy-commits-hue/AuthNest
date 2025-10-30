using Moq;
using AuthApiBackend.Services;
using AuthApiBackend.Models;
using AuthApiBackend.Utilities;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Exceptions.ExceptionTypes;
using Microsoft.Extensions.Logging;
using AuthApiBackend.DTOs;

namespace AuthApiBackendTest
{
    public class AccountServiceTests
    {

        private readonly Mock<IAccountRepository> accountRepo;
        private readonly AccountService accountService;
        private readonly Mock<ILogger<AccountService>> logger;

        public AccountServiceTests()
        {
            accountRepo = new Mock<IAccountRepository>();
            logger = new Mock<ILogger<AccountService>>();
            accountService = new AccountService(accountRepo.Object, logger.Object);
        }

        [Fact]
        public async Task CreateAccountAsync_ShouldCreateAccount()
        {
            string userId = Guid.NewGuid().ToString();
            string password = "#JaBu@5.";

            string passwordHash = HashHelper.Hash(password);
            
            var account = new Account
            {
                UserId = userId,
                Password = passwordHash,
            };

            accountRepo.Setup(repo => repo.GetUserIdAsync(userId, CancellationToken.None))
                       .ReturnsAsync((string?)null);

            accountRepo.Setup(accountRepo => accountRepo.CreateAsync(account, CancellationToken.None))
                       .Returns(Task.CompletedTask);

            await accountService.CreateAccountAsync(userId, password, CancellationToken.None);

            accountRepo.Verify(accountRepo => accountRepo.GetUserIdAsync(userId, CancellationToken.None), Times.Once);
            accountRepo.Verify(accountRepo => accountRepo.CreateAsync(It.IsAny<Account>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task CreateAccountAsync_ShouldThrowAccountAlreadyExistsException_IfAccountExists()
        {
            string userId = Guid.NewGuid().ToString();
            string password = "#JaBu@5.";

            accountRepo.Setup(repo => repo.GetUserIdAsync(userId, CancellationToken.None))
                       .ReturnsAsync(userId);

            await Assert.ThrowsAsync<AccountAlreadyExistException>(() => 
                accountService.CreateAccountAsync(userId, password, CancellationToken.None));

            accountRepo.Verify(accountRepo => accountRepo.GetUserIdAsync(userId, CancellationToken.None), Times.Once);
            accountRepo.Verify(accountRepo => accountRepo.CreateAsync(It.IsAny<Account>(), CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task UpdateAccountNumber_UpdateAccountNumber_IfAccountExist()
        {
            string userId = Guid.NewGuid().ToString();
            string lastAccountNumber = "250000001";
            String newAccountNumber = "250000002";

            accountRepo.Setup(repo => repo.GetUserIdAsync(userId, CancellationToken.None))
                       .ReturnsAsync(userId);

            accountRepo.Setup(repo => repo.GetLastAccountNumberAsync(CancellationToken.None))
                       .ReturnsAsync(lastAccountNumber);

            accountRepo.Setup(repo => repo.UpdateAccountAsync(userId,GenerateCode.GenerateAccountNumber(lastAccountNumber),
                       CancellationToken.None)).Returns(Task.CompletedTask);

            await accountService.UpdateAccountNumber(userId, CancellationToken.None);

            Assert.Equal(newAccountNumber, GenerateCode.GenerateAccountNumber(lastAccountNumber));

            accountRepo.Verify(repo => repo.GetUserIdAsync(userId, CancellationToken.None), Times.Once);
            accountRepo.Verify(repo => repo.UpdateAccountAsync(It.IsAny<string>(), newAccountNumber, CancellationToken.None), Times.Once);
            accountRepo.Verify(repo => repo.GetLastAccountNumberAsync(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetPendingAccounts_ReturnsPendingAccounts_IfAny()
        {

            accountRepo.Setup(accountRepo => accountRepo.GetPendingAccounts(CancellationToken.None))
                       .ReturnsAsync(new List<PendingAccountNumbers>
                       {
                           new PendingAccountNumbers { AccountId = Guid.NewGuid().ToString(), AccountNumber = "250000001", 
                               Email = "jimmy@gmail.com" },
                           new PendingAccountNumbers { AccountId = Guid.NewGuid().ToString(), AccountNumber = "250000002", 
                               Email = "jimmy@gmail.com" }
                       });

            var result = await accountService.GetPendingAccounts(CancellationToken.None);

            Assert.IsType<List<PendingAccountNumbers>>(result);

            accountRepo.Verify(accountRepo => accountRepo.GetPendingAccounts(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetPendingEmails_ReturnsNull_IfNone()
        {

            accountRepo.Setup(accountRepo => accountRepo.GetPendingAccounts(CancellationToken.None))
                       .ReturnsAsync((IEnumerable<PendingAccountNumbers>?)null);

            var result = await accountService.GetPendingAccounts(CancellationToken.None);

            Assert.Null(result);

            accountRepo.Verify(accountRepo => accountRepo.GetPendingAccounts(CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task UpdateIsEmailSent_ShouldUpdateEmailSentStatus()
        {

            string accountId = Guid.NewGuid().ToString();

            accountRepo.Setup(accountRepo => accountRepo.UpdateIsEmailSentStatus(accountId, CancellationToken.None))
                       .Returns(Task.CompletedTask);

            await accountService.UpdateIsEmailSent(accountId, CancellationToken.None);

            accountRepo.Verify(accountRepo => accountRepo.UpdateIsEmailSentStatus(accountId, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task VerifyLoginDetails_NoException_IfValid()
        {

            var loginDetail = new LoginDto
            {
                LoginNumber = "250000001",
                Password = "JaBu@5."
            };

            accountRepo.Setup(c => c.GetUserLoginDetails(loginDetail.LoginNumber, CancellationToken.None)).
                        ReturnsAsync(HashHelper.Hash(loginDetail.Password));

             await accountService.VerifyLoginDetails(loginDetail, CancellationToken.None);

            accountRepo.Verify(c => c.GetUserLoginDetails(It.IsAny<string>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task VerifyLoginDetails_ThrowsException_IfUserLoginNumberIsInvalid()
        {

            var loginDetail = new LoginDto
            {
                LoginNumber = "250000001",
                Password = "JaBu@5."
            };


            accountRepo.Setup(c => c.GetUserLoginDetails(loginDetail.LoginNumber, CancellationToken.None)).
                        ReturnsAsync(HashHelper.Hash(loginDetail.Password));

            loginDetail.LoginNumber = "250000002";

            var ex = await Assert.ThrowsAsync<UserNotFoundException>(async () => 
            await accountService.VerifyLoginDetails(loginDetail, CancellationToken.None));

            Assert.Equal("Please Register first", ex.Message);

            accountRepo.Verify(c => c.GetUserLoginDetails(It.IsAny<string>(), CancellationToken.None), Times.Once);

        }

        [Fact]
        public async Task VerifyLoginDetails_ThrowsException_IfPasswordIsInvalid()
        {

            var loginDetail = new LoginDto
            {
                LoginNumber = "250000001",
                Password = "JaBu@5."
            };


            accountRepo.Setup(c => c.GetUserLoginDetails(loginDetail.LoginNumber, CancellationToken.None)).
                        ReturnsAsync(HashHelper.Hash(loginDetail.Password));

            loginDetail.Password = "JaBu@4.";

            var ex = await Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
            await accountService.VerifyLoginDetails(loginDetail, CancellationToken.None));

            Assert.Equal("Invalid Password or Account number", ex.Message);

            accountRepo.Verify(c => c.GetUserLoginDetails(It.IsAny<string>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task UpdatePassowrd_ItReplaceOldPasswordWithNewPassword()
        {
            var accountId = Guid.NewGuid().ToString();

            var updatePassword = new UpdatePasswordDto
            {
                loginNumber = "250000001",
                OldPassword = "#JaBu@5.",
                NewPassword = "#JaBu@4.",
                ConfirmPassword = "#JaBu@4."
            };

            accountRepo.Setup(c => c.RetrieveOldPassword(updatePassword.loginNumber, CancellationToken.None)).
                        ReturnsAsync(new OldPassword (
                            HashHelper.Hash(updatePassword.OldPassword),
                            AuthApiBackend.Enums.AccountStatus.Active,
                            accountId,
                            false
                            ));

            accountRepo.Setup(c => c.UpdatePassword(accountId, updatePassword.NewPassword, CancellationToken.None)).
                       Returns(Task.CompletedTask);

            await accountService.UpdatePassword(updatePassword, CancellationToken.None);

            accountRepo.Verify(c => c.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None), Times.Once);
            accountRepo.Verify(c => c.RetrieveOldPassword(It.IsAny<string>(), CancellationToken.None), Times.Once);
                       
        }

        [Fact]
        public async Task UpdatePassword_ThrowsException_IfUserNotFound()
        {

            var accountId = Guid.NewGuid().ToString();

            var updatePassword = new UpdatePasswordDto
            {
                loginNumber = "250000001",
                OldPassword = "#JaBu@5.",
                NewPassword = "#JaBu@4.",
                ConfirmPassword = "#JaBu@4."
            };

            accountRepo.Setup(c => c.RetrieveOldPassword(updatePassword.loginNumber, CancellationToken.None)).
                       ReturnsAsync(new OldPassword(
                           HashHelper.Hash(updatePassword.OldPassword),
                           AuthApiBackend.Enums.AccountStatus.Active,
                           accountId,
                           false
                           ));

            updatePassword.loginNumber = "250000002";

            accountRepo.Setup(c => c.UpdatePassword(accountId, updatePassword.NewPassword, CancellationToken.None)).
                       Returns(Task.CompletedTask);

            var ex = await Assert.ThrowsAsync<UserNotFoundException>(async () => 
                    await accountService.UpdatePassword(updatePassword, CancellationToken.None));

            Assert.Equal("Please register first", ex.Message);

            accountRepo.Verify(c => c.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None),
                Times.Never);
            accountRepo.Verify(c => c.RetrieveOldPassword(It.IsAny<string>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task UpdatePassword_ThrowsException_IfAccountLocked()
        {

            var accountId = Guid.NewGuid().ToString();

            var updatePassword = new UpdatePasswordDto
            {
                loginNumber = "250000001",
                OldPassword = "#JaBu@5.",
                NewPassword = "#JaBu@4.",
                ConfirmPassword = "#JaBu@4."
            };

            accountRepo.Setup(c => c.RetrieveOldPassword(updatePassword.loginNumber, CancellationToken.None)).
                       ReturnsAsync(new OldPassword(
                           HashHelper.Hash(updatePassword.OldPassword),
                           AuthApiBackend.Enums.AccountStatus.Active,
                           accountId,
                           true
                           ));

            accountRepo.Setup(c => c.UpdatePassword(accountId, updatePassword.NewPassword, CancellationToken.None)).
                       Returns(Task.CompletedTask);

            var ex = await Assert.ThrowsAsync<AccountLockedException>(async () =>
                    await accountService.UpdatePassword(updatePassword, CancellationToken.None));

            Assert.Equal("Account is locked", ex.Message);

            accountRepo.Verify(c => c.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None),
                Times.Never);
            accountRepo.Verify(c => c.RetrieveOldPassword(It.IsAny<string>(), CancellationToken.None), Times.Once);

        }

        [Fact]
        public async Task UpdatePassword_ThrowsException_AccountDisable()
        {
            var accountId = Guid.NewGuid().ToString();

            var updatePassword = new UpdatePasswordDto
            {
                loginNumber = "250000001",
                OldPassword = "#JaBu@5.",
                NewPassword = "#JaBu@4.",
                ConfirmPassword = "#JaBu@4."
            };

            accountRepo.Setup(c => c.RetrieveOldPassword(updatePassword.loginNumber, CancellationToken.None)).
                       ReturnsAsync(new OldPassword(
                           HashHelper.Hash(updatePassword.OldPassword),
                           AuthApiBackend.Enums.AccountStatus.Disabled,
                           accountId,
                           false
                           ));

            accountRepo.Setup(c => c.UpdatePassword(accountId, updatePassword.NewPassword, CancellationToken.None)).
                       Returns(Task.CompletedTask);

            var ex = await Assert.ThrowsAsync<AccountDisabledException>(async () =>
                    await accountService.UpdatePassword(updatePassword, CancellationToken.None));

            Assert.Equal("Please enable your account first", ex.Message);

            accountRepo.Verify(c => c.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None),
                Times.Never);
            accountRepo.Verify(c => c.RetrieveOldPassword(It.IsAny<string>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task UpdatePassword_ThrowsException_IfAccountScheduledForDeletion()
        {

            var accountId = Guid.NewGuid().ToString();

            var updatePassword = new UpdatePasswordDto
            {
                loginNumber = "250000001",
                OldPassword = "#JaBu@5.",
                NewPassword = "#JaBu@4.",
                ConfirmPassword = "#JaBu@4."
            };

            accountRepo.Setup(c => c.RetrieveOldPassword(updatePassword.loginNumber, CancellationToken.None)).
                       ReturnsAsync(new OldPassword(
                           HashHelper.Hash(updatePassword.OldPassword),
                           AuthApiBackend.Enums.AccountStatus.Deleted,
                           accountId,
                           false
                           ));

            accountRepo.Setup(c => c.UpdatePassword(accountId, updatePassword.NewPassword, CancellationToken.None)).
                       Returns(Task.CompletedTask);

            var ex = await Assert.ThrowsAsync<AccountScheduledForDeletionException>(async () =>
                    await accountService.UpdatePassword(updatePassword, CancellationToken.None));

            Assert.Equal("This account has been scheduled for deletion, please restore your account first", ex.Message);

            accountRepo.Verify(c => c.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None),
                Times.Never);
            accountRepo.Verify(c => c.RetrieveOldPassword(It.IsAny<string>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task UpdatePassword_ThrowsException_IfInvalidOldPassword()
        {

            var accountId = Guid.NewGuid().ToString();

            var updatePassword = new UpdatePasswordDto
            {
                loginNumber = "250000001",
                OldPassword = "#JaBu@5.",
                NewPassword = "#JaBu@4.",
                ConfirmPassword = "#JaBu@4."
            };

            accountRepo.Setup(c => c.RetrieveOldPassword(updatePassword.loginNumber, CancellationToken.None)).
                       ReturnsAsync(new OldPassword(
                           HashHelper.Hash(updatePassword.OldPassword),
                           AuthApiBackend.Enums.AccountStatus.Active,
                           accountId,
                           false
                           ));

            updatePassword.OldPassword = "#JaBu@3.";

            accountRepo.Setup(c => c.UpdatePassword(accountId, updatePassword.NewPassword, CancellationToken.None)).
                       Returns(Task.CompletedTask);

            var ex = await Assert.ThrowsAsync<InvalidOldPasswordException>(async () =>
                    await accountService.UpdatePassword(updatePassword, CancellationToken.None));

            Assert.Equal("Invalid old password", ex.Message);

            accountRepo.Verify(c => c.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None),
                Times.Never);
            accountRepo.Verify(c => c.RetrieveOldPassword(It.IsAny<string>(), CancellationToken.None), Times.Once);
        }


        [Fact]
        public async Task GetAccountId_ReturnsAccountId_IfLoginNumberIsValid()
        {

            string loginNumber = "250000001";

            accountRepo.Setup(c => c.GetAccountId(loginNumber, CancellationToken.None)).
                        ReturnsAsync(Guid.NewGuid().ToString());

            var accountId = await accountService.GetAccountId(loginNumber, CancellationToken.None);

            Assert.IsType<string>(accountId);

            accountRepo.Verify(c => c.GetAccountId(It.IsAny<string>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetAccountId_ThrowsException_IfIncorrectLoginNumber()
        {

            string loginNumber = "250000001";

            accountRepo.Setup(c => c.GetAccountId(loginNumber, CancellationToken.None)).ReturnsAsync((string?)null);

            var ex = await Assert.ThrowsAsync<UserNotFoundException>(async () =>
                     await accountService.GetAccountId(loginNumber, CancellationToken.None));

            Assert.Equal("Please register first", ex.Message);

            accountRepo.Verify(c => c.GetAccountId(It.IsAny<string>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task VerifyResetPassword_ThrowsException_IfOldPasswordIsEqualToNewPassword()
        {

            string password = "#JaBu@5.";
            string userId = Guid.NewGuid().ToString();

            accountRepo.Setup(c => c.GetOldPassword(userId, CancellationToken.None)).
                ReturnsAsync(HashHelper.Hash(password));

            var ex = await Assert.ThrowsAsync<NewOldPasswordEqualException>(async () =>
                     await accountService.VerifyResetPassword(userId, password, CancellationToken.None));

            Assert.Equal("New password cannot be old password", ex.Message);

            accountRepo.Verify(c => c.GetOldPassword(It.IsAny<string>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task VerifyResetPassword()
        {

            string password = "#JaBu@5.";
            string userId = Guid.NewGuid().ToString();

            accountRepo.Setup(c => c.GetOldPassword(userId, CancellationToken.None)).
                ReturnsAsync(HashHelper.Hash(password));

            await accountService.VerifyResetPassword(userId, "#JaBu@4.", CancellationToken.None);

            accountRepo.Verify(c => c.GetOldPassword(It.IsAny<string>(), CancellationToken.None), Times.Once);

        }


        [Fact]
        public async Task ResetPassword_ResetsPasswords()
        {

            string accountId = Guid.NewGuid().ToString();
            string newPassword = "#JaBu@5.";

            accountRepo.Setup(c => c.UpdatePassword(accountId, newPassword, CancellationToken.None)).
                        Returns(Task.CompletedTask);

            await accountService.ResetPassword(accountId, newPassword, CancellationToken.None);

            accountRepo.Verify(c => c.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None),
                Times.Once);
        }
    }
}
