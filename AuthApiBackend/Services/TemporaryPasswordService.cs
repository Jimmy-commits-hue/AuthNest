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

    public class TemporaryPasswordService : ITemporaryPasswordService
    {

        private readonly ITemporaryPasswordRepo passwordRepo;
        private readonly MaxAttemptsConfig max;

        public TemporaryPasswordService(ITemporaryPasswordRepo passwordRepo, IOptions<MaxAttemptsConfig> option)
        {
            this.passwordRepo = passwordRepo;
            max = option.Value;
        }

        public async Task CreateTemporaryPassword(string accountId, int attemptCount, CancellationToken cancellationToken)
        {
            var tempPassword = new TemporaryPassword
            {
                AccountId = accountId,
                IsActive = true,
                AttemptCount = attemptCount,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds(),
                HashedPassword = EncryptData.Encrypt(GenerateCode.TemporaryPassword())
            };

            await passwordRepo.CreatePassword(tempPassword, cancellationToken);
        }

        public async Task<string> VerifyPassword(string tempPasswordId, string password, CancellationToken cancellationToken)
        {
            var storedPassword = await passwordRepo.GetPassword(tempPasswordId, cancellationToken);

            if(password != EncryptData.Decrypt(storedPassword.password))
            {
                throw new InvalidTempPassword("Incorrect temp password");
            }

            return storedPassword.accountId;
        }

        public async Task UpdatePasswordStatus(string tempPasswordId, CancellationToken cancellationToken)
        {
            await passwordRepo.UpdateStatus(tempPasswordId, cancellationToken);
        }

        public async Task<int> NumberOfPendingPasswords(CancellationToken cancellationToken)
        {
            return await passwordRepo.NumberOfPendingPasswords(cancellationToken);
        }

        public async Task<IEnumerable<ResetPasswordResponse>?> GetAllPendingPasswords(int round, CancellationToken cancellationToken)
        {
            return await passwordRepo.GetAllPendingPasswords(round, cancellationToken);
        }

        public async Task<int> CheckAttemptNumber(string accountId, CancellationToken cancellationToken)
        {
            var attemptCount = await passwordRepo.GetAttemptCount(accountId, cancellationToken);

            if(attemptCount == 0)
            {
                return 1;
            }

            if(attemptCount > 0 && attemptCount < int.Parse(max.Max))
            {
                string tempPassId = await passwordRepo.GetTempCodeId(accountId, cancellationToken);

                await passwordRepo.DeactivateOldCode(tempPassId, cancellationToken);
            }

            if(attemptCount > int.Parse(max.Max))
            {
                throw new DailyMaximumAttemptsReachedException("Daily permitted maximum request attempts reached");
            }

            return attemptCount + 1;
        }

        public async Task<int> NumberOfExpiredTempCodes(CancellationToken cancellationToken)
        {
            return await passwordRepo.CountExpiredTempCodes(cancellationToken);
        }

        public async Task<IEnumerable<TemporaryPassword>?> RetrieveExpiredCodes(int rounds, CancellationToken cancellationToken)
        {
            return await passwordRepo.GetExpiredCodes(rounds,cancellationToken);
        }

        public async Task<int> NumberOfUsedCodes(CancellationToken cancellationToken)
        {
            return await passwordRepo.CountUsedCodes(cancellationToken);
        }

        public async Task<IEnumerable<TemporaryPassword>?> RetrieveUsedCodes(int round, CancellationToken cancellationToken)
        {
            return await passwordRepo.GetUsedCodes(round, cancellationToken);
        }

        public async Task RemoveCodes(TemporaryPassword code, CancellationToken cancellationToken)
        {
           await passwordRepo.DeleteCodes(code, cancellationToken);
        }

    }

}