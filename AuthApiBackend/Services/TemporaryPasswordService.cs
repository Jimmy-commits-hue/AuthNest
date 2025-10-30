using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Exceptions.ExceptionTypes;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Models;
using AuthApiBackend.Utilities;

namespace AuthApiBackend.Services
{

    public class TemporaryPasswordService(ITemporaryPasswordRepo passwordRepo) : ITemporaryPasswordService
    {
        
        public async Task CreateTemporaryPassword(string accountId, CancellationToken cancellationToken)
        {

            var tempPassword = new TemporaryPassword
            {
                AccountId = accountId,
                IsActive = true,
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

        public async Task<IEnumerable<ResetPasswordResponse>?> GetAllPendingPasswords(CancellationToken cancellationToken)
        {
            return await passwordRepo.GetAllPendingPasswords(cancellationToken);
        }

    }

}
