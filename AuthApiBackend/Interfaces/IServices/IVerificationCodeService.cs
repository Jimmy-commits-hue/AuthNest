using AuthApiBackend.DTOs.ResponseDtos;
using System.Threading;

namespace AuthApiBackend.Interfaces.IServices
{
    public interface IVerificationCodeService
    {
        Task CreateCodeAsync(string userId, CancellationToken cancellationToken, int attemptCount = 0);

        Task<IEnumerable<PendingCode>?> GetPendingCodeAsync(CancellationToken cancellationToken);

        Task RequestForCode(UserResponse userAttemptsAndUserId, CancellationToken cancellationToken);

        Task<string> VerifyCodeAsync(string codeId, string code, CancellationToken cancellationToken);

        Task UpdateCodeAsync(string codeId, CancellationToken cancellationToken);

        Task UpdateEmailSentAsync(string codeId, CancellationToken cancellationToken);
    }
}
