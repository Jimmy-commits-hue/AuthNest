using AuthApiBackend.Controllers.V1;
using AuthApiBackend.Controllers.V2;
using AuthApiBackend.Controllers.V3;
using AuthApiBackend.Interfaces.IOperations;
using AuthApiBackend.Interfaces.IRepositories;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Interfaces.IServices.ISendNotification;
using AuthApiBackend.Repositories;
using AuthApiBackend.Services;
using AuthApiBackend.Services.Operations;
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
            Services.AddScoped<ITemporaryPasswordService, TemporaryPasswordService>();

            //Transient Services
            Services.AddTransient<INotification, Notification>();

            //Repositories
            Services.AddScoped<IContactDetailsRepo, ContactDetailsRepo>();
            Services.AddScoped<IUserRepository, UserRepository>();
            Services.AddScoped<IRoleRepository, RoleRepository>();
            Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            Services.AddScoped<IVerificationCodeRepo, VerificationCodeRepo>();
            Services.AddScoped<IAccountRepository, AccountRepository>();
            Services.AddScoped<ITemporaryPasswordRepo,  TemporaryPasswordRepo>();

            //HostedServices
            Services.AddHostedService<BackgroundTask.SendVerificationCodeNotification>();
            Services.AddHostedService<BackgroundTask.SendAccountNumberNotification>();
            Services.AddHostedService<BackgroundTask.SendPasswordChangeNotification>();
            Services.AddHostedService<BackgroundTask.SendForgettenLoginNumber>();
            Services.AddHostedService<BackgroundTask.UnlockAccounts>();
            Services.AddHostedService<BackgroundTask.PermanentDeleteAccounts>();
            //Services.AddHostedService<BackgroundTask.CleanExpiredCodes>();
            //Services.AddHostedService<BackgroundTask.CleanUsedCodes>();

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

            //Operations Services
            Services.AddScoped<IRegistration, Registration>();
            Services.AddScoped<ICodeVerification, CodeVerification>();
            Services.AddScoped<ILoginOperation, LoginOperation>();
            Services.AddScoped<ICodeResend, CodeResend>();
            Services.AddScoped<IResetPassword, ResetPassword>();
            Services.AddScoped<IResetPasswordRequest, ResetPasswordRequest>();
            Services.AddScoped<ICancelDeletion, CancelDeletion>();
            Services.AddScoped<IDeleteRole,  DeleteRole>();

            Services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            return Services;
            
        }

    }

}
