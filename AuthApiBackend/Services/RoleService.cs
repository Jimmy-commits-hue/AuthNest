using AuthApiBackend.DTOs;
using AuthApiBackend.Models;
using AuthApiBackend.Exceptions.ExceptionTypes;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Interfaces.IServices;

namespace AuthApiBackend.Services
{
    public class RoleService(IRoleRepository roleRepo) : IRoleService
    {

        public async Task CreateRoleAsync(RoleDto role, CancellationToken cancellationToken)
        {

            var roleExist = await roleRepo.GetAsync(role.RoleName, cancellationToken);

            if(roleExist is not 0)
               throw new RoleAlreadyExistException($"{role.RoleName} role already exist");

            await roleRepo.CreateAsync(new Role
            {

                RoleName = role.RoleName,

            }, cancellationToken);

        }

        public async Task<int> GetRoleAsync(string role, CancellationToken cancellationToken) 
        {

            return await roleRepo.GetAsync(role, cancellationToken) ?? 
                   throw new NoRoleMatchException($"No role match for {role}");

        }

        //not complete
        public async Task<IEnumerable<Role>> GetAllRolesAsync(CancellationToken cancellationToken)
        {

            return await roleRepo.GetAllAsync(cancellationToken);

        } 

        //not complete
        public async Task UpdateRoleAsync(string role, string newRole, CancellationToken cancellationToken)
        {

            _ = await roleRepo.GetAsync(role, cancellationToken) ??
                 throw new NoRoleMatchException($"Failed to update. Role {role} does not exist");

        }
        
    }

}
