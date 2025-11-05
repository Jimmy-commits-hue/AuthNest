using AuthApiBackend.Configurations;
using AuthApiBackend.DTOs;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Enums;
using AuthApiBackend.Exceptions.ExceptionTypes;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Models;
using AuthApiBackend.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AuthApiBackend.Services
{

    public class AccountService : IAccountService
    {

        private readonly IAccountRepository accountRepo;
        private readonly ILogger<AccountService> logger;
        private readonly MaxAttemptsConfig max;

        public AccountService(IAccountRepository accountRepo, ILogger<AccountService> logger, 
            IOptions<MaxAttemptsConfig> option)
        {
            this.accountRepo = accountRepo;
            this.logger = logger;
            max = option.Value;
        }

        public async Task CreateAccountAsync(string accountId, string password, CancellationToken cancellationToken)
        {
            if(await accountRepo.AccountExists(accountId, cancellationToken))
            {
                logger.LogError("Account for {UserId} already exist", accountId);

                throw new AccountAlreadyExistException("Account already exists");
            }

            var account = new Account
            {
                Id = accountId,
                Password = HashHelper.Hash(password),       
            };

            await accountRepo.CreateAsync(account, cancellationToken);
        }

        public async Task UpdateAccountNumber(string accountId, CancellationToken cancellationToken)
        {
            if(!await accountRepo.AccountExists(accountId, cancellationToken))
            {
                throw new NoAccountMatchException("No account match");
            }
                              
            string? lastAccountNumber = await accountRepo.GetLastAccountNumberAsync(cancellationToken);

            var number = GenerateCode.GenerateAccountNumber(lastAccountNumber);

            await accountRepo.UpdateAccountAsync(accountId, number, cancellationToken);
        }   

        public async Task<IEnumerable<PendingAccountNumbers>?> GetPendingAccounts(CancellationToken cancellationToken)
        {
            return await accountRepo.GetPendingAccounts(cancellationToken);
        }

        public async Task UpdateIsEmailSent(string accountId,CancellationToken cancellationToken)
        {
            await accountRepo.UpdateIsEmailSentStatus(accountId, cancellationToken);
        }

        public async Task<string> VerifyLoginNumber(string loginNumber, CancellationToken cancellationToken)
        {
            var hashedPassword = await accountRepo.GetUserPassword(loginNumber, cancellationToken)
                ?? throw new UserNotFoundException("Please register first.");

            return hashedPassword;
        }

        public async Task<int> VerifyAccountStatus(string loginNumber, CancellationToken cancellationToken)
        {
            var account = await accountRepo.GetFailedAttemptCount(loginNumber, cancellationToken);

            if (account.IsLocked)
                throw new AccountLockedException("Account locked. Please try again later.");

            if (account.Status != AccountStatus.Active)
                throw new AccountInactiveException("Please activate your account first.");

            return account.LoginAttempt;
        }


        public async Task<string> GetAccountId(string loginNumber, CancellationToken cancellationToken)
        {
            return (await accountRepo.GetAccountId(loginNumber, cancellationToken)) ??
                   throw new NoAccountMatchException("No account match");
        }

        public async Task VerifyAttemptNumber(string accountId, int loginAttempt, CancellationToken cancellationToken)
        {
            if (loginAttempt > int.Parse(max.Max))
            {
                await accountRepo.LockAccount(accountId, cancellationToken);

                throw new AccountLockedException("Account locked. Please try again after 24 hours.");
            }
        }

        public async Task VerifyPassword(string accountId, string hashedPassword, string rawPassword, int attemptCount, 
                                        CancellationToken cancellationToken)
        {
            var result = HashHelper.VerifyHash(hashedPassword, rawPassword);

            if (result == PasswordVerificationResult.Failed)
            {
                await accountRepo.UpdateFailedLoginAttempts(accountId, attemptCount + 1, cancellationToken);
                throw new InvalidCredentialsException("Invalid password or account number.");
            }

            if (attemptCount > 1)
            {
                await accountRepo.UpdateFailedLoginAttempts(accountId, 0, cancellationToken);
            }
        }

        public async Task UpdatePassword(UpdatePasswordDto password, CancellationToken cancellationToken)
        {
            OldPassword oldPassword = await accountRepo.RetrieveOldPassword(password.loginNumber, cancellationToken)
                ?? throw new UserNotFoundException("Please register first");

            if(oldPassword.IsLocked)
            {
                throw new AccountLockedException("Account is locked");
            }

            if (oldPassword.Status == AccountStatus.Disabled)
            {
                throw new AccountDisabledException("Please enable your account first");
            }

            if (oldPassword.Status == AccountStatus.Deleted)
            {
                throw new AccountScheduledForDeletionException(
                    "This account has been scheduled for deletion, please restore your account first");
            }

            PasswordVerificationResult result = HashHelper.VerifyHash(oldPassword.OldUserPassword, password.OldPassword);

            if (result is PasswordVerificationResult.Failed)
            {
                throw new InvalidOldPasswordException("Invalid old password");
            }

            await accountRepo.UpdatePassword(oldPassword.accountId, HashHelper.Hash(password.NewPassword), cancellationToken);
        }

        public async Task VerifyResetPassword(string userId, string password, CancellationToken cancellationToken)
        {
            var oldPassword = await accountRepo.GetOldPassword(userId, cancellationToken);

            if(HashHelper.VerifyHash(oldPassword!, password) == PasswordVerificationResult.Success)
            {
                throw new NewOldPasswordEqualException("New password cannot be old password");
            }
        }

        public async Task ResetPassword(string userId, string password, CancellationToken cancellationToken)
        {
            await accountRepo.UpdatePassword(userId, HashHelper.Hash(password), cancellationToken);
        }

        public async Task ScheduleAccountForDeletion(string loginNumber, CancellationToken cancellationToken)
        {
            var accountId = await accountRepo.GetAccountId(loginNumber, cancellationToken);

            await accountRepo.DeleteAccount(accountId!, cancellationToken);
        }

        //still to be refactored 
        public async Task DisableAccount(string loginNumber, CancellationToken cancellationToken)
        {
            var accountId = await accountRepo.GetAccountId(loginNumber, cancellationToken);

            await accountRepo.DisableAccount(accountId!, cancellationToken);
        }

        public async Task<IEnumerable<LockedAccounts>?> GetAllLockedAccounts(CancellationToken cancellationToken)
        {
            return await accountRepo.GetLockedAccounts(cancellationToken);
        }

        public async Task UnlockAccount(string accountId, CancellationToken cancellationToken)
        {
            await accountRepo.UnlockAccount(accountId, cancellationToken);
        }

        public async Task EnableAccount(string accountId, string loginNumber, CancellationToken cancellationToken)
        {
            _ = await accountRepo.GetAccountId(loginNumber, cancellationToken);

            await accountRepo.EnableAccount(accountId, cancellationToken);
        }

        public async Task CancelAccountDeletion(string accountId, string hashedPassword, string password, CancellationToken cancellationToken)
        {
            if(HashHelper.VerifyHash(hashedPassword, password) == PasswordVerificationResult.Failed)
            {
                throw new InvalidPasswordException("No account match");
            }

            await accountRepo.UpdateDeleteStatus(accountId, cancellationToken);
        }

        public async Task<IEnumerable<User>?> GetAllDeletedAccounts(CancellationToken cancellationToken)
        {
            return await accountRepo.GetAllDeletedAccounts(cancellationToken);
        }

    }

}