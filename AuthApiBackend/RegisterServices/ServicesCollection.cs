using AuthApiBackend.Controllers.V1;
using AuthApiBackend.Controllers.V2;
using AuthApiBackend.Controllers.V3;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Interfaces.IServices.ISendNotification;
using AuthApiBackend.Repositories;
using AuthApiBackend.Services;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;

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

            //api versionning Service
            Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();

                options.Conventions.Controller<HomeController>().HasApiVersion(1, 0);
                options.Conventions.Controller<AdminController>().HasApiVersion(2, 0);
                options.Conventions.Controller<UserController>().HasApiVersion(3, 0);
            });

            Services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            return Services;
            
        }

    }

}
