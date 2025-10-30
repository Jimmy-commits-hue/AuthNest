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

    }

}
