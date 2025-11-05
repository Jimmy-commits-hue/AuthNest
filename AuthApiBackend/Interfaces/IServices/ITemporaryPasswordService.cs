using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IServices
{
    public interface ITemporaryPasswordService
    {
        Task CreateTemporaryPassword(string accountId, int attemptCount, CancellationToken cancellationToken);

        Task<string> VerifyPassword(string accountId, string password, CancellationToken cancellationToken);

        Task UpdatePasswordStatus(string accountId, CancellationToken cancellationToken);

        Task<IEnumerable<ResetPasswordResponse>?> GetAllPendingPasswords(CancellationToken cancellationToken);

        Task<IEnumerable<TemporaryPassword>?> RetrieveExpiredCodes(CancellationToken cancellationToken);

        Task<IEnumerable<TemporaryPassword>?> RetrieveUsedCodes(CancellationToken cancellationToken);

        Task RemoveCodes(TemporaryPassword code, CancellationToken cancellationToken);

        Task<int> CheckAttemptNumber(string accountId, CancellationToken cancellationToken);
    }
}
