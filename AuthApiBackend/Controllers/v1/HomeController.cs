using AuthApiBackend.Configurations;
using AuthApiBackend.DTOs;
using AuthApiBackend.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthApiBackend.Controllers.v1
{
    public class HomeController : Controller
    {
        private readonly IUserService userService;
        private readonly IContactDetailsService contactService;
        private readonly IRoleService roleService;
        private readonly IUserRoleService userRoleService;
        private readonly IVerificationCodeService codeService;
        private readonly IAccountService accountService;
        private readonly MaxAttemptsConfig maxAttempts;

        public HomeController(IUserService userService, IContactDetailsService contactService, IRoleService roleService,
            IUserRoleService userRoleService, IVerificationCodeService codeService, IAccountService accountService
            , IOptions<MaxAttemptsConfig> maxAttempt)
        {

            this.contactService = contactService;
            this.userService = userService;
            this.roleService = roleService;
            this.userRoleService = userRoleService;
            this.codeService = codeService;
            this.accountService = accountService;
            this.maxAttempts = maxAttempt.Value;

        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto user, CancellationToken cancellationToken)
        {

            string userId = await userService.CreateUserAsync(user, cancellationToken);

            await contactService.CreateUserContactDetails(userId, user.Email, cancellationToken);

            int role = await roleService.GetRoleAsync("User", cancellationToken);

            await userRoleService.CreateUserRoleAsync(role, userId, cancellationToken);

            await codeService.CreateCodeAsync(userId, cancellationToken);

            await accountService.CreateAccountAsync(userId, user.Password, cancellationToken);

            return Ok(new { Message = "Please check your emails for comfirmation email with a code" });

        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyCode([FromBody] CodeVerificationDto code, CancellationToken cancellationToken)
        {

            string userId = await codeService.VerifyCodeAsync(code.CodeId, code.Code, cancellationToken);

            await contactService.UpdateIsEmailVerified(userId, cancellationToken);

            await codeService.UpdateCodeAsync(code.CodeId, cancellationToken);

            await accountService.UpdateAccountNumber(userId, cancellationToken);

            return Ok(new { Message = "Code verified successfully" });
        }

        [HttpPost("resend-code")]
        public async Task<IActionResult> ResendCode([FromBody] string idNumber, CancellationToken cancellationToken)
        {
            var userAttemptsAndUserId = await userService.GetUserIdAsync(idNumber, cancellationToken);

            await codeService.RequestForCode(userAttemptsAndUserId, cancellationToken);

            return Ok(new { Message = "A new code has been sent to your email." });
        }
    }
}
