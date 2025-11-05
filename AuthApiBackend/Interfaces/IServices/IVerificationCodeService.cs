using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Models;
using System.Threading;

namespace AuthApiBackend.Interfaces.IServices
{
    public interface IVerificationCodeService
    {
        Task CreateCodeAsync(string userId, CancellationToken cancellationToken, int attemptCount = 1);

        Task<IEnumerable<PendingCode>?> GetPendingCodeAsync(CancellationToken cancellationToken);

        Task RequestForCode(UserResponse userAttemptsAndUserId, CancellationToken cancellationToken);

        Task<string> VerifyCodeAsync(string codeId, string code, CancellationToken cancellationToken);

        Task UpdateCodeAsync(string codeId, CancellationToken cancellationToken);

        Task UpdateEmailSentAsync(string codeId, CancellationToken cancellationToken);

        Task<IEnumerable<VerificationCode>?> ExpiredVerificationCodes(CancellationToken cancellationToken);

        Task RemoveCodes(VerificationCode code, CancellationToken cancellationToken);

        Task<IEnumerable<VerificationCode>?> RetrieveUsedCodes(CancellationToken cancellationToken);
    }
}
