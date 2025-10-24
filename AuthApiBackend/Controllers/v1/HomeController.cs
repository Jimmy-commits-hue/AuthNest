using AuthApiBackend.Configurations;
using AuthApiBackend.DTOs;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Interfaces.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog.Context;
using System.Security.Claims;

namespace AuthApiBackend.Controllers.V1
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]

    public class HomeController : ControllerBase
    {

        private readonly IUserService userService;
        private readonly IContactDetailsService contactService;
        private readonly IRoleService roleService;
        private readonly IUserRoleService userRoleService;
        private readonly IVerificationCodeService codeService;
        private readonly IAccountService accountService;
        private readonly MaxAttemptsConfig maxAttempts;
        private readonly ILogger<HomeController> logger;

        public HomeController(IUserService userService, IContactDetailsService contactService, IRoleService roleService,
            IUserRoleService userRoleService, IVerificationCodeService codeService, IAccountService accountService
            , IOptions<MaxAttemptsConfig> maxAttempt, ILogger<HomeController> logger)
        {

            this.contactService = contactService;
            this.userService = userService;
            this.roleService = roleService;
            this.userRoleService = userRoleService;
            this.codeService = codeService;
            this.accountService = accountService;
            this.maxAttempts = maxAttempt.Value;
            this.logger = logger;

        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto user, CancellationToken cancellationToken)
        {

            using (LogContext.PushProperty("Operation", "Registration"))
            {

                string userId = await userService.CreateUserAsync(user, cancellationToken);

                await contactService.CreateUserContactDetails(userId, user.Email, cancellationToken);

                int role = await roleService.GetRoleAsync("User", cancellationToken);

                await userRoleService.CreateUserRoleAsync(role, userId, cancellationToken);

                await codeService.CreateCodeAsync(userId, cancellationToken);

                await accountService.CreateAccountAsync(userId, user.Password, cancellationToken);

                logger.LogInformation("A verification Code for {UserId} was sent to {Email}", userId, user.Email);

            }

            return Ok(new { Message = "Please check your emails for comfirmation email with a code" });

        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyCode([FromBody] CodeVerificationDto code, CancellationToken cancellationToken)
        {

            using (LogContext.PushProperty("Operation", "CodeVerification"))
            {

                string userId = await codeService.VerifyCodeAsync(code.CodeId, code.Code, cancellationToken);

                await contactService.UpdateIsEmailVerified(userId, cancellationToken);

                await codeService.UpdateCodeAsync(code.CodeId, cancellationToken);

                await accountService.UpdateAccountNumber(userId, cancellationToken);

                logger.LogInformation("Code for {UserId} was verifyed successfully", userId);

            }

            return Ok(new { Message = "Code verified successfully" });

        }

        [HttpGet("googleRegistration")]
        public IActionResult Login()
        {
            
            return Challenge(new AuthenticationProperties { RedirectUri = "/api/v1/home/google-callback" },
                             GoogleDefaults.AuthenticationScheme);

        }

        [Authorize]
        [HttpGet("google-callback")]
        public IActionResult GetUserInfo()
        {
            
            var googleUser = new GoogleResponse
            {
                Surname = User.FindFirstValue(ClaimTypes.Surname),
                GivenName = User.FindFirstValue(ClaimTypes.GivenName),
                Email = User.FindFirstValue(ClaimTypes.Email)!
            };

            return Ok(googleUser);

        }

        [HttpPost("resend-code")]
        public async Task<IActionResult> ResendCode([FromBody] string idNumber, CancellationToken cancellationToken)
        {

            using (LogContext.PushProperty("Operation", "CodeRequest"))
            {

                var userAttemptsAndUserId = await userService.GetUserIdAsync(idNumber, cancellationToken);

                await codeService.RequestForCode(userAttemptsAndUserId, cancellationToken);

                logger.LogInformation("New verification code was sent for {UserId}", userAttemptsAndUserId.UserId);

            }

            return Ok(new { Message = "A new code has been sent to your email." });

        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginFrom([FromBody] LoginDto user)
        {

            return Ok("Logged in Successfully");
        }

    }

}

