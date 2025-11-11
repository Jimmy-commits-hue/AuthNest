using AuthApiBackend.Models;

namespace AuthApiBackend.Interfaces.IRepositories
{
    public interface IBlackListedTokenRepo
    {

        Task AddToken(BlackListedToken blackListedToken, CancellationToken cancellationToken);

        Task<bool> IsBlackListed(string tokenId, CancellationToken cancellationToken);

        Task<IEnumerable<BlackListedToken>?> GetAllExpiredBlackListedTokens(CancellationToken cancellationToken);

        Task RemoveTokens(BlackListedToken token, CancellationToken cancellationToken);
    }
}
