using AuthApiBackend.DTOs;
using AuthApiBackend.Exceptions.ExceptionTypes;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Models;
using AuthApiBackend.Services;
using Moq;

namespace AuthApiBackendTest
{
    public class RoleServiceTest
    {

        private readonly Mock<IRoleRepository> roleRepo;
        private readonly RoleService roleService;

        public RoleServiceTest()
        {
            roleRepo = new Mock<IRoleRepository>();
            roleService = new RoleService(roleRepo.Object);
        }

        [Fact]
        public async Task CreateRoleAsync_ShouldCreateRole()
        {
            string roleName = "Admin";
            var role = new Role
            {
                RoleName = roleName,
            };

            roleRepo.Setup(roleRepo => roleRepo.GetAsync(roleName, CancellationToken.None))
                    .ReturnsAsync(0);

            roleRepo.Setup(roleRepo => roleRepo.CreateAsync(role, CancellationToken.None))
                    .Returns(Task.CompletedTask);

            await roleService.CreateRoleAsync(new RoleDto { RoleName = roleName }, CancellationToken.None);

            roleRepo.Verify(roleRepo => roleRepo.CreateAsync(It.IsAny<Role>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task CreateRoleAsync_ThrowsExpection_IfRoleAlreadyExist()
        {

            string roleName = "Admin";

            roleRepo.Setup(roleRepo => roleRepo.GetAsync(roleName, CancellationToken.None))
                    .ReturnsAsync(1);

            var ex = await Assert.ThrowsAsync<RoleAlreadyExistException>(() =>
                roleService.CreateRoleAsync(new RoleDto { RoleName = roleName }, CancellationToken.None)
            );

            Assert.Equal($"{roleName} role already exist", ex.Message);

            roleRepo.Verify(roleRepo => roleRepo.GetAsync(roleName, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetRoleAsync_ShouldReturnRole_IfExists()
        {
            int roleId = 1;
            string roleName = "Admin";
            roleRepo.Setup(roleRepo => roleRepo.GetAsync(roleName, CancellationToken.None))
                    .ReturnsAsync(roleId);

            var result = await roleService.GetRoleAsync(roleName, CancellationToken.None);

            Assert.IsType<int>(result);

            Assert.Equal(roleId, result);

            roleRepo.Verify(roleRepo => roleRepo.GetAsync(It.IsAny<string>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task GetRoleAsync_ThrowsRoleNotFoundException_IfRoleDoesNotExist()
        {
            string roleName = "Admin";

            roleRepo.Setup(roleRepo => roleRepo.GetAsync(roleName, CancellationToken.None))
                    .ReturnsAsync((int?)null);

            var ex = await Assert.ThrowsAsync<NoRoleMatchException>(() =>
                roleService.GetRoleAsync(roleName, CancellationToken.None)
            );

            Assert.Equal($"No role match for {roleName}", ex.Message);

            roleRepo.Verify(roleRepo => roleRepo.GetAsync(roleName, CancellationToken.None), Times.Once);
        }

    }
}
