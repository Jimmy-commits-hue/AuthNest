using AuthApiBackend.DTOs;
using AuthApiBackend.DTOs.ResponseDtos;
using AuthApiBackend.Interfaces.IOperations;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
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

        [Authorize]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(IAccountService accountService, GenerateJwtToken toke, 
            IRefreshTokenService tokenService,CancellationToken cancellationToken)
        {
            var refreshToken = HttpContext.Request.Cookies.TryGetValue("refreshToken", out var token) ? token : null;

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized("No refresh token provided");
            }

            var refreshTokenObject = await tokenService.GetRefreshToken(refreshToken, cancellationToken);

            var userDetails = await accountService.GetAccountUserDeatailsUponLogin(refreshTokenObject.AccountId, cancellationToken);

            var newRefreshToken = toke.GenerateToken(refreshTokenObject.AccountId, userDetails.FirstName, userDetails.Surname, userDetails.Role);

            await tokenService.DeleteRefreshToken(refreshTokenObject, cancellationToken);

            await tokenService.CreateRefreshToken(refreshTokenObject.AccountId, newRefreshToken, cancellationToken);

            return Ok(new { Message = "Token refreshed successfully" });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(IRefreshTokenService tokenService, IBlackListedTokenService blackListService,
            CancellationToken cancellationToken)
        {
            HttpContext.Request.Cookies.TryGetValue("refreshToken", out var token);
            HttpContext.Response.Cookies.Delete("refreshToken");

            HttpContext.Request.Cookies.TryGetValue("accessToken", out var accessToken);
            HttpContext.Response.Cookies.Delete("accessToken");
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken!);
            var validTo = new DateTimeOffset(jwtToken.ValidTo).ToUnixTimeSeconds();
            var jti = jwtToken.Id;

            var refreshTokenObject = await tokenService.GetRefreshToken(token!, cancellationToken);

            if (refreshTokenObject != null)
            {
                await tokenService.DeleteRefreshToken(refreshTokenObject, cancellationToken);
            }

            await blackListService.AddBlackListedToken(jti, validTo, cancellationToken);

            return Ok(new { Message = "Logged out successfully" });
        }

    }

}