using Moq;
using Xunit;
using AuthApiBackend.Models;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using AuthApiBackend.Utilities;

namespace AuthApiBackendTest
{
    public class RefreshTokenTest
    {

        private readonly Mock<IRefreshTokenRepo> tokenRepo;
        private readonly IRefreshTokenService tokenService;

        public RefreshTokenTest()
        {
            tokenRepo = new Mock<IRefreshTokenRepo>();
            tokenService = new RefreshTokenService(tokenRepo.Object);
        }

        [Fact]
        public async Task CreateRefreshToken_Should_Call_Repo_CreateToken_Once()
        {
            var accountId = Guid.NewGuid().ToString();
            var token = GenerateCode.GenerateRetreshToken();

            var fakeDb = new List<RefreshToken>();

            tokenRepo.Setup(repo => repo.CreateToken(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
                     .Callback<RefreshToken, CancellationToken>((refreshToken, cancellationToken) =>
                     {
                         fakeDb.Add(refreshToken);
                     })
                     .Returns(Task.CompletedTask);

            await tokenService.CreateRefreshToken(accountId, token, CancellationToken.None);

            Assert.Single(fakeDb);

            tokenRepo.Verify(repo => repo.CreateToken(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetRefreshToken_Should_Call_Repo_GetToken_Once_And_Return_Token()
        {
            var token = GenerateCode.GenerateRetreshToken();
            var hashedToken = HashHelper.HashId(token);

            var expectedRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid().ToString(),
                AccountId = Guid.NewGuid().ToString(),
                Token = hashedToken,
                ExpiresAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            tokenRepo.Setup(repo => repo.GetToken(hashedToken, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(expectedRefreshToken);

            var result = await tokenService.GetRefreshToken(token, CancellationToken.None);

            Assert.Equal(expectedRefreshToken, result);

            tokenRepo.Verify(repo => repo.GetToken(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteRefreshToken_Should_Call_Repo_DeleteToken_Once()
        {
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid().ToString(),
                AccountId = Guid.NewGuid().ToString(),
                Token = EncryptData.Encrypt(GenerateCode.GenerateRetreshToken()),
                ExpiresAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            var fakeDb = new List<RefreshToken> { refreshToken };

            tokenRepo.Setup(repo => repo.DeleteToken(refreshToken, It.IsAny<CancellationToken>())).
                      Callback<RefreshToken, CancellationToken>((token, cancellationToken) =>
                      {
                          fakeDb.Remove(token);
                      })
                     .Returns(Task.CompletedTask);

            await tokenService.DeleteRefreshToken(refreshToken, CancellationToken.None);

            Assert.Empty(fakeDb);

            tokenRepo.Verify(repo => repo.DeleteToken(refreshToken, It.IsAny<CancellationToken>()), Times.Once);
        }

    }

}