using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IRepositories
{

    public interface IVerificationCodeRepo
    {

        Task CreateAsync(VerificationCode code, CancellationToken cancellationToken);

        Task<VerificationResponse?> GetAsync(string codeId, CancellationToken cancellationToken);

        Task<int> NumberOfPendingCodes(CancellationToken cancellationToken);

        Task<IEnumerable<PendingCode>?> GetPendingCodes(int round,CancellationToken cancellationToken);

        Task<bool> IsUserEmailVerified(string userId, CancellationToken cancellationToken);

        Task UpdateAsync(VerificationCode code, CancellationToken cancellationToken);

        Task UpdateEmailSentAsync(string codeId, CancellationToken cancellationToken);

        Task UpdateActiveStatusAsync(string codeId, CancellationToken cancellationToken);

        Task<int> NumberOfExpiredCodes(CancellationToken cancellationToken);

        Task<IEnumerable<VerificationCode>?> GetExpiredVericationCodes(int round, CancellationToken cancellationToken);

        Task DeleteCodes(VerificationCode code, CancellationToken cancellationToken);

        Task<string?> GetCodeId(string accountId, CancellationToken cancellationToken);

        Task DeactivateOldCode(string code, CancellationToken cancellationToken);

        Task<int> NumberOfUsedVerificationCodes(CancellationToken cancellationToken);

        Task<IEnumerable<VerificationCode>?> GetAllUsedVerificationCodes(int round, CancellationToken cancellationToken);
    }

}
