using AuthApiBackend.Database;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace AuthApiBackend.Repositories
{
    public class VerificationCodeRepo : IVerificationCodeRepo
    {

        private readonly AuthApiDbContext db;
        //private readonly IDistributedCache cache;
       

        public VerificationCodeRepo(AuthApiDbContext db, IDistributedCache cache)
        {
            this.db = db;
          //  this.cache = cache;
           
        }

        public async Task CreateAsync(VerificationCode code, CancellationToken cancellationToken)
        {
            db.VerificationCode.Add(code);
            await db.SaveChangesAsync(cancellationToken);

           /* await cache.SetStringAsync(code.Id, JsonSerializer.Serialize(new VerificationResponse { Code = code.Code
            , IsExpired = code.IsExpired, UserId = code.ContactDetails.User.Id}), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            }, cancellationToken);

            await database.ListLeftPushAsync("PendingCodes", JsonSerializer.Serialize(new PendingCode
            {
                Id = code.Id,
                Code = code.Code,
                Email = code.ContactDetails.Email,
                FirstName = code.ContactDetails.User.FirstName,
                Surname = code.ContactDetails.User.Surname
            }));*/
        }

        public async Task<VerificationResponse?> GetAsync(string codeId, CancellationToken cancellationToken)
        {
            /*var code = await cache.GetStringAsync(codeId, cancellationToken);

            if (!string.IsNullOrEmpty(code))
            {
                return JsonSerializer.Deserialize<VerificationResponse>(code);
            }*/

            return await db.VerificationCode.Where(c => c.Id == codeId).OrderByDescending(c => c.AttemptCount).
                         AsNoTracking().Select(c => new VerificationResponse{ Code = c.Code, IsExpired = c.IsExpired, 
                         UserId = c.ContactDetails.User.Id }).FirstOrDefaultAsync(cancellationToken);

        }

        public async Task<IEnumerable<PendingCode>?> GetPendingCodes(CancellationToken cancellationToken)
        {

            /*var pendingCodes = await database.ListLeftPopAsync("PendingCodes");

            if (!string.IsNullOrEmpty(pendingCodes))
            {
                return JsonSerializer.Deserialize<IEnumerable<PendingCode>>(pendingCodes!);
            }*/

            return await db.VerificationCode.
                    Where(v => v.IsActive == true && !v.IsEmailSent).AsNoTracking().Select(v => new PendingCode{ Code = v.Code, Id = v.Id,
                       Email = v.ContactDetails.Email, FirstName = v.ContactDetails.User.FirstName, Surname = v.ContactDetails.User.Surname
                    }).ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(VerificationCode code, CancellationToken cancellationToken)
        {
            db.VerificationCode.Update(code);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> IsUserEmailVerified(string userId, CancellationToken cancellationToken)
        {
            
            return await db.VerificationCode.Where(c => c.ContactDetails.User.Id.CompareTo(userId) == 0).
                        Select(c => c.ContactDetails.IsEmailVerified).FirstOrDefaultAsync(cancellationToken);

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

    }
}
