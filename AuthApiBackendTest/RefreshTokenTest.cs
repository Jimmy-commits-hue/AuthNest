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
    }
}
