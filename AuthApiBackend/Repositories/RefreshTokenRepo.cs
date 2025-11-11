using AuthApiBackend.Database;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApiBackend.Repositories
{

    public class RefreshTokenRepo(AuthApiDbContext context) : IRefreshTokenRepo
    {

        private readonly AuthApiDbContext db = context;

        public async Task CreateToken(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            db.RefreshToken.Add(refreshToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<RefreshToken> GetToken(string token, CancellationToken cancellationToken)
        {
            return await db.RefreshToken.AsNoTracking().
                         Where(c => c.Token == token).
                         FirstAsync(cancellationToken);
        }

        public async Task DeleteToken(RefreshToken token, CancellationToken cancellationToken)
        {
            db.RefreshToken.Remove(token);
            await db.SaveChangesAsync(cancellationToken);
        }


    }

}