using AuthApiBackend.DTOs;
using AuthApiBackend.Interfaces.IOperations;
using AuthApiBackend.Interfaces.IServices;
using Serilog.Context;

namespace AuthApiBackend.Services.Operations
{
    public class LoginOperation : ILoginOperation
    {

        private readonly IAccountService accountService;
        private readonly ILogger<LoginOperation> logger;

        public LoginOperation(IAccountService accountService, ILogger<LoginOperation> logger)
        {
            this.accountService = accountService;
            this.logger = logger;
        }

        public async Task Login(LoginDto login, CancellationToken cancellationToken)
        {
            using(LogContext.PushProperty("Operation", nameof(Login)))
            {
                var hashedPassword = await accountService.VerifyLoginNumber(login.LoginNumber, cancellationToken);
                var accountId = await accountService.GetAccountId(login.LoginNumber, cancellationToken);
                var attemptCount = await accountService.VerifyAccountStatus(login.LoginNumber, cancellationToken);

                await accountService.VerifyAttemptNumber(accountId, attemptCount, cancellationToken);

                await accountService.VerifyPassword(accountId, hashedPassword, login.Password, attemptCount, cancellationToken);

                logger.LogInformation("{AccountId} logged in successfully at {DateTime}", accountId, DateTime.UtcNow.ToLocalTime());
            }
        }
    }
}
