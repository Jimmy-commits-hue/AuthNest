using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IServices
{
    public interface IRefreshTokenService
    {
        Task CreateRefreshToken(string accountId, string token, CancellationToken cancellationToken);

        Task<RefreshToken> GetRefreshToken(string token, CancellationToken cancellationToken);

        Task DeleteRefreshToken(RefreshToken refreshToken, CancellationToken cancellationToken);
    }
}
