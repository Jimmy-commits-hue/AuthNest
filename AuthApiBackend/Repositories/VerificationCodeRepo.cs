using AuthApiBackend.Database;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApiBackend.Repositories
{

    public class VerificationCodeRepo : IVerificationCodeRepo
    {

        private readonly AuthApiDbContext db;

        public VerificationCodeRepo(AuthApiDbContext db)
        {
            this.db = db;
        }

        public async Task CreateAsync(VerificationCode code, CancellationToken cancellationToken)
        {
            db.VerificationCode.Add(code);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<VerificationResponse?> GetAsync(string codeId, CancellationToken cancellationToken)
        {
            return await db.VerificationCode.AsNoTracking().
                         Where(c => c.Id == codeId).
                         OrderByDescending(c => c.AttemptCount).
                         Select(c => new VerificationResponse(
                                     c.ContactDetails.User.Id,
                                     c.IsExpired,
                                     c.Code)).
                         FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<PendingCode>?> GetPendingCodes(CancellationToken cancellationToken)
        {
            return await db.VerificationCode.AsNoTracking().
                         Where(v => v.IsActive == true && !v.IsEmailSent).
                         Select(v => new PendingCode(
                                     v.Id, 
                                     v.Code,
                                     v.ContactDetails.Email, 
                                     v.ContactDetails.User.FirstName, 
                                     v.ContactDetails.User.Surname)).
                         ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(VerificationCode code, CancellationToken cancellationToken)
        {
            db.VerificationCode.Update(code);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> IsUserEmailVerified(string userId, CancellationToken cancellationToken)
        {
            return await db.VerificationCode.
                         Where(c => c.ContactDetails.User.Id == userId).
                         Select(c => c.ContactDetails.IsEmailVerified).
                         FirstOrDefaultAsync(cancellationToken);
        }

        public async Task UpdateEmailSentAsync(string codeId, CancellationToken cancellationToken)
        {
            var code = new VerificationCode { Id = codeId };

            db.VerificationCode.Attach(code);

            code.IsEmailSent = true;

            db.Entry(code).Property(c => c.IsEmailSent).IsModified = true;

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateActiveStatusAsync(string codeId, CancellationToken cancellationToken)
        {
            var code = new VerificationCode { Id = codeId };

            db.VerificationCode.Attach(code);

            code.IsActive = false;

            db.Entry(code).Property(c => c.IsActive).IsModified = true;

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<VerificationCode>?> GetExpiredVericationCodes(CancellationToken cancellationToken)
        {
            long dateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            return await db.VerificationCode.AsNoTracking().
                   Where(c => c.IsActive == true && c.ExpiresAt < DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToListAsync(cancellationToken);
        }

        public async Task DeleteCodes(VerificationCode code, CancellationToken cancellationToken)
        {
            db.VerificationCode.Remove(code);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<string?> GetCodeId(string accountId, CancellationToken cancellationToken)
        {
            return await db.VerificationCode.AsNoTracking().
                         Where(u => u.EmailId == accountId).
                         OrderByDescending(u => u.AttemptCount).
                         Select(u => u.Id).
                         FirstOrDefaultAsync(cancellationToken);
        }

        public async Task DeactivateOldCode(string code, CancellationToken cancellationToken)
        {
            var verificationCode = new VerificationCode { Id = code };

            db.VerificationCode.Attach(verificationCode);

            verificationCode.IsActive = false;

            db.Entry(verificationCode).Property(u => u.IsActive).IsModified = true;

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<VerificationCode>?> GetAllUsedVerificationCodes(CancellationToken cancellationToken)
        {
            return await db.VerificationCode.AsNoTracking().
                         Where(c => c.IsActive == false).
                         Select(u => u).
                         ToListAsync(cancellationToken);
        }
    }

}