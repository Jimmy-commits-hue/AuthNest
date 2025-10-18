using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Exceptions.ExceptionTypes;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Utilities;

namespace AuthApiBackend.Services
{
    public class AccountService(IAccountRepository accountRepo) : IAccountService
    {

        public async Task CreateAccountAsync(string userId, string password, CancellationToken cancellationToken)
        {

            string? results = await accountRepo.ExistsAsync(userId, cancellationToken);

            if (results is not null)
            {
                throw new AccountAlreadyExistException("Account already exists");
            }

            var account = new Models.Account
            {
                UserId = userId,
                Password = HashHelper.HashPassword(password),
            };

            await accountRepo.CreateAsync(account, cancellationToken);
        }

        public async Task UpdateAccountNumber(string userId, CancellationToken cancellationToken)
        {
            string? results = await accountRepo.ExistsAsync(userId, cancellationToken) ?? 
                              throw new NoAccountMatchException("Please register first");

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
    }
}
