namespace AuthApiBackend.Interfaces.IServices
{
    public interface IBlackListedTokenService
    {
        Task AddBlackListedToken(string tokenId, long expiresAt, CancellationToken cancellationToken);
        Task<bool> TokenExist(string tokenId, CancellationToken cancellationToken);
    }
}
