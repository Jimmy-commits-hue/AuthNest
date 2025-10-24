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



    }

}
