using Moq;
using Xunit;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Models;
using AuthApiBackend.Services;
using MimeKit.Cryptography;
using AuthApiBackend.Interfaces.IServices;

namespace AuthApiBackendTest
{
    public class BlackListedTokenTest
    {

        private readonly Mock<IBlackListedTokenRepo> tokenRepo;
        private readonly IBlackListedTokenService tokenService;

        public BlackListedTokenTest()
        {
            tokenRepo = new Mock<IBlackListedTokenRepo>();
            tokenService = new BlackListedTokenService(tokenRepo.Object);
        }

        [Fact]
        public async Task AddToken_Should_Call_Repo_AddToken_Once()
        {
            // Arrange

            var fakeDb = new List<BlackListedToken>();

            tokenRepo.Setup(repo => repo.AddToken(It.IsAny<BlackListedToken>(), It.IsAny<CancellationToken>())).
                      Callback<BlackListedToken, CancellationToken>((token, cancellationToken) =>
                      {
                          fakeDb.Add(token);
                      })
                     .Returns(Task.CompletedTask);

            await tokenService.AddBlackListedToken(Guid.NewGuid().ToString(), DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds()
                                                  , CancellationToken.None);

            Assert.Single(fakeDb);

            tokenRepo.Verify(repo => repo.AddToken(It.IsAny<BlackListedToken>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task IsBlackListed_Should_Return_True_If_Token_Exists()
        {
            var BlackListedTokenId = new BlackListedToken
            {
                Id = Guid.NewGuid().ToString(),
                TokenId = Guid.NewGuid().ToString(),
                ExpiresIn = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds()
            };

            var fakeDb = new List<BlackListedToken> { BlackListedTokenId };

            tokenRepo.Setup(c => c.IsBlackListed(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((string tokenId, CancellationToken cancellationToken) =>
                     {
                         return fakeDb.Any(t => t.TokenId == tokenId);
                     });

            await tokenService.TokenExist(BlackListedTokenId.TokenId, CancellationToken.None);

            tokenRepo.Verify(repo => repo.IsBlackListed(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetExpiredTokens_ShouldReturnExpiredTokens_IfAny()
        {
            var expiredToken = new BlackListedToken
            {
                Id = Guid.NewGuid().ToString(),
                TokenId = Guid.NewGuid().ToString(),
                ExpiresIn = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeSeconds()
            };
            var validToken = new BlackListedToken
            {
                Id = Guid.NewGuid().ToString(),
                TokenId = Guid.NewGuid().ToString(),
                ExpiresIn = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds()
            };

            var fakeDb = new List<BlackListedToken> { expiredToken, validToken };

            tokenRepo.Setup(c => c.GetAllExpiredBlackListedTokens(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(() =>
                     {
                         return fakeDb.Where(t => t.ExpiresIn < DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                     });

            var result = await tokenService.GetExpiredTokens(CancellationToken.None);

            Assert.NotNull(result);
            Assert.Contains(expiredToken, result);

            tokenRepo.Verify(repo => repo.GetAllExpiredBlackListedTokens(It.IsAny<CancellationToken>()), Times.Once);
        }
        [Fact]
        public async Task GetExpiredTokens_ShouldReturnEmpty_IfNoExpiredTokens()
        {
            var validToken1 = new BlackListedToken
            {
                Id = Guid.NewGuid().ToString(),
                TokenId = Guid.NewGuid().ToString(),
                ExpiresIn = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds()
            };

            var validToken2 = new BlackListedToken
            {
                Id = Guid.NewGuid().ToString(),
                TokenId = Guid.NewGuid().ToString(),
                ExpiresIn = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds()
            };

            var fakeDb = new List<BlackListedToken> { validToken1, validToken2 };

            tokenRepo.Setup(c => c.GetAllExpiredBlackListedTokens(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(() =>
                     {
                         return fakeDb.Where(t => t.ExpiresIn < DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                     });

            var result = await tokenService.GetExpiredTokens(CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);

            tokenRepo.Verify(repo => repo.GetAllExpiredBlackListedTokens(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RemoveToken_Should_Call_Repo_RemoveTokens_Once()
        {
            // Arrange
            var fakeDb = new List<BlackListedToken>();

            var tokenToRemove = new BlackListedToken
            {
                Id = Guid.NewGuid().ToString(),
                TokenId = Guid.NewGuid().ToString(),
                ExpiresIn = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds()
            };
            fakeDb.Add(tokenToRemove);

            tokenRepo.Setup(repo => repo.RemoveTokens(It.IsAny<BlackListedToken>(), It.IsAny<CancellationToken>())).
                      Callback<BlackListedToken, CancellationToken>((token, cancellationToken) =>
                      {
                          fakeDb.Remove(token);
                      })
                     .Returns(Task.CompletedTask);

            await tokenService.RemoveToken(tokenToRemove, CancellationToken.None);

            Assert.Empty(fakeDb);

            tokenRepo.Verify(repo => repo.RemoveTokens(It.IsAny<BlackListedToken>(), It.IsAny<CancellationToken>()), Times.Once);
        }

    }

}
