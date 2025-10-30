using AuthApiBackend.DTOs.ResponseDtos;

namespace AuthApiBackend.Interfaces.IServices
{
    public interface ITemporaryPasswordService
    {
        Task CreateTemporaryPassword(string accountId, CancellationToken cancellationToken);

        Task<string> VerifyPassword(string accountId, string password, CancellationToken cancellationToken);

        Task UpdatePasswordStatus(string accountId, CancellationToken cancellationToken);

        Task<IEnumerable<ResetPasswordResponse>?> GetAllPendingPasswords(CancellationToken cancellationToken);
    }
}
