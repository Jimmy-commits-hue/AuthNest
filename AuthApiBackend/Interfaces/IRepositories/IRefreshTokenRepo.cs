using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IRepositories
{
    public interface IRefreshTokenRepo
    {
        Task CreateToken(RefreshToken refreshToken, CancellationToken cancellationToken);
    }
}
