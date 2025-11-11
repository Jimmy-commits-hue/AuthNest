using AuthApiBackend.Database;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApiBackend.Repositories
{

    public class BlackListedTokenRepo(AuthApiDbContext db) : IBlackListedTokenRepo
    {

        public async Task AddToken(BlackListedToken blackListedToken, CancellationToken cancellationToken)
        {
            await db.BlackListedToken.AddAsync(blackListedToken, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> IsBlackListed(string tokenId, CancellationToken cancellationToken)
        {
            return await db.BlackListedToken.AsNoTracking().
                         AnyAsync(c => c.TokenId == tokenId, cancellationToken);
        }

        public async Task<IEnumerable<BlackListedToken>?> GetAllExpiredBlackListedTokens(CancellationToken cancellationToken)
        {
            return await db.BlackListedToken.AsNoTracking().
                         Where(c => c.ExpiresIn < DateTimeOffset.UtcNow.ToUnixTimeSeconds()).
                         ToListAsync(cancellationToken);
        }

        public async Task RemoveTokens(BlackListedToken token, CancellationToken cancellationToken)
        {
            db.BlackListedToken.Remove(token);
            await db.SaveChangesAsync(cancellationToken);
        }

    }

}