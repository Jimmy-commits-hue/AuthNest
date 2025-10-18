using Moq;
using AuthApiBackend.Services;
using AuthApiBackend.Models;
using AuthApiBackend.Interfaces.IRepositories;

namespace AuthApiBackendTest
{
    public class UserRoleServiceTest
    {

        private readonly Mock<IUserRoleRepository> userRoleRepo;
        private readonly UserRoleService userRoleService;

        public UserRoleServiceTest()
        {
            userRoleRepo = new Mock<IUserRoleRepository>();
            userRoleService = new UserRoleService(userRoleRepo.Object);
        }

        [Fact]
        public async Task CreateUserRoleAsync_ShouldCreateUserRole()
        {
            int roleId = 1;
            string userId = Guid.NewGuid().ToString();

            var userRole = new UserRole
            {
                RoleId = roleId,
                UserId = userId,
            };

            userRoleRepo.Setup(userRoleRepo => userRoleRepo.CreateAsync(userRole, CancellationToken.None))
                       .Returns(Task.CompletedTask);

            await userRoleService.CreateUserRoleAsync(roleId, userId, CancellationToken.None);

            userRoleRepo.Verify(userRoleRepo => userRoleRepo.CreateAsync(It.IsAny<UserRole>(), CancellationToken.None), Times.Once);
        }
    }
}
