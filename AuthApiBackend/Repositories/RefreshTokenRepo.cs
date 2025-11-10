using AuthApiBackend.Database;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Models;

namespace AuthApiBackend.Repositories
{

    public class RefreshTokenRepo(AuthApiDbContext db) : IRefreshTokenRepo
    {

        public async Task CreateToken(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            db.RefreshToken.Add(refreshToken);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

}
