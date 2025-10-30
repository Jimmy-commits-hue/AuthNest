using AuthApiBackend.Database;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AuthApiBackend.Repositories
{
    public class TemporaryPasswordRepo(AuthApiDbContext db) : ITemporaryPasswordRepo
    {

        public async Task CreatePassword(TemporaryPassword temp, CancellationToken cancellationToken)
        {
            
            db.TemporaryPassword.Add(temp);
            await db.SaveChangesAsync();
           
        }

        public async Task<PasswordResetResponse> GetPassword(string tempPassowrdId, CancellationToken cancellationToken)
        {

           return await db.TemporaryPassword.AsNoTracking().Where(u => u.Id == tempPassowrdId).Select(p => new PasswordResetResponse(
                p.HashedPassword,
                p.AccountId)).FirstAsync(cancellationToken);
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

            return await db.TemporaryPassword.AsNoTracking().Where(u => u.IsActive == true && u.IsEmailSent == false).
                           Select(p => new ResetPasswordResponse
                           (
                             p.Account.User.ContactDetails!.Email,
                             p.HashedPassword,
                             p.Id
                           )).ToListAsync(cancellationToken);
        } 
    }
}
