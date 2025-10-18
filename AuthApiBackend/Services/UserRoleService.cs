using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Interfaces.IServices;

namespace AuthApiBackend.Services
{
    public class UserRoleService(IUserRoleRepository userRoleRepo) : IUserRoleService
    {

        public async Task CreateUserRoleAsync(int roleId, string userId, CancellationToken cancellationToken)
        {

            await userRoleRepo.CreateAsync(new Models.UserRole
            {

                UserId = userId,
                RoleId = roleId,

            }, cancellationToken);

        }

    }
}
