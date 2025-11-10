namespace AuthApiBackend.Interfaces.IServices
{
    public interface IRefreshTokenService
    {
        Task CreateRefreshToken(string accountId, string token, CancellationToken cancellationToken);
    }
}
