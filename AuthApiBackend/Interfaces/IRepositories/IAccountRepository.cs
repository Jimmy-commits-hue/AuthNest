using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IRepositories
{
    public interface IAccountRepository
    {
        Task CreateAsync(Account account, CancellationToken cancellationToken);

        Task<string?> ExistsAsync(string userId, CancellationToken cancellationToken);

        Task UpdateAccountAsync(string userId, string accountNumber, CancellationToken cancellationToken);

        Task<string?> GetLastAccountNumberAsync(CancellationToken cancellationToken);

        Task<IEnumerable<PendingAccountNumbers>?> GetPendingAccounts(CancellationToken cancellationToken);

        Task UpdateIsEmailSentStatus(string accountId, CancellationToken cancellationToken);
    }
}
