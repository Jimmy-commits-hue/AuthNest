using AuthApiBackend.DTOs;
using AuthApiBackend.Interfaces.IOperations;
using AuthApiBackend.Interfaces.IServices;
using Serilog.Context;

namespace AuthApiBackend.Services.Operations
{

    public class Registration : IRegistration
    {

        private readonly IUserService userService;
        private readonly IContactDetailsService contactService;
        private readonly IRoleService roleService;
        private readonly IUserRoleService userRoleService;
        private readonly IVerificationCodeService codeService;
        private readonly IAccountService accountService;
        private ILogger<Registration> logger;

        public Registration(IUserService userService, IContactDetailsService contactService, IRoleService roleService,
            IUserRoleService userRoleService, IVerificationCodeService codeService, IAccountService accountService, 
            ILogger<Registration> logger)
        {
            this.userService = userService;
            this.contactService = contactService;
            this.roleService = roleService;
            this.userRoleService = userRoleService;
            this.codeService = codeService;
            this.accountService = accountService;
            this.logger = logger;
        }

        public async Task Register(RegisterDto user, CancellationToken cancellationToken) 
        {

            using (LogContext.PushProperty("Operation", "Registration"))
            {

                string userId = await userService.CreateUserAsync(user, cancellationToken);

                await contactService.CreateUserContactDetails(userId, user.Email, cancellationToken);

                int role = await roleService.GetRoleId("User", cancellationToken);

                await userRoleService.CreateUserRoleAsync(role, userId, cancellationToken);

                await codeService.CreateCodeAsync(userId, cancellationToken);

                await accountService.CreateAccountAsync(userId, user.Password, cancellationToken);

                logger.LogInformation("A verification Code for {UserId} was sent to {Email}", userId, user.Email);

            }

        }

    }

}