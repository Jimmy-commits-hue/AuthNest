using AuthApiBackend.DTOs;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Interfaces.IOperations;
using AuthApiBackend.Interfaces.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthApiBackend.Controllers.V1
{

    [AllowAnonymous]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]

    public class HomeController : ControllerBase
    {

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto user, IRegistration registration, 
            CancellationToken cancellationToken)
        {
            await registration.Register(user, cancellationToken);

            return Ok(new { Message = "Please check your emails for comfirmation email with a code" });
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyCode([FromBody] CodeVerificationDto code, ICodeVerification verify,
            CancellationToken cancellationToken)
        {
            await verify.VerifyCode(code, cancellationToken);

            return Ok(new { Message = "Code verified successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto user, ILoginOperation login, CancellationToken cancellationToken)
        {

            await login.Login(user, cancellationToken);

            return Ok("Logged in Successfully");
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
        public async Task<IActionResult> ResendCode([FromBody] string idNumber, ICodeResend codeResend, CancellationToken cancellationToken)
        {
            await codeResend.ResendCode(idNumber, cancellationToken);

            return Ok(new { Message = "A new code has been sent to your email." });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto update, CancellationToken cancellationToken,
            IAccountService accountService)
        {
            await accountService.UpdatePassword(update, cancellationToken);

            return Ok("Password changed Successfully");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordRequest([FromBody] PasswordReset passwordReset, IResetPasswordRequest reset,
            CancellationToken cancellationToken)
        {
            await reset.RequestResetPassword(passwordReset, cancellationToken);

            return Ok(new { Messagge = "Please check your email for temporary password" });
        }

        [HttpPost("reset-verify")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPassword, IResetPassword reset,
            CancellationToken cancellationToken)
        {
            await reset.PasswordReset(resetPassword, cancellationToken);

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