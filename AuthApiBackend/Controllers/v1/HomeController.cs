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

    public class HomeController(ILogger<HomeController> logger) : ControllerBase
    {

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto user, CancellationToken cancellationToken,
            IAccountService accountService, IVerificationCodeService codeService, IUserRoleService userRoleService,
            IRoleService roleService, IUserService userService, IContactDetailsService contactService)
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
        public async Task<IActionResult> VerifyCode([FromBody] CodeVerificationDto code, CancellationToken cancellationToken,
            IAccountService accountService, IVerificationCodeService codeService, IContactDetailsService contactService)
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
        public async Task<IActionResult> ResendCode([FromBody] string idNumber, CancellationToken cancellationToken,
            IVerificationCodeService codeService, IUserService userService)
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
        public async Task<IActionResult> LoginFrom([FromBody] LoginDto user, CancellationToken cancellationToken,
            IAccountService accountService)
        {
            await accountService.VerifyLoginDetails(user, cancellationToken);

            return Ok("Logged in Successfully");
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto update, CancellationToken cancellationToken,
            IAccountService accountService)
        {

            await accountService.UpdatePassword(update, cancellationToken);

            return Ok("Password changed Successfully");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] string loginNumber, CancellationToken cancellationToken,
            IAccountService accountService, ITemporaryPasswordService tempPassword)
        {

            string accountId = await accountService.GetAccountId(loginNumber, cancellationToken);

            await tempPassword.CreateTemporaryPassword(accountId, cancellationToken);

            return Ok(new { Messagge = "Please check your email for temporary password" });
        }

        [HttpPost("reset-verify")]
        public async Task<IActionResult> VerifyTempPassword([FromBody] ResetPasswordDto resetPassword, CancellationToken cancellationToken,
            IAccountService accountService, ITemporaryPasswordService tempPassword)
        {

            var accountId = await tempPassword.VerifyPassword(resetPassword.TempPasswordId, resetPassword.TemporaryPassword, cancellationToken);
            
            await accountService.VerifyResetPassword(accountId, resetPassword.NewPassword, cancellationToken);

            await accountService.ResetPassword(accountId, resetPassword.NewPassword, cancellationToken);

            await tempPassword.UpdatePasswordStatus(resetPassword.TempPasswordId, cancellationToken);

            return Ok(new { Message = "Password resetted successfully" });
        }

        [HttpPost("forgot-loginNumber")]
        public async Task<IActionResult> RetrieveLoginNumber([FromBody] string nationalId, CancellationToken cancellationToken
            , IUserService userService)
        {

            await userService.FindUserLoginNumberById(nationalId, cancellationToken);

            return Ok(new { Message = "An email has been sent to ******@gmail.com" });
        }

    }

}