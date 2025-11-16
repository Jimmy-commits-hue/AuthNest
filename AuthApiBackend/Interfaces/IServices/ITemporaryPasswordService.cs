using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IServices
{
    public interface ITemporaryPasswordService
    {
        Task CreateTemporaryPassword(string accountId, int attemptCount, CancellationToken cancellationToken);

        Task<string> VerifyPassword(string accountId, string password, CancellationToken cancellationToken);

        Task UpdatePasswordStatus(string accountId, CancellationToken cancellationToken);

        Task<int> NumberOfPendingPasswords(CancellationToken cancellationToken);
         
        Task<IEnumerable<ResetPasswordResponse>?> GetAllPendingPasswords(int round, CancellationToken cancellationToken);

        Task<int> NumberOfExpiredTempCodes(CancellationToken cancellationToken);

        Task<IEnumerable<TemporaryPassword>?> RetrieveExpiredCodes(int rounds, CancellationToken cancellationToken);

        Task<int> NumberOfUsedCodes(CancellationToken cancellationToken);

        Task<IEnumerable<TemporaryPassword>?> RetrieveUsedCodes(int round, CancellationToken cancellationToken);

        Task RemoveCodes(TemporaryPassword code, CancellationToken cancellationToken);

        Task<int> CheckAttemptNumber(string accountId, CancellationToken cancellationToken);
    }
}
