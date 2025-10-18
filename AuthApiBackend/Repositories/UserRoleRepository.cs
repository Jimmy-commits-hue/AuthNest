using AuthApiBackend.Database;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApiBackend.Repositories
{
    public class UserRoleRepository(AuthApiDbContext context) : IUserRoleRepository
    {

        public async Task CreateAsync(UserRole userRole, CancellationToken cancellationToken)
        {

            context.UserRole.Add(userRole);
            await context.SaveChangesAsync(cancellationToken);

        }

        public async Task<int?> GetAsync(string userId, CancellationToken cancellationToken)
        {

            return await context.UserRole.AsNoTracking().Where(u => u.UserId.CompareTo(userId) == 0).
                                 Select(u => u.RoleId).FirstOrDefaultAsync(cancellationToken);

        }

        public async Task UpdateAsync(UserRole userRole, CancellationToken cancellationToken)
        {

            context.UserRole.Update(userRole);
            await context.SaveChangesAsync(cancellationToken);

        }

    }
}
