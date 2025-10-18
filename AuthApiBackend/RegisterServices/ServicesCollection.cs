using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Interfaces.IServices.ISendNotification;
using AuthApiBackend.Repositories;
using AuthApiBackend.Services;

namespace AuthApiBackend.RegisterServices
{
    public static class ServicesCollection
    {

        public static IServiceCollection AddServiceCollection(this IServiceCollection Services)
        {

            //Scoped Services
            Services.AddScoped<IContactDetailsService, ContactDetailsService>();
            Services.AddScoped<IUserService, UserService>();
            Services.AddScoped<IRoleService, RoleService>();
            Services.AddScoped<IUserRoleService, UserRoleService>();
            Services.AddScoped<IVerificationCodeService, VerificationCodeService>();
            Services.AddScoped<IAccountService, AccountService>();

            //Transient Services
            Services.AddTransient<INotification, Notification>();

            //Repositories
            Services.AddScoped<IContactDetailsRepo, ContactDetailsRepo>();
            Services.AddScoped<IUserRepository, UserRepository>();
            Services.AddScoped<IRoleRepository, RoleRepository>();
            Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            Services.AddScoped<IVerificationCodeRepo, VerificationCodeRepo>();
            Services.AddScoped<IAccountRepository, AccountRepository>();

            //HostedServices
            Services.AddHostedService<BackgroundTask.SendNotifications>();
            Services.AddHostedService<BackgroundTask.SendAccountNumber>();

            //HttpContextAccessor Service
            Services.AddHttpContextAccessor();


            return Services;
            
        }

    }

}
