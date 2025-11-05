using AuthApiBackend.Database;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApiBackend.Repositories
{
    public class TemporaryPasswordRepo(AuthApiDbContext db) : ITemporaryPasswordRepo
    {

        public async Task CreatePassword(TemporaryPassword temp, CancellationToken cancellationToken)
        {
            db.TemporaryPassword.Add(temp);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<PasswordResetResponse> GetPassword(string tempPassowrdId, CancellationToken cancellationToken)
        {
           return await db.TemporaryPassword.AsNoTracking().
                        Where(u => u.Id == tempPassowrdId).
                        Select(p => new PasswordResetResponse(
                                    p.HashedPassword,
                                    p.AccountId)).
                        FirstAsync(cancellationToken);
        }  

        public async Task UpdateStatus(string tempPasswordId, CancellationToken cancellationToken)
        {
            var tempPassword = new TemporaryPassword { Id = tempPasswordId };

            db.TemporaryPassword.Attach(tempPassword);

            tempPassword.IsActive = false;
            tempPassword.IsEmailSent = true;

            db.Entry(tempPassword).Property(status => status.IsActive).IsModified = true;
            db.Entry(tempPassword).Property(email => email.IsEmailSent).IsModified = true;

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<ResetPasswordResponse>?> GetAllPendingPasswords(CancellationToken cancellationToken)
        {
            return await db.TemporaryPassword.AsNoTracking().
                         Where(u => u.IsActive == true && u.IsEmailSent == false).
                         Select(p => new ResetPasswordResponse(
                                     p.Account.User.ContactDetails!.Email,
                                     p.HashedPassword,
                                     p.Id)).
                         ToListAsync(cancellationToken);
        }

        public async Task<int> GetAttemptCount(string accountId, CancellationToken cancellationToken)
        {
            return await db.TemporaryPassword.AsNoTracking().
                         Where(u => u.AccountId == accountId).
                         OrderByDescending(u => u.AttemptCount).
                         Select(u => u.AttemptCount).
                         FirstOrDefaultAsync(cancellationToken);
        }
        
        public async Task<string> GetTempCodeId(string accountId, CancellationToken cancellationToken)
        {
            return await db.TemporaryPassword.AsNoTracking().
                         Where(u => u.AccountId == accountId).
                         OrderByDescending(u => u.AttemptCount).
                         Select(u => u.Id).
                         FirstAsync(cancellationToken); 
        }

        public async Task DeactivateOldCode(string tempId, CancellationToken cancellationToken)
        {
            
            var tempCode = new TemporaryPassword { Id = tempId };

            db.TemporaryPassword.Attach(tempCode);

            tempCode.IsActive = false;

            db.Entry(tempCode).Property(c => c.IsActive).IsModified = true;

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<TemporaryPassword>?> GetExpiredCodes(CancellationToken cancellationToken)
        {
            return await db.TemporaryPassword.AsNoTracking().
                         Where(u => u.IsExpired).
                         Select(u => u).
                         ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TemporaryPassword>?> GetUsedCodes(CancellationToken cancellationToken)
        {
            return await db.TemporaryPassword.AsNoTracking().
                         Where(u => u.IsActive == false).
                         Select(u => u).
                         ToListAsync(cancellationToken);
        }

        public async Task DeleteCodes(TemporaryPassword code, CancellationToken cancellationToken)
        {
            db.TemporaryPassword.Remove(code);
            await db.SaveChangesAsync(cancellationToken);
        }
        
    }

}