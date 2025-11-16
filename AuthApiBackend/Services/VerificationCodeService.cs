using AuthApiBackend.Configurations;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Exceptions.ExceptionTypes;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Models;
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

        public async Task CreateCodeAsync(string accountId, CancellationToken cancellationToken, int attemptCount = 1)
        { 
            var code = new VerificationCode
            {
                EmailId = accountId,
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

        public async Task<int> NumberOfPendingCodes(CancellationToken cancellationToken)
        {
            return await codeRepo.NumberOfPendingCodes(cancellationToken);
        }

        public async Task<IEnumerable<PendingCode>?> GetPendingCodeAsync(int round, CancellationToken cancellationToken)
        {
            return await codeRepo.GetPendingCodes(round,cancellationToken);
        }

        public async Task RequestForCode(UserResponse userAttemptsAndUserId, CancellationToken cancellationToken)
        {

            var result = await codeRepo.IsUserEmailVerified(userAttemptsAndUserId.UserId, cancellationToken);

            if (result == true)
            {
                throw new EmailAlreadyVerifiedException("Email has been sent to your email");
            }

            int attemptCount = userAttemptsAndUserId.AttemptCount + 1;

            if (attemptCount > int.Parse(max.Max))
            {
                throw new DailyMaximumAttemptsReachedException("Maximum attempt reached. Please try again later");
            }

            string? codeId = await codeRepo.GetCodeId(userAttemptsAndUserId.UserId, cancellationToken);

            if(userAttemptsAndUserId.AttemptCount < int.Parse(max.Max) && codeId != null)
            {
                await codeRepo.DeactivateOldCode(codeId, cancellationToken);
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

        public async Task<int> NumberOfExpiredCodes(CancellationToken cancellationToken)
        {
            return await codeRepo.NumberOfExpiredCodes(cancellationToken);
        }

        public async Task<IEnumerable<VerificationCode>?> ExpiredVerificationCodes(int round, CancellationToken cancellationToken)
        {
            return await codeRepo.GetExpiredVericationCodes(round, cancellationToken);
        }

        public async Task RemoveCodes(VerificationCode code, CancellationToken cancellationToken)
        {
            await codeRepo.DeleteCodes(code, cancellationToken);
        }


        public async Task<int> NumberOfUsedCodes(CancellationToken cancellationToken)
        {
            return await codeRepo.NumberOfUsedVerificationCodes(cancellationToken);
        }

        public async Task<IEnumerable<VerificationCode>?> RetrieveUsedCodes(int round, CancellationToken cancellationToken)
        {
            return await codeRepo.GetAllUsedVerificationCodes(round, cancellationToken);
        }

    }

}