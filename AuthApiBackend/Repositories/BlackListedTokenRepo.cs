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
    }

}
