using AuthApiBackend.Database;
using AuthApiBackend.DTOs;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace AuthApiBackend.Repositories
{

    public class UserRepository(AuthApiDbContext db) : IUserRepository
    {

        public async Task CreateAsync(User user, CancellationToken cancellationToken)
        {
            db.User.Add(user);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<UserResponse?> GetAsync(string idNumber, CancellationToken cancellationToken)
        {
            return await db.User.AsNoTracking().
                         Where(c => c.IdNumber == idNumber).
                         Select(c => new UserResponse(
                                     c.Id,
                                     c.ContactDetails!.VerificationCode.
                                     OrderByDescending(c => c.AttemptCount).
                                     Select(c => c.AttemptCount).First())).
                         FirstOrDefaultAsync(cancellationToken);
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken)
        {
            db.User.Update(user);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(User user, CancellationToken cancellationToken)
        {
            db.User.Remove(user);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<ForgottenLoginNumber?> GetUserId(string nationalId,  CancellationToken cancellationToken)
        {
            return await db.User.AsNoTracking().
                         Where(u => u.IdNumber == nationalId).
                         Select(u => new ForgottenLoginNumber(
                                     u.Account!.AccountNumber,
                                     u.ContactDetails!.Email)).
                         FirstOrDefaultAsync(cancellationToken);
        }

        public async Task PatchUserDetails(string id, JsonPatchDocument<UserPatchDetails> patchDetails, UserPatchDetails user, 
            CancellationToken cancellationToken)
        {

            var updateUser = new User { Id = id };

            db.User.Attach(updateUser);

            foreach(var op in patchDetails.Operations)
            {
                switch (op.path.ToLower())
                {
                    case "/firstname": 
                         updateUser.FirstName = user.FirstName!;
                        db.Entry(updateUser).Property(c => c.FirstName).IsModified = true;
                        break;

                    case "/surname":
                        updateUser.Surname = user.Surname!;
                        db.Entry(updateUser).Property(c => c.Surname).IsModified = true;
                        break;

                }
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<string?> GetUserPkById(string nationalId, CancellationToken cancellationToken)
        {
            return await db.User.AsNoTracking().
                         Where(c => nationalId == c.IdNumber).
                         Select(c => c.Id).
                         FirstOrDefaultAsync(cancellationToken);
        }

        public async Task DeleteUser(User user, CancellationToken cancellationToken)
        {
            db.User.Remove(user);
            await db.SaveChangesAsync(cancellationToken);
        }

    }

}