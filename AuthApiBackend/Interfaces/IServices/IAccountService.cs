using AuthApiBackend.DTOs.ResponseDtos;

namespace AuthApiBackend.Interfaces.IServices
{
    public interface IAccountService
    {
        Task CreateAccountAsync(string userId, string password, CancellationToken cancellationToken);

        Task UpdateAccountNumber(string userId, CancellationToken cancellationToken);

        Task <IEnumerable<PendingAccountNumbers>?> GetPendingAccounts(CancellationToken cancellationToken);

        Task UpdateIsEmailSent(string accountId, CancellationToken cancellationToken);
    }
}
