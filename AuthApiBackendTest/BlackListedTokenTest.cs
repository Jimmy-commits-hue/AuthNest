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
                                                  ,CancellationToken.None);

            Assert.Single(fakeDb);

            tokenRepo.Verify(repo => repo.AddToken(It.IsAny<BlackListedToken>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task IsBlackListed_Should_Return_True_If_Token_Exists()
        {
            var BlackListedTokenId = new BlackListedToken { Id = Guid.NewGuid().ToString(), TokenId = Guid.NewGuid().ToString(),
                                                            ExpiresIn = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds()};

            var fakeDb = new List<BlackListedToken> { BlackListedTokenId };

            tokenRepo.Setup(c => c.IsBlackListed(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((string tokenId, CancellationToken cancellationToken) =>
                     {
                         return fakeDb.Any(t => t.TokenId == tokenId);
                     });

            await tokenService.TokenExist(BlackListedTokenId.TokenId, CancellationToken.None);

            tokenRepo.Verify(repo => repo.IsBlackListed(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
