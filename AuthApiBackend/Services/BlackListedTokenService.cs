using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Interfaces.IServices;

namespace AuthApiBackend.Services
{
    public class BlackListedTokenService(IBlackListedTokenRepo blackListedTokenRepo) : IBlackListedTokenService
    {

        private readonly IBlackListedTokenRepo blackListedToken = blackListedTokenRepo;

        public async Task AddBlackListedToken(string tokenId, long expiresAt, CancellationToken cancellationToken)
        {
            await blackListedToken.AddToken(new Models.BlackListedToken
            {
                ExpiresIn = expiresAt,
                Id = Guid.NewGuid().ToString(),
                TokenId = tokenId
            }, cancellationToken);

        }

        public async Task<bool> TokenExist(string tokenId, CancellationToken cancellationToken)
        {
            return await blackListedToken.IsBlackListed(tokenId, cancellationToken);
        }

    }

}