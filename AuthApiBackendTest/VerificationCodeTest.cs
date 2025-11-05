using Moq;
using AuthApiBackend.Services;
using AuthApiBackend.Models;
using AuthApiBackend.Utilities;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Exceptions.ExceptionTypes;
using Microsoft.Extensions.Options;

namespace AuthApiBackendTest
{
    
    [Collection("Env collection")]
    public class VerificationCodeTest
    {

        private readonly Mock<IVerificationCodeRepo> codeRepo;
        private readonly VerificationCodeService service;

        public VerificationCodeTest()
        {

            codeRepo = new Mock<IVerificationCodeRepo>();
            service = new VerificationCodeService(codeRepo.Object, Options.Create(new AuthApiBackend.Configurations.MaxAttemptsConfig
                                                   {
                                                     Max = "3"
                                                   })
                                                 );

        }

        [Fact]
        public async Task CreateCodeAsync_ShouldCreateTheCode()
        {
            var fakeDb = new List<VerificationCode>();

            codeRepo.Setup(repo => repo.CreateAsync(It.IsAny<VerificationCode>(), It.IsAny<CancellationToken>())).
                     Callback<VerificationCode, CancellationToken>((verificationCode, cancellationToken) =>
                     {
                         fakeDb.Add(verificationCode);
                     }).
                     Returns(Task.CompletedTask);

            await service.CreateCodeAsync(Guid.NewGuid().ToString(), CancellationToken.None);

            Assert.Single(fakeDb);

            codeRepo.Verify(repo => repo.CreateAsync(It.IsAny<VerificationCode>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyCodeAsync_ShouldReturnAccountId_IfCodeIsValid()
        {
            string codeId = Guid.NewGuid().ToString();
            string codeValue = "123456";
            string accountId = Guid.NewGuid().ToString();

            codeRepo.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new VerificationResponse(
                                      accountId,
                                      false,
                                      EncryptData.Encrypt(codeValue))
                                 );
                        
            var result = await service.VerifyCodeAsync(codeId, codeValue, CancellationToken.None);

            Assert.Equal(accountId, result);
            
            codeRepo.Verify(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyCodeAsync_ShouldThrowException_IfCodeIsInvalid()
        {   
            string codeId = Guid.NewGuid().ToString();
            string codeValue = "123456";

            codeRepo.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     ReturnsAsync((VerificationResponse?)null);

            var ex = await Assert.ThrowsAsync<NoCodeMatchException>(async () => {
                     await service.VerifyCodeAsync(codeId, codeValue, CancellationToken.None);});

            Assert.Equal("Invalid code", ex.Message);

            codeRepo.Verify(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyCodeAsync_ShouldThrowException_IfNoCodeIdMatch()
        {
            string codeId = Guid.NewGuid().ToString();
            string codeValue = "123456";

            codeRepo.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     ReturnsAsync((VerificationResponse?)null);

            var ex = await Assert.ThrowsAsync<NoCodeMatchException>(async () => { 
                     await service.VerifyCodeAsync(codeId, codeValue, CancellationToken.None);});

            Assert.Equal("Invalid code", ex.Message);

            codeRepo.Verify(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task VerifyCodeAsync_ShouldThrowException_IfCodeIsExpired()
        {
            string codeId = Guid.NewGuid().ToString();
            string codeValue = "123456";

            codeRepo.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     ReturnsAsync(new VerificationResponse
                                      (
                                        Guid.NewGuid().ToString(),
                                        true,
                                        EncryptData.Encrypt(codeValue)
                                      )
                                 );

            var ex = await Assert.ThrowsAsync<CodeExpiredException>(async () => { 
                     await service.VerifyCodeAsync(codeId, codeValue, CancellationToken.None);});

            Assert.Equal("Code has expired, Please request for a new code", ex.Message);

            codeRepo.Verify(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            codeRepo.Verify(repo => repo.UpdateActiveStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPendingCode_ReturnsPendingCodes_IfAny()
        {
            codeRepo.Setup(repo => repo.GetPendingCodes(It.IsAny<CancellationToken>())).
                     ReturnsAsync(
                                     [
                                        new(
                                             Guid.NewGuid().ToString(),
                                             EncryptData.Encrypt("123456"),
                                             "jimmyjabulani01@gmail",
                                             "Jimmy",
                                             "Khabana"
                                           )
                                     ]
                                 );

            var result = await service.GetPendingCodeAsync(CancellationToken.None);

            #pragma warning disable CS8604
            Assert.IsType<PendingCode>(result.First());
            #pragma warning restore CS8604

            codeRepo.Verify(repo => repo.GetPendingCodes(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPendingCode_ReturnsNull_IfNoPendingCodes()
        {
            codeRepo.Setup(repo => repo.GetPendingCodes(It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<PendingCode>?)null);

            var result = await service.GetPendingCodeAsync(CancellationToken.None);

            Assert.Null(result);

            codeRepo.Verify(repo => repo.GetPendingCodes(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCodeAsync_DeactiveTheCode()
        {
            bool codeStatus = true;

            codeRepo.Setup(c => c.UpdateActiveStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     Callback<string, CancellationToken>((Id, cancellationToken) =>
                     {
                        codeStatus = false;
                     }).
                     Returns(Task.CompletedTask);

            await service.UpdateCodeAsync(Guid.NewGuid().ToString(), CancellationToken.None);

            Assert.False(codeStatus);

            codeRepo.Verify(repo => repo.UpdateActiveStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateEmailSentAsync()
        {
            bool updateEmailSent = false;

            codeRepo.Setup(c => c.UpdateEmailSentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     Callback<string, CancellationToken>((Id, cancellationToken) =>
                     {
                         updateEmailSent = true;
                     }).
                     Returns(Task.CompletedTask);

            await service.UpdateEmailSentAsync(Guid.NewGuid().ToString(), CancellationToken.None);

            Assert.True(updateEmailSent);

            codeRepo.Verify(repo => repo.UpdateEmailSentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RequestForCode_ThrowsException_IfEmailAlreadyVerified()
        {
            codeRepo.Setup(repo => repo.IsUserEmailVerified(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);

            var userResponse = new UserResponse
            (
                Guid.NewGuid().ToString(),
                1
            );

            var ex = await Assert.ThrowsAsync<EmailAlreadyVerifiedException>(async () => { 
                     await service.RequestForCode(userResponse, CancellationToken.None);});

            Assert.Equal("Email has been sent to your email", ex.Message);

            codeRepo.Verify(repo => repo.IsUserEmailVerified(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            codeRepo.Verify(c => c.GetCodeId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            codeRepo.Verify(c => c.DeactivateOldCode(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RequestForCode_IfMaxAttemptsNotExceededAndUserVerificationCodeHistoryErased()
        {
            codeRepo.Setup(repo => repo.IsUserEmailVerified(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);

            codeRepo.Setup(c => c.GetCodeId(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     ReturnsAsync((string?)null);

            var userResponse = new UserResponse
            (
                Guid.NewGuid().ToString(),
                0
            );

            await service.RequestForCode(userResponse, CancellationToken.None);

            codeRepo.Verify(c => c.IsUserEmailVerified(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            codeRepo.Verify(c => c.GetCodeId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            codeRepo.Verify(c => c.DeactivateOldCode(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RequestCode_DeactivatedOldCodes_IfThereExistOtherUserVerificationCodes()
        {
            var verificationCode = new VerificationCode { Id = Guid.NewGuid().ToString(), IsActive = true };
            codeRepo.Setup(c => c.IsUserEmailVerified(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     ReturnsAsync(false);

            codeRepo.Setup(c => c.GetCodeId(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     ReturnsAsync(Guid.NewGuid().ToString());

            codeRepo.Setup(c => c.DeactivateOldCode(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     Callback<string, CancellationToken>((Id, cancellationToken) =>
                     {
                         verificationCode.IsActive = false;
                     }).
                     Returns(Task.CompletedTask);

            var userResponse = new UserResponse
                                (
                                   Guid.NewGuid().ToString(),
                                   1
                                );
            
            await service.RequestForCode(userResponse, CancellationToken.None);

            Assert.False(verificationCode.IsActive);

            codeRepo.Verify(c => c.IsUserEmailVerified(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            codeRepo.Verify(c => c.GetCodeId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            codeRepo.Verify(c => c.DeactivateOldCode(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task RequestForCode_ThrowsException_IfMaxAttemptsExceeded()
        {
            codeRepo.Setup(repo => repo.IsUserEmailVerified(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     ReturnsAsync(false);

            var userResponse = new UserResponse
            (
                Guid.NewGuid().ToString(),
                3
            );

            var ex = await Assert.ThrowsAsync<DailyMaximumAttemptsReachedException>(async () => {
                     await service.RequestForCode(userResponse, CancellationToken.None);});

            Assert.Equal("Maximum attempt reached. Please try again later", ex.Message);

            codeRepo.Verify(c => c.IsUserEmailVerified(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            codeRepo.Verify(c => c.GetCodeId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            codeRepo.Verify(c => c.DeactivateOldCode(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ExpiredVerificationCodes_ReturnsExpiredVerificationCodes_IfAny()
        {
            codeRepo.Setup(c => c.GetExpiredVericationCodes(It.IsAny<CancellationToken>())).
                     ReturnsAsync(
                                  [
                                    new()
                                         {
                                            Id = Guid.NewGuid().ToString(),
                                         }
                                  ]
                                 );

            var expiredCodes = await service.ExpiredVerificationCodes(CancellationToken.None);

            #pragma warning disable CS8604
            Assert.IsType<VerificationCode>(expiredCodes.First());
            #pragma warning restore CS8604

            codeRepo.Verify(c => c.GetExpiredVericationCodes(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExpiredVerificationCodes_ReturnsNull_IfNone()
        {
            codeRepo.Setup(c => c.GetExpiredVericationCodes(It.IsAny<CancellationToken>())).
                     ReturnsAsync((List<VerificationCode>?)null);

            var expiredCodes = await service.ExpiredVerificationCodes(CancellationToken.None);

            Assert.Null(expiredCodes);

            codeRepo.Verify(c => c.GetExpiredVericationCodes(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RemoveCodes_PermanentlyRemoveCodes_IfAny()
        {
            var oldVerificationCode = new VerificationCode { Id = Guid.NewGuid().ToString() };

            var fakeDb = new List<VerificationCode>
            {
                oldVerificationCode
            };

            codeRepo.Setup(c => c.DeleteCodes(It.IsAny<VerificationCode>(), It.IsAny<CancellationToken>())).
                     Callback<VerificationCode, CancellationToken>((verificationCode, cancellationToken) =>
                     {
                         fakeDb.Remove(verificationCode);
                     }).
                     Returns(Task.CompletedTask);

            await service.RemoveCodes(oldVerificationCode, CancellationToken.None);

            Assert.Empty(fakeDb);

            codeRepo.Verify(c => c.DeleteCodes(It.IsAny<VerificationCode>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RetrieveUsedCodes_ReturnUsedCodes_IfAny()
        {
            codeRepo.Setup(c => c.GetAllUsedVerificationCodes(It.IsAny<CancellationToken>())).
                     ReturnsAsync(
                                    [
                                       new()
                                           {
                                              Id = Guid.NewGuid().ToString(),
                                           }
                                    ]
                                 );

            var usedCodes = await service.RetrieveUsedCodes(It.IsAny<CancellationToken>());

            #pragma warning disable CS8604
            Assert.IsType<VerificationCode>(usedCodes.First());
            #pragma warning restore CS8604

            codeRepo.Verify(c => c.GetAllUsedVerificationCodes(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RetrieveUsedCodes_ReturnNull_IfNone()
        {
            codeRepo.Setup(c => c.GetAllUsedVerificationCodes(It.IsAny<CancellationToken>())).
                     ReturnsAsync((List<VerificationCode>?)null);

            var usedCodes = await service.RetrieveUsedCodes(It.IsAny<CancellationToken>());

            Assert.Null(usedCodes);

            codeRepo.Verify(c => c.GetAllUsedVerificationCodes(It.IsAny<CancellationToken>()), Times.Once);
        }

    }

}