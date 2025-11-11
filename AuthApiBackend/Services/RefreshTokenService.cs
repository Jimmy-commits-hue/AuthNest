using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Models;
using AuthApiBackend.Utilities;

namespace AuthApiBackend.Services
{

    public class RefreshTokenService(IRefreshTokenRepo refreshTokenRepo) : IRefreshTokenService
    {

        public async Task CreateRefreshToken(string accountId, string token, CancellationToken cancellationToken)
        {
            await refreshTokenRepo.CreateToken(new RefreshToken
            {
                AccountId = accountId,
                ExpiresAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Id = Guid.NewGuid().ToString(),
                Token = HashHelper.HashId(token)
            }, cancellationToken);
        }

        public async Task<RefreshToken> GetRefreshToken(string token, CancellationToken cancellationToken)
        {
            var encryptedToken = HashHelper.HashId(token);
            return await refreshTokenRepo.GetToken(encryptedToken, cancellationToken);
        }

        public async Task DeleteRefreshToken(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            await refreshTokenRepo.DeleteToken(refreshToken, cancellationToken);
        }

    }

}