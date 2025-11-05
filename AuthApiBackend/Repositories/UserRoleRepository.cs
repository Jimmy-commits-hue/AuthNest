using AuthApiBackend.Database;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApiBackend.Repositories
{
    public class UserRoleRepository(AuthApiDbContext db) : IUserRoleRepository
    {

        public async Task CreateAsync(UserRole userRole, CancellationToken cancellationToken)
        {
            db.UserRole.Add(userRole);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<int?> GetAsync(string userRoleId, CancellationToken cancellationToken)
        {
            return await db.UserRole.AsNoTracking().
                         Where(u => u.Id == userRoleId).
                         Select(u => u.RoleId).
                         FirstOrDefaultAsync(cancellationToken);
        }

        public async Task UpdateAsync(UserRole userRole, CancellationToken cancellationToken)
        {
            db.UserRole.Update(userRole);
            await db.SaveChangesAsync(cancellationToken);
        }

    }

}