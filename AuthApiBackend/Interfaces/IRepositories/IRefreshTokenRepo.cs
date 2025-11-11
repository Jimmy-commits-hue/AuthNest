using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IRepositories
{
    public interface IRefreshTokenRepo
    {
        Task CreateToken(RefreshToken refreshToken, CancellationToken cancellationToken);

        Task<RefreshToken> GetToken(string token, CancellationToken cancellationToken);

        Task DeleteToken(RefreshToken token, CancellationToken cancellationToken);
    }
}
