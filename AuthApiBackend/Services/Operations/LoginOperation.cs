using AuthApiBackend.DTOs;
using AuthApiBackend.Interfaces.IOperations;
using AuthApiBackend.Interfaces.IServices;
using AuthApiBackend.Security;
using Serilog.Context;

namespace AuthApiBackend.Services.Operations
{
    public class LoginOperation : ILoginOperation
    {

        private readonly IAccountService accountService;
        private readonly ILogger<LoginOperation> logger;
        private readonly GenerateJwtToken jwtToken;
        private readonly IRefreshTokenService tokenService;

        public LoginOperation(IAccountService accountService, ILogger<LoginOperation> logger, GenerateJwtToken jwtToken, 
            IRefreshTokenService tokenService)
        {
            this.accountService = accountService;
            this.logger = logger;
            this.jwtToken = jwtToken;
            this.tokenService = tokenService;
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

                var response = await accountService.GetAccountUserDeatailsUponLogin(accountId, cancellationToken);

                var refreshToken = jwtToken.GenerateToken(accountId, response.FirstName, response.Surname, response.Role);

                await tokenService.CreateRefreshToken(accountId, refreshToken, cancellationToken);

                logger.LogInformation("{AccountId} logged in successfully at {DateTime}", accountId, DateTime.UtcNow.ToLocalTime());
            }
        }
    }
}
