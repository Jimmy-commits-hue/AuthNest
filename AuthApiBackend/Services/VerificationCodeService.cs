using AuthApiBackend.Configurations;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Exceptions.ExceptionTypes;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Utilities;
using Microsoft.Extensions.Options;

namespace AuthApiBackend.Services
{

    public class VerificationCodeService : IVerificationCodeService
    {

        private readonly IVerificationCodeRepo codeRepo;
        private readonly MaxAttemptsConfig max;

        public VerificationCodeService(IVerificationCodeRepo codeRepo, IOptions<MaxAttemptsConfig> option)
        {
            this.codeRepo = codeRepo;
            max = option.Value;
        }

        public async Task CreateCodeAsync(string userId, CancellationToken cancellationToken, int attemptCount = 1)
        {

            var code = new Models.VerificationCode
            {
                EmailId = userId,
                Code = EncryptData.Encrypt(GenerateCode.GenerateVerificationCode()),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds(),
                AttemptCount = attemptCount,
                IsActive = true,
            };

            await codeRepo.CreateAsync(code, cancellationToken);

        }

        public async Task<string> VerifyCodeAsync(string codeId, string code, CancellationToken cancellationToken)
        {

            VerificationResponse? existingCode = await codeRepo.GetAsync(codeId, cancellationToken);
            
            if (existingCode is null || EncryptData.Decrypt(existingCode.Code) != code)
            {
                throw new NoCodeMatchException("Invalid code");
            }
            if (existingCode.IsExpired == true)
            {
                await UpdateCodeAsync(codeId, cancellationToken);

                throw new CodeExpiredException("Code has expired, Please request for a new code");
            }

            return existingCode.UserId;

        }

        public async Task<IEnumerable<PendingCode>?> GetPendingCodeAsync(CancellationToken cancellationToken)
        {
            return await codeRepo.GetPendingCodes(cancellationToken);
        }

        public async Task RequestForCode(UserResponse userAttemptsAndUserId, CancellationToken cancellationToken)
        {

            var result = await codeRepo.IsUserEmailVerified(userAttemptsAndUserId.UserId, cancellationToken);

            if (result == true)
            {
                throw new EmailAlreadyVerifiedException("Email has been sent to your email");
            }

            int attemptCount = userAttemptsAndUserId.AttemptCount + 1;

            if (attemptCount > int.Parse(max.MaxAttempts))
            {
               throw new DailyMaximumAttemptsReachedException("Maximum attempt reached. Please try again later");
            }

            await CreateCodeAsync(userAttemptsAndUserId.UserId, cancellationToken, attemptCount);
            
        }

        public async Task UpdateCodeAsync(string codeId, CancellationToken cancellationToken)
        {
            await codeRepo.UpdateActiveStatusAsync(codeId, cancellationToken);
        }

        public async Task UpdateEmailSentAsync(string codeId, CancellationToken cancellationToken)
        { 
            await codeRepo.UpdateEmailSentAsync(codeId, cancellationToken);
        }

    }

}