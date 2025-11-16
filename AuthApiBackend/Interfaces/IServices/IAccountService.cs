using AuthApiBackend.DTOs;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IServices
{
    public interface IAccountService
    {
        Task CreateAccountAsync(string userId, string password, CancellationToken cancellationToken);

        Task UpdateAccountNumber(string userId, CancellationToken cancellationToken);

        Task<int> GetNumberOfPendingAccounts(CancellationToken cancellationToken);

        Task <IEnumerable<PendingAccountNumbers>?> GetPendingAccounts(int round, CancellationToken cancellationToken);

        Task<int> NumberOfAccountsToDelete(CancellationToken cancellationToken);

        Task UpdateIsEmailSent(string accountId, CancellationToken cancellationToken);

        Task<AccountResponse> GetAccountUserDeatailsUponLogin(string accountId, CancellationToken cancellationToken);
        Task<string> VerifyLoginNumber(string loginNumber, CancellationToken cancellationToken);

        Task<int> VerifyAccountStatus(string loginNumber, CancellationToken cancellationToken);

        Task<string> GetAccountId(string loginNumber, CancellationToken cancellationToken);

        Task VerifyAttemptNumber(string accountId, int loginAttempt, CancellationToken cancellationToken);

        Task VerifyPassword(string accountId, string hashedPassword, string rawPassword, int attemptCount, CancellationToken cancellationToken);

        Task UpdatePassword(UpdatePasswordDto password, CancellationToken cancellationToken);

        Task CancelAccountDeletion(string accountId, string loginNumber, string password, CancellationToken cancellationToken);

        Task VerifyResetPassword(string userId, string password, CancellationToken cancellationToken);

        Task ResetPassword(string userId, string password, CancellationToken cancellationToken);

        Task<int> NumberOfLockedAccounts(CancellationToken cancellationToken);

        Task<IEnumerable<LockedAccounts>?> GetAllLockedAccounts(int round, CancellationToken cancellationToken);

        Task DisableAccount(string loginNumber, CancellationToken cancellationToken);

        Task EnableAccount(string userId, string loginNumber, CancellationToken cancellationToken);

        Task UnlockAccount(string accountId, CancellationToken cancellationToken);

        Task ScheduleAccountForDeletion(string loginNumber, CancellationToken cancellationToken);

        Task<IEnumerable<User>?> GetAllDeletedAccounts(int round, CancellationToken cancellationToken);
    }
}
