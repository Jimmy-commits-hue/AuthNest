using AuthApiBackend.DTOs;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Exceptions.ExceptionTypes;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Utilities;
using Microsoft.AspNetCore.Identity;

namespace AuthApiBackend.Services
{

    public class AccountService : IAccountService
    {

        private readonly IAccountRepository accountRepo;
        private readonly ILogger<AccountService> logger;

        public AccountService(IAccountRepository accountRepo, ILogger<AccountService> logger)
        {

            this.accountRepo = accountRepo;
            this.logger = logger;

        }

        public async Task CreateAccountAsync(string userId, string password, CancellationToken cancellationToken)
        {

            string? results = await accountRepo.GetUserIdAsync(userId, cancellationToken);

            if (results is not null)
            {

                logger.LogError("Account for {UserId} already exist", results);
                
                throw new AccountAlreadyExistException("Account already exists");

            }

            var account = new Models.Account
            {

                UserId = userId,

                Password = HashHelper.Hash(password),

            };

            await accountRepo.CreateAsync(account, cancellationToken);

        }

        public async Task UpdateAccountNumber(string userId, CancellationToken cancellationToken)
        {

            string results = (await accountRepo.GetUserIdAsync(userId, cancellationToken))!;
                              
            string? lastAccountNumber = await accountRepo.GetLastAccountNumberAsync(cancellationToken);

            var number = GenerateCode.GenerateAccountNumber(lastAccountNumber);

            await accountRepo.UpdateAccountAsync(results, number, cancellationToken);

        }   

        public async Task<IEnumerable<PendingAccountNumbers>?> GetPendingAccounts(CancellationToken cancellationToken)
        {

            return await accountRepo.GetPendingAccounts(cancellationToken);

        }

        public async Task UpdateIsEmailSent(string accountId,CancellationToken cancellationToken)
        {

            await accountRepo.UpdateIsEmailSentStatus(accountId, cancellationToken);

        }

        public async Task VerifyLoginDetails(LoginDto loginDetails, CancellationToken cancellationToken)
        {

            var getPassword = await accountRepo.GetUserLoginDetails(loginDetails.LoginNumber, cancellationToken) ??
                         throw new UserNotFoundException("Please Register first");

            PasswordVerificationResult verifyPassword = HashHelper.VerifyHash(getPassword, loginDetails.Password);

            if(verifyPassword is PasswordVerificationResult.Failed)
            {
                throw new InvalidCredentialsException("Invalid Password or Account number");
            }

        }

        public async Task UpdatePassword(UpdatePasswordDto password, CancellationToken cancellationToken)
        {

            OldPassword oldPassword = await accountRepo.RetrieveOldPassword(password.loginNumber, cancellationToken)
                ?? throw new UserNotFoundException("Please register first");

            if(oldPassword.IsLocked == true)
            {
                throw new AccountLockedException("Account is locked");

            }

            if (oldPassword.Status == Enums.AccountStatus.Disabled)
            {
                throw new AccountDisabledException("Please enable your account first");
            }

            if (oldPassword.Status == Enums.AccountStatus.Deleted)
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

        public async Task<string> GetAccountId(string loginNumber, CancellationToken cancellationToken)
        {
            return await accountRepo.GetAccountId(loginNumber, cancellationToken) ?? 
                 throw new UserNotFoundException("Please register first");
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

    }

}
