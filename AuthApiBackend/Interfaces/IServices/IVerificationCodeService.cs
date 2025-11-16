using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Models;
using System.Threading;

namespace AuthApiBackend.Interfaces.IServices
{
    public interface IVerificationCodeService
    {
        Task CreateCodeAsync(string userId, CancellationToken cancellationToken, int attemptCount = 1);

        Task<int> NumberOfPendingCodes(CancellationToken cancellationToken);

        Task<IEnumerable<PendingCode>?> GetPendingCodeAsync(int round, CancellationToken cancellationToken);

        Task RequestForCode(UserResponse userAttemptsAndUserId, CancellationToken cancellationToken);

        Task<string> VerifyCodeAsync(string codeId, string code, CancellationToken cancellationToken);

        Task UpdateCodeAsync(string codeId, CancellationToken cancellationToken);

        Task UpdateEmailSentAsync(string codeId, CancellationToken cancellationToken);

        Task<int> NumberOfExpiredCodes(CancellationToken cancellationToken);

        Task<IEnumerable<VerificationCode>?> ExpiredVerificationCodes(int round, CancellationToken cancellationToken);

        Task RemoveCodes(VerificationCode code, CancellationToken cancellationToken);

        Task<int> NumberOfUsedCodes(CancellationToken cancellationToken);
         
        Task<IEnumerable<VerificationCode>?> RetrieveUsedCodes(int round, CancellationToken cancellationToken);
    }
}
