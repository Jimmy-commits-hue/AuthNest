using Moq;
using AuthApiBackend.Services;
using AuthApiBackend.Models;
using AuthApiBackend.Utilities;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Exceptions.ExceptionTypes;
using Microsoft.Extensions.Logging;
using AuthApiBackend.DTOs;
using Microsoft.Extensions.Options;
using AuthApiBackend.Enums;

namespace AuthApiBackendTest
{

    [Collection("Env collection")]
    public class AccountServiceTests
    {

        private readonly Mock<IAccountRepository> accountRepo;
        private readonly AccountService accountService;
        private readonly Mock<ILogger<AccountService>> logger;

        public AccountServiceTests()
        {
            accountRepo = new Mock<IAccountRepository>();
            logger = new Mock<ILogger<AccountService>>();
            accountService = new AccountService(accountRepo.Object, logger.Object, Options.Create(
                             new AuthApiBackend.Configurations.MaxAttemptsConfig
                             {
                                 Max = "3"
                             }
            ));
        }

        [Fact]
        public async Task CreateAccountAsync_ShouldCreateAccount()
        {
            string accountId = Guid.NewGuid().ToString();
            string password = "#JaBu@5.";

            string passwordHash = HashHelper.Hash(password);
            
            var fakeDb = new List<Account>();

            accountRepo.Setup(repo => repo.AccountExists(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync(false);

            accountRepo.Setup(accountRepo => accountRepo.CreateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>())).
                        Callback<Account, CancellationToken>((account,cancellationToken) =>
                        {
                           fakeDb.Add(account);   
                        }).
                        Returns(Task.CompletedTask);

            await accountService.CreateAccountAsync(accountId, password, CancellationToken.None);

            Assert.Single(fakeDb);

            accountRepo.Verify(accountRepo => accountRepo.AccountExists(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            accountRepo.Verify(accountRepo => accountRepo.CreateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task CreateAccountAsync_ShouldThrowAccountAlreadyExistsException_IfAccountExists()
        {
            string accountId = Guid.NewGuid().ToString();
            string password = "#JaBu@5.";

            accountRepo.Setup(repo => repo.AccountExists(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<AccountAlreadyExistException>(() => 
                     accountService.CreateAccountAsync(accountId, password, CancellationToken.None));

            Assert.Equal("Account already exists", ex.Message);

            accountRepo.Verify(accountRepo => accountRepo.AccountExists(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            accountRepo.Verify(accountRepo => accountRepo.CreateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        
        [Fact]
        public async Task UpdateAccountNumber_IfAccountExist()
        {
            string accountId = Guid.NewGuid().ToString();
            string lastAccountNumber = "250000001";
            String newAccountNumber = string.Empty;

            accountRepo.Setup(repo => repo.AccountExists(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync(true);

            accountRepo.Setup(repo => repo.GetLastAccountNumberAsync(It.IsAny<CancellationToken>())).
                        ReturnsAsync(lastAccountNumber);

            accountRepo.Setup(repo => repo.UpdateAccountAsync(It.IsAny<string>(),It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        Callback<string, string, CancellationToken>((Id, accountNumber, cancellationToken) =>
                        {
                            newAccountNumber = accountNumber;
                        }).
                        Returns(Task.CompletedTask);

            await accountService.UpdateAccountNumber(accountId, CancellationToken.None);

            Assert.NotEmpty(newAccountNumber);

            accountRepo.Verify(repo => repo.AccountExists(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            accountRepo.Verify(repo => repo.UpdateAccountAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            accountRepo.Verify(repo => repo.GetLastAccountNumberAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task GetPendingAccounts_ReturnsPendingAccounts_IfAny()
        {
            accountRepo.Setup(accountRepo => accountRepo.GetPendingAccounts(It.IsAny<int>(),It.IsAny<CancellationToken>())).
                        ReturnsAsync(
                       [
                           new(
                                "250000001",
                                "jimmy@gmail.com",
                                Guid.NewGuid().ToString()
                              ),

                           new(
                                "250000002",
                                "jimmy@gmail.com",
                                Guid.NewGuid().ToString()
                              )

                       ]);

            var result = await accountService.GetPendingAccounts(0, CancellationToken.None);

            #pragma warning disable CS8604
            Assert.IsType<PendingAccountNumbers>(result.First());
            #pragma warning restore CS8604

            accountRepo.Verify(accountRepo => accountRepo.GetPendingAccounts(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task GetPendingEmails_ReturnsNull_IfNone()
        {
            accountRepo.Setup(accountRepo => accountRepo.GetPendingAccounts(It.IsAny<int>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync((IEnumerable<PendingAccountNumbers>?)null);

            var result = await accountService.GetPendingAccounts(0, CancellationToken.None);

            Assert.Null(result);

            accountRepo.Verify(accountRepo => accountRepo.GetPendingAccounts(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task UpdateIsEmailSent_ShouldUpdateEmailSentStatus()
        {
            string accountId = Guid.NewGuid().ToString();

            bool isEmailSent = false;
            bool isSent = true;

            accountRepo.Setup(accountRepo => accountRepo.UpdateIsEmailSentStatus(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        Callback<string, CancellationToken>((Id, cancellationToken) =>
                        {
                            isEmailSent = true;
                        })
                       .Returns(Task.CompletedTask);

            await accountService.UpdateIsEmailSent(accountId, CancellationToken.None);

            Assert.Equal(isEmailSent, isSent);

            accountRepo.Verify(accountRepo => accountRepo.UpdateIsEmailSentStatus(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task VerifyLoginNumber_ReturnsHashedPassword_IfValid()
        {
            var loginDetail = new LoginDto
            {
                LoginNumber = "250000001",
                Password = "JaBu@5."
            };

            accountRepo.Setup(c => c.GetUserPassword(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync(HashHelper.Hash(loginDetail.Password));

            string hashedPassword = await accountService.VerifyLoginNumber(loginDetail.LoginNumber, CancellationToken.None);

            Assert.NotEqual(hashedPassword, loginDetail.Password);

            accountRepo.Verify(c => c.GetUserPassword(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task VerifyLoginNumber_ThrowsException_IfUserLoginNumberIsInvalid()
        {
            var loginDetail = new LoginDto
            {
                LoginNumber = "250000001",
                Password = "JaBu@5."
            };

            accountRepo.Setup(c => c.GetUserPassword(loginDetail.LoginNumber, It.IsAny<CancellationToken>())).
                        ReturnsAsync(HashHelper.Hash(loginDetail.Password));

            loginDetail.LoginNumber = "250000002";

            var ex = await Assert.ThrowsAsync<UserNotFoundException>(async () => 
                     await accountService.VerifyLoginNumber(loginDetail.LoginNumber, CancellationToken.None));

            Assert.Equal("Please register first.", ex.Message);

            accountRepo.Verify(c => c.GetUserPassword(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyAccountStatus_ReturnsLoginAttempt_IfLessThan3()
        {
            string loginNumber = "250000001";

            accountRepo.Setup(c => c.GetFailedAttemptCount(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync(new VerifyLoginResponse(
                                         1,
                                         false,
                                         AccountStatus.Active
                                     ));

            int loginAttempt = await accountService.VerifyAccountStatus(loginNumber, CancellationToken.None);

            Assert.Equal(1, loginAttempt);
            
            accountRepo.Verify(c => c.GetFailedAttemptCount(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyAccountStatus_ThrowsException_IfAccountLocked()
        {
            string loginNumber = "250000001";

            accountRepo.Setup(c => c.GetFailedAttemptCount(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync(new VerifyLoginResponse(
                                         1,
                                         true,
                                         AccountStatus.Active
                                     ));

            var ex = await Assert.ThrowsAsync<AccountLockedException>(async () =>
                     await  accountService.VerifyAccountStatus(loginNumber, CancellationToken.None));

            Assert.Equal("Account locked. Please try again later.", ex.Message);

            accountRepo.Verify(c => c.GetFailedAttemptCount(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyStatus_ThrowsException_IfAccountScheduledForDeletion()
        {
            string loginNumber = "250000001";

            accountRepo.Setup(c => c.GetFailedAttemptCount(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync(new VerifyLoginResponse(
                                         1,
                                         false,
                                         AccountStatus.Deleted
                                     ));

            var ex = await Assert.ThrowsAsync<AccountInactiveException>(async () =>
                     await accountService.VerifyAccountStatus(loginNumber, CancellationToken.None));

            Assert.Equal("Please activate your account first.", ex.Message);

            accountRepo.Verify(c => c.GetFailedAttemptCount(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyStatus_ThrowsException_IfAccountDisabled()
        {
            string loginNumber = "250000001";

            accountRepo.Setup(c => c.GetFailedAttemptCount(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync(new VerifyLoginResponse(
                                         1,
                                         false,
                                         AccountStatus.Disabled
                                     ));

            var ex = await Assert.ThrowsAsync<AccountInactiveException>(async () =>
                     await accountService.VerifyAccountStatus(loginNumber, CancellationToken.None));

            Assert.Equal("Please activate your account first.", ex.Message);

            accountRepo.Verify(c => c.GetFailedAttemptCount(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyAttemptNumber()
        {
            string accountId = Guid.NewGuid().ToString();
            int attempt = 1;

            await accountService.VerifyAttemptNumber(accountId, attempt, CancellationToken.None);

            accountRepo.Verify(c => c.LockAccount(accountId, CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task VerifyAttemptNumber_LocksAccountAndThrowsException_IfAttemptIsGreaterThan3()
        {
            int attempt = 4;
            bool accountlocked = false;
            bool newAccountlockedStatus = true;

            accountRepo.Setup(c => c.LockAccount(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        Callback<string, CancellationToken>((Id, cancellationToken) =>
                        {
                            accountlocked = true;
                        }).
                        Returns(Task.CompletedTask);

            var ex = await Assert.ThrowsAnyAsync<AccountLockedException>(async () => 
                     await accountService.VerifyAttemptNumber(It.IsAny<string>(), attempt, CancellationToken.None));

            Assert.Equal<bool>(accountlocked, newAccountlockedStatus);
            Assert.Equal("Account locked. Please try again after 24 hours.", ex.Message);
           
            accountRepo.Verify(c => c.LockAccount(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyPassword_NoExceptionThrownAndNeverCallUpdateFailedLoginAttempt_IfPasswordValid()
        {
            int attempt = 1;
            string rawPassword = "#JaBu@5.";
            string hashedPassword = HashHelper.Hash(rawPassword);

            await accountService.VerifyPassword(It.IsAny<string>(), hashedPassword, rawPassword, attempt, CancellationToken.None);

            accountRepo.Verify(c => c.UpdateFailedLoginAttempts(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task VerifyPassword_ThrowsExceptionAndCallUpdateFailedLoginAttempt_IfInvalidPassword()
        {
            string accountId = Guid.NewGuid().ToString();
            int attempt = 1;
            string rawPassword = "#JaBu@5.";
            string hashedPassword = HashHelper.Hash(rawPassword);
            int failedLoginAttempts = 0;
            string wrongPassword = "#JaBu@3.";

            accountRepo.Setup(c => c.UpdateFailedLoginAttempts(accountId, attempt + 1, It.IsAny<CancellationToken>())).
                        Callback<string, int, CancellationToken>((Id, attemptCount, cancellationToken) =>
                        {
                            failedLoginAttempts = attemptCount;
                        }).
                        Returns(Task.CompletedTask);

            var ex = await Assert.ThrowsAnyAsync<InvalidCredentialsException>(async () => 
                     await accountService.VerifyPassword(accountId, hashedPassword, wrongPassword, attempt, CancellationToken.None));

            Assert.Equal(attempt + 1, failedLoginAttempts);
            Assert.Equal("Invalid password or account number.", ex.Message);

            accountRepo.Verify(c => c.UpdateFailedLoginAttempts(It.IsAny<string>(), It.IsAny<int>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task VerifyPassword_ResetFailedLoginAttempt_IfPasswordValidAfterMoreThanOneAttempt()
        {
            string accountId = Guid.NewGuid().ToString();
            int attempt = 2;
            string rawPassword = "#JaBu@5.";
            string hashedPassword = HashHelper.Hash(rawPassword);

            int failedLoginAttempt = 2;

            accountRepo.Setup(c => c.UpdateFailedLoginAttempts(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).
                        Callback<string, int, CancellationToken>((Id, attemptCount, cancellationToken) =>
                        {
                            failedLoginAttempt = attemptCount;
                        }).
                        Returns(Task.CompletedTask);

            await accountService.VerifyPassword(accountId, hashedPassword, rawPassword, attempt, CancellationToken.None);

            Assert.Equal<int>(0, failedLoginAttempt);

            accountRepo.Verify(c => c.UpdateFailedLoginAttempts(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
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

            string hashedPassword = HashHelper.Hash(updatePassword.OldPassword);

            accountRepo.Setup(c => c.RetrieveOldPassword(updatePassword.loginNumber, It.IsAny<CancellationToken>())).
                       ReturnsAsync(new OldPassword(
                           hashedPassword,
                           AccountStatus.Active,
                           accountId,
                           false
                           ));

            accountRepo.Setup(c => c.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        Callback<string, string, CancellationToken>((Id, newPassword, cancellationToken) =>
                        {
                            updatePassword.OldPassword = newPassword;
                        }).
                       Returns(Task.CompletedTask);

            await accountService.UpdatePassword(updatePassword, CancellationToken.None);

            Assert.NotEqual(updatePassword.NewPassword, updatePassword.OldPassword);

            accountRepo.Verify(c => c.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            accountRepo.Verify(c => c.RetrieveOldPassword(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);              
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

            string hashedPassword = HashHelper.Hash(updatePassword.OldPassword);

            accountRepo.Setup(c => c.RetrieveOldPassword(updatePassword.loginNumber, It.IsAny<CancellationToken>())).
                       ReturnsAsync(new OldPassword(
                           hashedPassword,
                           AccountStatus.Deleted,
                           accountId,
                           false
                           ));

            updatePassword.loginNumber = "250000002";

            accountRepo.Setup(c => c.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).
                       Returns(Task.CompletedTask);

            var ex = await Assert.ThrowsAsync<UserNotFoundException>(async () => 
                    await accountService.UpdatePassword(updatePassword, CancellationToken.None));

            Assert.Equal("Please register first", ex.Message);

            accountRepo.Verify(c => c.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
            accountRepo.Verify(c => c.RetrieveOldPassword(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
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

            string hashedPassword = HashHelper.Hash(updatePassword.OldPassword);

            accountRepo.Setup(c => c.RetrieveOldPassword(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                       ReturnsAsync(new OldPassword(
                           hashedPassword,
                           AccountStatus.Active,
                           accountId,
                           true
                           ));

            var ex = await Assert.ThrowsAsync<AccountLockedException>(async () =>
                    await accountService.UpdatePassword(updatePassword, CancellationToken.None));

            Assert.Equal("Account is locked", ex.Message);

            accountRepo.Verify(c => c.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
            accountRepo.Verify(c => c.RetrieveOldPassword(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
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

            string hashedPassword = HashHelper.Hash(updatePassword.OldPassword);

            accountRepo.Setup(c => c.RetrieveOldPassword(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                       ReturnsAsync(new OldPassword(
                           hashedPassword,
                           AccountStatus.Disabled,
                           accountId,
                           false
                           ));

            var ex = await Assert.ThrowsAsync<AccountDisabledException>(async () =>
                    await accountService.UpdatePassword(updatePassword, CancellationToken.None));

            Assert.Equal("Please enable your account first", ex.Message);

            accountRepo.Verify(c => c.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
            accountRepo.Verify(c => c.RetrieveOldPassword(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
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

            string hashedPassword = HashHelper.Hash(updatePassword.OldPassword);

            accountRepo.Setup(c => c.RetrieveOldPassword(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                       ReturnsAsync(new OldPassword(
                           hashedPassword,
                           AccountStatus.Deleted,
                           accountId,
                           false
                           ));

            var ex = await Assert.ThrowsAsync<AccountScheduledForDeletionException>(async () =>
                    await accountService.UpdatePassword(updatePassword, CancellationToken.None));

            Assert.Equal("This account has been scheduled for deletion, please restore your account first", ex.Message);

            accountRepo.Verify(c => c.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            accountRepo.Verify(c => c.RetrieveOldPassword(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
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

            var hashedPassword = HashHelper.Hash(updatePassword.OldPassword);

            accountRepo.Setup(c => c.RetrieveOldPassword(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync(new OldPassword(
                                         hashedPassword,
                                         AccountStatus.Active,
                                         accountId,
                                         false
                                     ));

            updatePassword.OldPassword = "#JaBu@3.";

            var ex = await Assert.ThrowsAsync<InvalidOldPasswordException>(async () =>
                    await accountService.UpdatePassword(updatePassword, CancellationToken.None));

            Assert.Equal("Invalid old password", ex.Message);

            accountRepo.Verify(c => c.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            accountRepo.Verify(c => c.RetrieveOldPassword(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
      
        [Fact]
        public async Task GetAccountId_ReturnsAccountId_IfLoginNumberIsValid()
        {
            string loginNumber = "250000001";

            accountRepo.Setup(c => c.GetAccountId(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync(Guid.NewGuid().ToString());

            var accountId = await accountService.GetAccountId(loginNumber, CancellationToken.None);

            Assert.IsType<string>(accountId);

            accountRepo.Verify(c => c.GetAccountId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAccountId_ThrowsException_IfNoAccountMatch()
        {
            string loginNumber = "250000001";

            accountRepo.Setup(c => c.GetAccountId(loginNumber, It.IsAny<CancellationToken>())).
                        ReturnsAsync(Guid.NewGuid().ToString());

            loginNumber = "250000002";

            var ex = await Assert.ThrowsAnyAsync<NoAccountMatchException>(async () =>
                     await accountService.GetAccountId(loginNumber, CancellationToken.None));

            Assert.Equal("No account match", ex.Message); 

            accountRepo.Verify(c => c.GetAccountId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyResetPassword_ThrowsException_IfOldPasswordIsEqualToNewPassword()
        {
            string password = "#JaBu@5.";
            string accountId = Guid.NewGuid().ToString();

            accountRepo.Setup(c => c.GetOldPassword(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync(HashHelper.Hash(password));

            var ex = await Assert.ThrowsAsync<NewOldPasswordEqualException>(async () =>
                     await accountService.VerifyResetPassword(accountId, password, CancellationToken.None));

            Assert.Equal("New password cannot be old password", ex.Message);

            accountRepo.Verify(c => c.GetOldPassword(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyResetPassword_NoExceptionThrown_IfResetPasswordValid()
        {
            string password = "#JaBu@5.";
            string userId = Guid.NewGuid().ToString();

            accountRepo.Setup(c => c.GetOldPassword(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync(HashHelper.Hash(password));

            await accountService.VerifyResetPassword(userId, "#JaBu@4.", CancellationToken.None);

            accountRepo.Verify(c => c.GetOldPassword(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ResetPassword_ResetsPasswords()
        {
            string accountId = Guid.NewGuid().ToString();
            string newPassword = "#JaBu@5.";
            string renewPassword = string.Empty;

            accountRepo.Setup(c => c.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        Callback<string, string, CancellationToken>((Id, password, cancellationToken) =>
                        {
                            renewPassword = password;
                        }).
                        Returns(Task.CompletedTask);

            await accountService.ResetPassword(accountId, newPassword, CancellationToken.None);

            Assert.NotEmpty(renewPassword);

            accountRepo.Verify(c => c.UpdatePassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ScheduleAccountForDeletion_AccountWillScheduledForDeletion_IfExist()
        {
            string loginNumber = "250000001";
            string accountId = Guid.NewGuid().ToString();

            var fakeDb = new List<Account>
            {
                new()
                {
                    Id = accountId
                },
                new()
                {
                    Id = Guid.NewGuid().ToString()
                }
            };

            accountRepo.Setup(c => c.GetAccountId(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync(accountId);

            accountRepo.Setup(c => c.DeleteAccount(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        Callback<string, CancellationToken>((Id, cancellationToken) =>
                        {
                            fakeDb.Remove(fakeDb.Where(c => c.Id == Id).First());
                        }).
                        Returns(Task.CompletedTask);

            await accountService.ScheduleAccountForDeletion(loginNumber, CancellationToken.None);

            Assert.Single(fakeDb);

            accountRepo.Verify(c => c.GetAccountId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            accountRepo.Verify(c => c.DeleteAccount(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task DisableAccount_DeactivatedAccount_IfExist()
        {
            string accountId = Guid.NewGuid().ToString();
            string loginNumber = "250000001";
            AccountStatus status = AccountStatus.Active;

            accountRepo.Setup(c => c.GetAccountId(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync(accountId);

            accountRepo.Setup(c => c.DisableAccount(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        Callback<string, CancellationToken>((Id, cancellationToken) =>
                        {
                            status = AccountStatus.Disabled;
                        }).
                        Returns(Task.CompletedTask);

            await accountService.DisableAccount(loginNumber, CancellationToken.None);

            Assert.Equal(AccountStatus.Disabled, status);

            accountRepo.Verify(c => c.GetAccountId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            accountRepo.Verify(c => c.DisableAccount(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAllLockedAccounts_ReturnsLockedAccounts_IfAny()
        {
            accountRepo.Setup(c => c.GetLockedAccounts(It.IsAny<int>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync(
                        [
                            new(
                                 Guid.NewGuid().ToString(),
                                 "jimmyjabulani01@gmail.com"
                                )
                        ]);

            var lockedAccounts = await accountService.GetAllLockedAccounts(0, CancellationToken.None);

            #pragma warning disable CS8602
            Assert.IsType<LockedAccounts>(lockedAccounts.GetEnumerator().Current);
            #pragma warning restore CS8602 

            accountRepo.Verify(c => c.GetLockedAccounts(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAllLockedAccounts_ReturnsNull_IfNone()
        {
            accountRepo.Setup(c => c.GetLockedAccounts(It.IsAny<int>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync((List<LockedAccounts>?)null);

            var lockedAccounts = await accountService.GetAllLockedAccounts(0, CancellationToken.None);

            Assert.Null(lockedAccounts);

            accountRepo.Verify(c => c.GetLockedAccounts(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UnlockAccounts_After24HoursOfBeingLocked()
        {
            string accountId = Guid.NewGuid().ToString();
            long dateTime = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();

            accountRepo.Setup(c => c.UnlockAccount(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        Callback<string, CancellationToken>((Id, cancellationToken) =>
                        {
                            dateTime = 0;
                        }).
                        Returns(Task.CompletedTask);

            await accountService.UnlockAccount(accountId, CancellationToken.None);

            Assert.Equal<long>(0, dateTime);

            accountRepo.Verify(c => c.UnlockAccount(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task EnableAccounts_IfAccountWasDisabled()
        {
            string accountId = Guid.NewGuid().ToString();
            string loginNumber = "250000001";
            AccountStatus status = AccountStatus.Disabled;

            accountRepo.Setup(c => c.GetAccountId(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync(accountId);

            accountRepo.Setup(c => c.EnableAccount(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        Callback<string, CancellationToken>((Id, cancellationToken) =>
                        {
                            status = AccountStatus.Active;
                        }).
                        Returns(Task.CompletedTask);

            await accountService.EnableAccount(accountId, loginNumber, CancellationToken.None);

            Assert.Equal(AccountStatus.Active, status);

            accountRepo.Verify(c => c.GetAccountId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            accountRepo.Verify(c => c.EnableAccount(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CancelAccountScheduledDeletion_RetrievesAccount_IfDeletionStillPending()
        {
            string accountId = Guid.NewGuid().ToString();
            string rawPassword = "#JaBu@5.";
            string hashedPassword = HashHelper.Hash(rawPassword);
            AccountStatus accountStatus = AccountStatus.Deleted;

            accountRepo.Setup(c => c.UpdateDeleteStatus(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                        Callback<string, CancellationToken>((Id, cancellationToken) =>
                        {
                            accountStatus = AccountStatus.Active;
                        }).
                        Returns(Task.CompletedTask);

            await accountService.CancelAccountDeletion(accountId, hashedPassword, rawPassword, CancellationToken.None);

            Assert.Equal(AccountStatus.Active, accountStatus);

            accountRepo.Verify(c => c.UpdateDeleteStatus(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CancelAccountScheduledDeletion_RetrievalFails_IfPasswordIncorrect()
        {
            string accountId = Guid.NewGuid().ToString();
            string rawPassword = "#JaBu@5.";
            string hashedPassword = HashHelper.Hash(rawPassword);
            string wrongPassword = "#JaBu@4.";

            var ex = await Assert.ThrowsAnyAsync<InvalidPasswordException>(async () => 
                     await accountService.CancelAccountDeletion(accountId, hashedPassword, wrongPassword, CancellationToken.None));

            Assert.Equal("No account match", ex.Message);

            accountRepo.Verify(c => c.UpdateDeleteStatus(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetAllDeletedAccounts_ReturnsAccountDueToDeletion_IfAny()
        {
            accountRepo.Setup(c => c.GetAllDeletedAccounts(It.IsAny<int>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync(
                        [
                            new()
                            {
                                Id = HashHelper.HashId("0987874564767"),
                                IdNumber = HashHelper.HashId("0192087466787"),
                                FirstName = "Jabulani",
                                Surname = "Khabana",
                            }
                        ]);

            var deletedAccount = await accountService.GetAllDeletedAccounts(0, CancellationToken.None);

            #pragma warning disable CS8602 
            var user = deletedAccount.GetEnumerator().Current;
            #pragma warning restore CS8602

            Assert.IsType<User>(deletedAccount.First());

            accountRepo.Verify(c => c.GetAllDeletedAccounts(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAllDeletedAccounts_ReturnNull_IfNone()
        {
            accountRepo.Setup(c => c.GetAllDeletedAccounts(It.IsAny<int>(), It.IsAny<CancellationToken>())).
                        ReturnsAsync((List<User>?)null);

            var deletedAccounts = await accountService.GetAllDeletedAccounts(0, CancellationToken.None);

            Assert.Null(deletedAccounts);

            accountRepo.Verify(c => c.GetAllDeletedAccounts(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
    }

}