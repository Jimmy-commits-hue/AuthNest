using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IRepositories
{

    public interface IVerificationCodeRepo
    {

        Task CreateAsync(VerificationCode code, CancellationToken cancellationToken);

        Task<VerificationResponse?> GetAsync(string codeId, CancellationToken cancellationToken);

        Task<IEnumerable<PendingCode>?> GetPendingCodes(CancellationToken cancellationToken);

        Task<bool> IsUserEmailVerified(string userId, CancellationToken cancellationToken);

        Task UpdateAsync(VerificationCode code, CancellationToken cancellationToken);

        Task UpdateEmailSentAsync(string codeId, CancellationToken cancellationToken);

        Task UpdateActiveStatusAsync(string codeId, CancellationToken cancellationToken);

    }

}
