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
            string roleName = "admin";
            var role = new Role
            {
                RoleName = roleName,
            };

            var fakeDb = new List<Role>();

            roleRepo.Setup(roleRepo => roleRepo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     ReturnsAsync(0);

            roleRepo.Setup(roleRepo => roleRepo.CreateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>())).
                     Callback<Role, CancellationToken>((newRole, cancellationToken) =>
                     {
                         fakeDb.Add(newRole);
                         roleName = newRole.RoleName;
                     }).
                     Returns(Task.CompletedTask);

            await roleService.CreateRoleAsync(new RoleDto { RoleName = roleName }, CancellationToken.None);

            Assert.Single(fakeDb);

            #pragma warning disable CS8602
            bool firstLetter = fakeDb.First().ToString().StartsWith("A");
            #pragma warning restore CS8602

            Assert.Equal("Admin", roleName);
            Assert.True(firstLetter);


            roleRepo.Verify(roleRepo => roleRepo.CreateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateRoleAsync_ThrowsExpection_IfRoleAlreadyExist()
        {
            string roleName = "Admin";

            roleRepo.Setup(roleRepo => roleRepo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     ReturnsAsync(1);

            var ex = await Assert.ThrowsAsync<RoleAlreadyExistException>(async () =>
                     await roleService.CreateRoleAsync(new RoleDto { RoleName = roleName }, CancellationToken.None));

            Assert.Equal($"{roleName} role already exist", ex.Message);

            roleRepo.Verify(roleRepo => roleRepo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetRoleAsync_ShouldReturnRole_IfExists()
        {
            int roleId = 1;
            string roleName = "Admin";

            roleRepo.Setup(roleRepo => roleRepo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).
                     ReturnsAsync(roleId);

            var result = await roleService.GetRoleId(roleName, CancellationToken.None);

            Assert.IsType<int>(result);

            Assert.Equal(roleId, result);

            roleRepo.Verify(roleRepo => roleRepo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetRoleAsync_ThrowsRoleNotFoundException_IfRoleDoesNotExist()
        {
            string roleName = "Admin";

            roleRepo.Setup(roleRepo => roleRepo.GetAsync(roleName, CancellationToken.None))
                    .ReturnsAsync((int?)null);

            var ex = await Assert.ThrowsAsync<NoRoleMatchException>(async () =>
                     await roleService.GetRoleId(roleName, CancellationToken.None));

            Assert.Equal($"No role match for {roleName}", ex.Message);

            roleRepo.Verify(roleRepo => roleRepo.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetRole()
        {
            roleRepo.Setup(c => c.GetRole(It.IsAny<int>(), It.IsAny<CancellationToken>())).
                     ReturnsAsync(new Role { Id = 1, RoleName = "Admin"});

            var role = await roleService.GetRole(1, CancellationToken.None);

            Assert.IsType<Role>(role);

            roleRepo.Verify(c => c.GetRole(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteRole()
        {
            var userRole = new Role{ Id = 1, RoleName = "Admin" };

            var fakeDb = new List<Role> { userRole };

            roleRepo.Setup(c => c.DeleteAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>())).
                     Callback<Role, CancellationToken>((role, cancellationToken) =>
                     {
                         fakeDb.Remove(userRole);
                     }).
                     Returns(Task.CompletedTask);

            await roleService.DeleteRole(userRole, CancellationToken.None);

            Assert.Empty(fakeDb);

            roleRepo.Verify(c => c.DeleteAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Once);
        }

    }

}