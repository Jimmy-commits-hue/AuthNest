using Moq;
using AuthApiBackend.Services;
using AuthApiBackend.Models;
using AuthApiBackend.Utilities;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.DTOs.ResponseDtos;
using Microsoft.Extensions.Configuration;
using AuthApiBackend.Exceptions.ExceptionTypes;

namespace AuthApiBackendTest
{
    public class VerificationCodeTest
    {

        private readonly Mock<IVerificationCodeRepo> codeRepo;
        private readonly Mock<IConfiguration> config;
        private readonly VerificationCodeService service;

        public VerificationCodeTest()
        {
            codeRepo = new Mock<IVerificationCodeRepo>();
            config = new Mock<IConfiguration>();
            service = new VerificationCodeService(codeRepo.Object, config.Object);
        }

        [Fact]
        public async Task CreateCodeAsync_ShouldCreateTheCode()
        {
            // Arrange
            var code = new VerificationCode
            {
                EmailId = Guid.NewGuid().ToString(),
                Code = GenerateCode.GenerateVerificationCode(),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds(),
                IsActive = true,
            };

            codeRepo.Setup(repo => repo.CreateAsync(code, CancellationToken.None))
                    .Returns(Task.CompletedTask).Verifiable();

            await service.CreateCodeAsync(code.EmailId, CancellationToken.None);

            codeRepo.Verify(repo => repo.CreateAsync(It.Is<VerificationCode>(c => c.EmailId == code.EmailId),
                     It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyCodeAsync_ShouldReturnUserId_IfCodeIsValid()
        {
            // Arrange
            string codeId = Guid.NewGuid().ToString();
            string codeValue = "123456";
            string userId = Guid.NewGuid().ToString();

            codeRepo.Setup(repo => repo.GetAsync(codeId, CancellationToken.None))
                    .ReturnsAsync(new VerificationResponse
                    {
                        Code = codeValue,
                        IsExpired = false,
                        UserId = userId
                    });

            var result = await service.VerifyCodeAsync(codeId, codeValue, CancellationToken.None);

            Assert.Equal(userId, result);

            codeRepo.Verify(repo => repo.GetAsync(codeId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyCodeAsync_ShouldThrowException_IfCodeIsInvalid()
        {
            // Arrange
            string codeId = Guid.NewGuid().ToString();
            string codeValue = "123456";

            codeRepo.Setup(repo => repo.GetAsync(codeId, CancellationToken.None))
                    .ReturnsAsync((VerificationResponse?)null);

            var ex = await Assert.ThrowsAsync<NoCodeMatchException>(async () =>
            {
                await service.VerifyCodeAsync(codeId, codeValue, CancellationToken.None);
            });

            Assert.Equal("Invalid code", ex.Message);

            codeRepo.Verify(repo => repo.GetAsync(codeId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyCodeAsync_ShouldThrowException_IfNoCodeIdMatch()
        {
            // Arrange
            string codeId = Guid.NewGuid().ToString();
            string codeValue = "123456";

            codeRepo.Setup(repo => repo.GetAsync(codeId, It.IsAny<CancellationToken>()))
                  .ReturnsAsync((VerificationResponse?)null);

            var ex = await Assert.ThrowsAsync<NoCodeMatchException>(async () =>
            {
                await service.VerifyCodeAsync(codeId, codeValue, CancellationToken.None);
            });

            Assert.Equal("Invalid code", ex.Message);
            codeRepo.Verify(repo => repo.GetAsync(codeId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyCodeAsync_ShouldThrowException_IfCodeIsExpired()
        {
            // Arrange
            string codeId = Guid.NewGuid().ToString();
            string codeValue = "123456";

            codeRepo.Setup(repo => repo.GetAsync(codeId, CancellationToken.None))
                    .ReturnsAsync(new VerificationResponse
                    {
                        Code = codeValue,
                        IsExpired = true,
                        UserId = Guid.NewGuid().ToString()
                    });

            var ex = await Assert.ThrowsAsync<CodeExpiredException>(async () =>
            {
                await service.VerifyCodeAsync(codeId, codeValue, CancellationToken.None);
            });

            Assert.Equal("Code has expired, Please request for a new code", ex.Message);

            codeRepo.Verify(repo => repo.GetAsync(codeId, It.IsAny<CancellationToken>()), Times.Once);
            codeRepo.Verify(repo => repo.UpdateActiveStatusAsync(codeId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPendingCode_ReturnsPendingCodes_IfAny()
        {
            codeRepo.Setup(repo => repo.GetPendingCodes(CancellationToken.None))
                    .ReturnsAsync(new List<PendingCode>
                    {
                        new PendingCode { Id = Guid.NewGuid().ToString(), Code = "123456", Email = "jimmyjabulani01@gmail",
                            FirstName = "Jimmy", Surname = "Khabana" }
                    });

            var result = await service.GetPendingCodeAsync(CancellationToken.None);

            Assert.IsType<List<PendingCode>>(result);

            codeRepo.Verify(repo => repo.GetPendingCodes(It.IsAny<CancellationToken>()), Times.Once);

        }

        [Fact]
        public async Task GetPendingCode_ReturnsNull_IfNoPendingCodes()
        {

            codeRepo.Setup(repo => repo.GetPendingCodes(CancellationToken.None))
                    .ReturnsAsync((IEnumerable<PendingCode>?)null);

            var result = await service.GetPendingCodeAsync(CancellationToken.None);

            Assert.Null(result);

            codeRepo.Verify(repo => repo.GetPendingCodes(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCodeAsync()
        {

            await service.UpdateCodeAsync(Guid.NewGuid().ToString(), CancellationToken.None);

            codeRepo.Verify(repo => repo.UpdateActiveStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateEmailSentAsync()
        {
            await service.UpdateEmailSentAsync(Guid.NewGuid().ToString(), CancellationToken.None);

            codeRepo.Verify(repo => repo.UpdateEmailSentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

       

        [Fact]
        public async Task RequestForCode_ThrowsException_IfEmailAlreadyVerified()
        {

            codeRepo.Setup(repo => repo.IsUserEmailVerified(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);

            var userResponse = new UserResponse
            {
                UserId = Guid.NewGuid().ToString(),
                AttemptCount = 1
            };

            var ex = await Assert.ThrowsAsync<EmailAlreadyVerifiedException>(async () =>
            {
                await service.RequestForCode(userResponse, CancellationToken.None);
            });

            Assert.Equal("Email has been sent to your email", ex.Message);

            config.VerifyGet(c => c[It.Is<string>(s => s == "MaxAttempts:Max")], Times.Never);
            codeRepo.Verify(repo => repo.IsUserEmailVerified(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

        }

        [Fact]
        public async Task RequestForCode_IfMaxAttemptsNotExceeded()
        {

            codeRepo.Setup(repo => repo.IsUserEmailVerified(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);

            config.SetupGet(c => c[It.Is<string>(s => s == "MaxAttempts:Max")])
                      .Returns("3");

            var userResponse = new UserResponse
            {
                UserId = Guid.NewGuid().ToString(),
                AttemptCount = 1
            };

            await service.RequestForCode(userResponse, CancellationToken.None);

            config.VerifyGet(c => c[It.Is<string>(s => s == "MaxAttempts:Max")], Times.Once);
        }

        [Fact]
        public async Task RequestForCode_ThrowsException_IfMaxAttemptsExceeded()
        {

             codeRepo.Setup(repo => repo.IsUserEmailVerified(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);

            config.SetupGet(c => c[It.Is<string>(s => s == "MaxAttempts:Max")])
                      .Returns("3");

            var userResponse = new UserResponse
            {
                UserId = Guid.NewGuid().ToString(),
                AttemptCount = 3
            };

            var ex = await Assert.ThrowsAsync<DailyMaximumAttemptsReachedException>(async () =>
            {
                await service.RequestForCode(userResponse, CancellationToken.None);
            });

            Assert.Equal("Maximum attempt reached. Please try again later", ex.Message);

            config.VerifyGet(c => c[It.Is<string>(s => s == "MaxAttempts:Max")], Times.Once);
        }

        [Fact]
        public async Task RequestForCode_ThrowsException_IfMaAttemptsExceeded()
        {

            codeRepo.Setup(repo => repo.IsUserEmailVerified(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

            config.SetupGet(c => c[It.Is<string>(s => s == "MaxAttempts:Max")])
                      .Returns("3");

            var userResponse = new UserResponse
            {
                UserId = Guid.NewGuid().ToString(),
                AttemptCount = 3
            };

            var ex = await Assert.ThrowsAsync<DailyMaximumAttemptsReachedException>(async () =>
            {
                await service.RequestForCode(userResponse, CancellationToken.None);
            });

            Assert.Equal("Maximum attempt reached. Please try again later", ex.Message);

            config.VerifyGet(c => c[It.Is<string>(s => s == "MaxAttempts:Max")], Times.Once);
        }

    }
}
