using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IRepositories
{

    public interface ITemporaryPasswordRepo
    {

        Task CreatePassword(TemporaryPassword temp, CancellationToken cancellationToken);

        Task<PasswordResetResponse> GetPassword(string accountId, CancellationToken cancellationToken);

        Task UpdateStatus(string accountId, CancellationToken cancellationToken);

        Task<IEnumerable<ResetPasswordResponse>?> GetAllPendingPasswords(CancellationToken cancellationToken);

        Task<int> GetAttemptCount(string accountId, CancellationToken cancellationToken);

        Task<string> GetTempCodeId(string accountId, CancellationToken cancellationToken);

        Task DeactivateOldCode(string tempId, CancellationToken cancellationToken);

        Task<IEnumerable<TemporaryPassword>?> GetExpiredCodes(CancellationToken cancellationToken);

        Task<IEnumerable<TemporaryPassword>?> GetUsedCodes(CancellationToken cancellationToken);

        Task DeleteCodes(TemporaryPassword code, CancellationToken cancellationToken);
    }

}
