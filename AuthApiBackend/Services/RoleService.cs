using AuthApiBackend.DTOs;
using AuthApiBackend.Models;
using AuthApiBackend.Exceptions.ExceptionTypes;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Utilities;

namespace AuthApiBackend.Services
{
    public class RoleService(IRoleRepository roleRepo) : IRoleService
    {

        public async Task CreateRoleAsync(RoleDto role, CancellationToken cancellationToken)
        {
            var roleFormat = RoleFormat.Format(role.RoleName.ToString());

            var roleExist = await roleRepo.GetAsync(roleFormat, cancellationToken);

            if(roleExist is not 0)
               throw new RoleAlreadyExistException($"{role.RoleName} role already exist");

            await roleRepo.CreateAsync(new Role
            {

                RoleName = roleFormat,

            }, cancellationToken);

        }

        public async Task<int> GetRoleId(string role, CancellationToken cancellationToken) 
        {
            return await roleRepo.GetAsync(RoleFormat.Format(role), cancellationToken) ?? 
                   throw new NoRoleMatchException($"No role match for {role}");
        }

        public async Task<Role> GetRole(int roleId, CancellationToken cancellationToken)
        {
            return await roleRepo.GetRole(roleId, cancellationToken);
        }

        public async Task DeleteRole(Role role, CancellationToken cancellationToken)
        {
            await roleRepo.DeleteAsync(role, cancellationToken);
        }
        
    }

}