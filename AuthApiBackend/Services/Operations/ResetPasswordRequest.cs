using AuthApiBackend.DTOs;
using AuthApiBackend.Interfaces.IOperations;
using AuthApiBackend.Interfaces.IServices;
using Serilog.Context;

namespace AuthApiBackend.Services.Operations
{
    public class ResetPasswordRequest : IResetPasswordRequest
    {
        private readonly IUserService userService;
        private readonly IAccountService accountService;
        private readonly ITemporaryPasswordService tempPassword;
        private readonly ILogger<ResetPasswordRequest> logger;
 
        public ResetPasswordRequest(IUserService userService, IAccountService accountService, ITemporaryPasswordService tempPassword, 
            ILogger<ResetPasswordRequest> logger)
        {
            this.userService = userService;
            this.accountService = accountService;
            this.tempPassword = tempPassword;
            this.logger = logger;
        }

        public async Task RequestResetPassword(PasswordReset resetPassword, CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("Operation", nameof(ResetPasswordRequest)))
            {
                _ = await userService.GetUserPkById(resetPassword.IdNumber, cancellationToken);

                string accountId = await accountService.GetAccountId(resetPassword.LoginNumber, cancellationToken);

                int attemptCount = await tempPassword.CheckAttemptNumber(accountId, cancellationToken);

                await tempPassword.CreateTemporaryPassword(accountId, attemptCount, cancellationToken);

                logger.LogInformation("Password Reset Requested by {AccountId} on {DateTime}", accountId, DateTime.UtcNow.ToLocalTime());
            }
        }
    }
}
