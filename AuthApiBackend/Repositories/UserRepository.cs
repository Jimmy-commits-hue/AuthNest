using AuthApiBackend.Database;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApiBackend.Repositories
{

    public class UserRepository(AuthApiDbContext context) : IUserRepository
    {
        public async Task CreateAsync(User user, CancellationToken cancellationToken)
        {

            context.User.Add(user);
            await context.SaveChangesAsync(cancellationToken);


        }

        
        public async Task<UserResponse?> GetAsync(string idNumber, CancellationToken cancellationToken)
        {

            return await context.User
                .Where(c => c.IdNumber == idNumber).AsNoTracking()
                .Select(c => new UserResponse
                {
                    UserId = c.Id,
                    AttemptCount = c.ContactDetails!.VerificationCode.OrderByDescending(c => c.AttemptCount).
                 Select(c => c.AttemptCount).First()
                })
                .FirstOrDefaultAsync(cancellationToken);

        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken)
        {

            context.User.Update(user);
            await context.SaveChangesAsync(cancellationToken);

        }

        public async Task DeleteAsync(User user, CancellationToken cancellationToken)
        {

            context.User.Remove(user);
            await context.SaveChangesAsync(cancellationToken);

        }

    }

}
