using AuthApiBackend.DTOs;
using AuthApiBackend.Interfaces.IOperations;
using AuthApiBackend.Interfaces.IServices;
using Serilog.Context;

namespace AuthApiBackend.Services.Operations
{

    public class ResetPassword : IResetPassword
    {

        private readonly ITemporaryPasswordService tempPassword;
        private readonly IAccountService accountService;
        private readonly ILogger<ResetPassword> logger;

        public ResetPassword(ITemporaryPasswordService tempPassword, IAccountService accountService, ILogger<ResetPassword> logger)
        {
            this.tempPassword = tempPassword;
            this.accountService = accountService;   
            this.logger = logger;
        }

        public async Task PasswordReset(ResetPasswordDto verifyPassword, CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("Operation", nameof(Operations.ResetPassword)))
            {
                var accountId = await tempPassword.VerifyPassword(verifyPassword.TempPasswordId, verifyPassword.TemporaryPassword, cancellationToken);

                await accountService.VerifyResetPassword(accountId, verifyPassword.NewPassword, cancellationToken);

                await accountService.ResetPassword(accountId, verifyPassword.NewPassword, cancellationToken);

                await tempPassword.UpdatePasswordStatus(verifyPassword.TempPasswordId, cancellationToken);

                logger.LogInformation("Password resetted successfully for {AccountId}", accountId);
            }
        }

    }

}