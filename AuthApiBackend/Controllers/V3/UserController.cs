using AuthApiBackend.DTOs;
using AuthApiBackend.Interfaces.IOperations;
using AuthApiBackend.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;


namespace AuthApiBackend.Controllers.V3
{

    [Authorize(Policy = "User")]
    [Route("api/v{version:apiversion}/[controller]")]
    [ApiController]
    [EnableRateLimiting("AfterLogin")]
    public class UserController : ControllerBase
    {

        [HttpGet("welcome")]
        public IActionResult UserDashboard()
        {
            return Ok("Welcome");
        }

        [HttpPatch("update-details")]
        public async Task<IActionResult> UpdateDetails(string Id, [FromBody] JsonPatchDocument<UserPatchDetails> userPatch,
            IUserService userService, CancellationToken cancellationToken, IContactDetailsService contactDetails)
        {

            if (userPatch == null)
                return BadRequest("No fields to update");

            var user = new UserPatchDetails();

            userPatch.ApplyTo(user, ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (user.FirstName != null || user.Surname != null)
            {
                await userService.UpdateUserPartially(Id, userPatch, user, cancellationToken);
            }

            if (user.Email != null)
            {
                await contactDetails.UpdateEmail(Id, user.Email!, cancellationToken);
            }

            return Ok("Details Updated Successfully");
        }

        [HttpPut("Disable-account")]
        public async Task<IActionResult> DeactivateAccount(string loginNumber, CancellationToken cancellationToken, IAccountService accountService)
        {

            await accountService.DisableAccount(loginNumber, cancellationToken);

            return Ok("Account deactivated successfully");
        }

        [HttpPut("active-account")]
        public async Task<IActionResult> ActivateAccount(string userId, string loginNumber, CancellationToken cancellationToken,
            IAccountService accountService, IUserService userService)
        {
            var accountId = await userService.GetUserPkById(userId, cancellationToken);

            await accountService.EnableAccount(accountId, loginNumber, cancellationToken);

            return Ok(new { Message = "Account activated successfully" });
        }

        [HttpPut("delete-account")]
        public async Task<IActionResult> DeleteAccount(string loginNumber, CancellationToken cancellationToken, IAccountService accountService)
        {
            await accountService.ScheduleAccountForDeletion(loginNumber, cancellationToken);

            return Ok(new { Message = $"Account will be permanently deleted on {DateTime.UtcNow.AddDays(2).ToLocalTime()}" });
        }

        [HttpPut("cancel-account-deletion")]
        public async Task<IActionResult> CancelAccountScheduledDeletion([FromBody] LoginDto login, ICancelDeletion cancel,
            CancellationToken cancellationToken)
        {
            await cancel.CancelAccountDeletion(login, cancellationToken);

            return Ok(new { Message = "Account retrieved" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(ILogoutOperation logout,
            CancellationToken cancellationToken)
        {
            await logout.Logout(cancellationToken);

            return Ok(new { Message = "Logged out successfully" });
        }
    }
}
