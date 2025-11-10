using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IRepositories
{
    public interface IAccountRepository
    {
        Task CreateAsync(Account account, CancellationToken cancellationToken);

        Task<bool> AccountExists(string accountId, CancellationToken cancellationToken);

        Task UpdateAccountAsync(string accountId, string accountNumber, CancellationToken cancellationToken);

        Task<string?> GetLastAccountNumberAsync(CancellationToken cancellationToken);

        Task<IEnumerable<PendingAccountNumbers>?> GetPendingAccounts(CancellationToken cancellationToken);

        Task<AccountResponse> GetAccountDetailsUponLogin(string accountdId, CancellationToken cancellationToken);

        Task UpdateIsEmailSentStatus(string accountId, CancellationToken cancellationToken);

        Task<string?> GetUserPassword(string accountNumber, CancellationToken cancellationToken);

        Task<OldPassword?> RetrieveOldPassword(string accountId, CancellationToken cancellationToken);

        Task<string?> GetAccountId(string loginNumber, CancellationToken cancellationToken);

        Task<string?> GetOldPassword(string userId, CancellationToken cancellationToken);

        Task UpdateFailedLoginAttempts(string accountId, int failedAttempt, CancellationToken cancellationToken);

        Task<VerifyLoginResponse> GetFailedAttemptCount(string accountId, CancellationToken cancellationToken);

        Task LockAccount(string accountId, CancellationToken cancellationToken);

        Task DisableAccount(string accountId, CancellationToken cancellationToken);

        Task EnableAccount(string accountId, CancellationToken cancellationToken);

        Task DeleteAccount(string accountId, CancellationToken cancellationToken);

        Task UpdatePassword(string accountId, string NewPassword, CancellationToken cancellationToken);

        Task<IEnumerable<LockedAccounts>?> GetLockedAccounts(CancellationToken cancellationToken);

        Task UnlockAccount(string accountId, CancellationToken cancellationToken);

        Task UpdateDeleteStatus(string accountId, CancellationToken cancellationToken);

        Task<IEnumerable<User>?> GetAllDeletedAccounts(CancellationToken cancellationToken);
    }
}
