using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IRepositories
{

    public interface ITemporaryPasswordRepo
    {

        Task CreatePassword(TemporaryPassword temp, CancellationToken cancellationToken);

        Task<PasswordResetResponse> GetPassword(string accountId, CancellationToken cancellationToken);

        Task UpdateStatus(string accountId, CancellationToken cancellationToken);

        Task<int> NumberOfPendingPasswords(CancellationToken cancellationToken);

        Task<IEnumerable<ResetPasswordResponse>?> GetAllPendingPasswords(int round, CancellationToken cancellationToken);

        Task<int> GetAttemptCount(string accountId, CancellationToken cancellationToken);

        Task<string> GetTempCodeId(string accountId, CancellationToken cancellationToken);

        Task DeactivateOldCode(string tempId, CancellationToken cancellationToken);

        Task<int> CountExpiredTempCodes(CancellationToken cancellationToken);

        Task<IEnumerable<TemporaryPassword>?> GetExpiredCodes(int rounds, CancellationToken cancellationToken);

        Task<int> CountUsedCodes(CancellationToken cancellationToken);

        Task<IEnumerable<TemporaryPassword>?> GetUsedCodes(int round, CancellationToken cancellationToken);

        Task DeleteCodes(TemporaryPassword code, CancellationToken cancellationToken);
    }

}
